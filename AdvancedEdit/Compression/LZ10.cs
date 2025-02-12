using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AdvancedEdit.Compression;

public static class Lz10
{
    public static ReadOnlySpan<byte> Decompress(BinaryReader reader)
    {
        // Format constants
        const int bufferLength = 0x1000;

        // Read and validate the type byte
        byte type = reader.ReadByte();
        if (type != 0x10)
            throw new InvalidDataException($"Invalid LZ-0x10 compressed stream @ {reader.BaseStream.Position:X}");

        // Read decompressed size
        var sizeBytes = reader.ReadBytes(3);
        int decompressedSize = sizeBytes[0] | (sizeBytes[1] << 8) | (sizeBytes[2] << 16);
        if (decompressedSize == 0)
        {
            decompressedSize = reader.ReadInt32();
        }

        // Initialize buffer and output memory
        byte[] buffer = new byte[bufferLength];
        int bufferOffset = 0;
        byte[] output = new byte[decompressedSize];
        int currentOutSize = 0;

        // Decompression loop
        int flags = 0, mask = 1;
        while (currentOutSize < decompressedSize)
        {
            // Update mask and read new flags byte if needed
            if (mask == 1)
            {
                flags = reader.ReadByte();
                mask = 0x80;
            }
            else
            {
                mask >>= 1;
            }

            if ((flags & mask) > 0) // Compressed block
            {
                int byte1 = reader.ReadByte();
                int byte2 = reader.ReadByte();

                int length = (byte1 >> 4) + 3;
                int disp = ((byte1 & 0x0F) << 8) | byte2;
                disp += 1;

                if (disp > currentOutSize)
                    throw new InvalidDataException("Invalid displacement.");

                int bufIdx = bufferOffset + bufferLength - disp;
                for (int i = 0; i < length; i++)
                {
                    byte next = buffer[bufIdx % bufferLength];
                    bufIdx++;

                    output[currentOutSize++] = next;
                    buffer[bufferOffset] = next;
                    bufferOffset = (bufferOffset + 1) % bufferLength;
                }
            }
            else // Uncompressed block
            {
                byte next = reader.ReadByte();
                output[currentOutSize++] = next;

                buffer[bufferOffset] = next;
                bufferOffset = (bufferOffset + 1) % bufferLength;
            }
        }

        // Dont Validate alignment (I kinda just ignored indata.Length :P)
        // if (reader.BaseStream.Position < indata.Length)
        // {
        //     long remaining = indata.Length - reader.BaseStream.Position;
        //     if (remaining > 4 || ((reader.BaseStream.Position ^ (reader.BaseStream.Position & 3)) + 4 < indata.Length))
        //         throw new Exception("idk something went wrong.");
        // }

        return new ReadOnlySpan<byte>(output);
    }

    #region Original Compress method
    /// <summary>
    /// Compresses the input using the 'original', unoptimized compression algorithm.
    /// This algorithm should yield files that are the same as those found in the games.
    /// (delegates to the optimized method if LookAhead is set)
    /// </summary>
    public static unsafe byte[] Compress(ReadOnlySpan<byte> indata)
    {
        List<byte> finalBuffer = new List<byte>();
        // make sure the decompressed size fits in 3 bytes.
        // There should be room for four bytes, however I'm not 100% sure if that can be used
        // in every game, as it may not be a built-in function.
        if (indata.Length > 0xFFFFFF)
            throw new ArgumentOutOfRangeException("Input too large.");
        
        // write the compression header first
        finalBuffer.Add(0x10);
        finalBuffer.Add((byte)(indata.Length & 0xFF));
        finalBuffer.Add((byte)((indata.Length >> 8) & 0xFF));
        finalBuffer.Add((byte)((indata.Length >> 16) & 0xFF));
        fixed (byte* instart = &indata[0])
        {
            // we do need to buffer the output, as the first byte indicates which blocks are compressed.
            // this version does not use a look-ahead, so we do not need to buffer more than 8 blocks at a time.
            byte[] outbuffer = new byte[8 * 2 + 1];
            outbuffer[0] = 0;
            int bufferlength = 1, bufferedBlocks = 0;
            int readBytes = 0;
            while (readBytes < indata.Length)
            {
                #region If 8 blocks are buffered, write them and reset the buffer
                // we can only buffer 8 blocks at a time.
                if (bufferedBlocks == 8)
                {
                    finalBuffer.AddRange(outbuffer.Take(bufferlength));
                    // reset the buffer
                    outbuffer[0] = 0;
                    bufferlength = 1;
                    bufferedBlocks = 0;
                }
                #endregion
                // determine if we're dealing with a compressed or raw block.
                // it is a compressed block when the next 3 or more bytes can be copied from
                // somewhere in the set of already compressed bytes.
                int disp;
                int oldLength = Math.Min(readBytes, 0x1000);
                int length = GetOccurrenceLength(instart + readBytes, (int)Math.Min(indata.Length - readBytes, 0x12),
                                                      instart + readBytes - oldLength, oldLength, out disp);
                // length not 3 or more? next byte is raw data
                if (length < 3)
                {
                    outbuffer[bufferlength++] = *(instart + (readBytes++));
                }
                else
                {
                    // 3 or more bytes can be copied? next (length) bytes will be compressed into 2 bytes
                    readBytes += length;
                    // mark the next block as compressed
                    outbuffer[0] |= (byte)(1 << (7 - bufferedBlocks));
                    outbuffer[bufferlength] = (byte)(((length - 3) << 4) & 0xF0);
                    outbuffer[bufferlength] |= (byte)(((disp - 1) >> 8) & 0x0F);
                    bufferlength++;
                    outbuffer[bufferlength] = (byte)((disp - 1) & 0xFF);
                    bufferlength++;
                }
                bufferedBlocks++;
            }
            // copy the remaining blocks to the output
            if (bufferedBlocks > 0)
            {
                finalBuffer.AddRange(outbuffer.Take(bufferlength));
            }
        }

        while (finalBuffer.Count % 4 != 0)
        {
            finalBuffer.Add(0);
        }
        return finalBuffer.ToArray();
    }
    #endregion

    /// <summary>
    /// Determine the maximum size of a LZ-compressed block starting at newPtr, using the already compressed data
    /// starting at oldPtr. Takes O(inLength * oldLength) = O(n^2) time.
    /// </summary>
    /// <param name="newPtr">The start of the data that needs to be compressed.</param>
    /// <param name="newLength">The number of bytes that still need to be compressed.
    /// (or: the maximum number of bytes that _may_ be compressed into one block)</param>
    /// <param name="oldPtr">The start of the raw file.</param>
    /// <param name="oldLength">The number of bytes already compressed.</param>
    /// <param name="disp">The offset of the start of the longest block to refer to.</param>
    /// <param name="minDisp">The minimum allowed value for 'disp'.</param>
    /// <returns>The length of the longest sequence of bytes that can be copied from the already decompressed data.</returns>
    private static unsafe int GetOccurrenceLength(byte* newPtr, int newLength, byte* oldPtr, int oldLength, out int disp, int minDisp = 1)
    {
        disp = 0;
        if (newLength == 0)
            return 0;
        int maxLength = 0;
        // try every possible 'disp' value (disp = oldLength - i)
        for (int i = 0; i < oldLength - minDisp; i++)
        {
            // work from the start of the old data to the end, to mimic the original implementation's behaviour
            // (and going from start to end or from end to start does not influence the compression ratio anyway)
            byte* currentOldStart = oldPtr + i;
            int currentLength = 0;
            // determine the length we can copy if we go back (oldLength - i) bytes
            // always check the next 'newLength' bytes, and not just the available 'old' bytes,
            // as the copied data can also originate from what we're currently trying to compress.
            for (int j = 0; j < newLength; j++)
            {
                // stop when the bytes are no longer the same
                if (*(currentOldStart + j) != *(newPtr + j))
                    break;
                currentLength++;
            }
            // update the optimal value
            if (currentLength > maxLength)
            {
                maxLength = currentLength;
                disp = oldLength - i;
                // if we cannot do better anyway, stop trying.
                if (maxLength == newLength)
                    break;
            }
        }
        return maxLength;
    }
}