using System;
using System.Collections.Generic;
using System.IO;

namespace AdvancedEdit.Compression;

public static class Lz10
{
    public static ReadOnlySpan<byte> Decompress(BinaryReader reader, long inLength)
{
    // Format constants
    const int bufferLength = 0x1000;

    // Read and validate the type byte
    byte type = reader.ReadByte();
    if (type != 0x10)
        throw new InvalidDataException("Invalid LZ-0x10 compressed stream.");

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
            if (reader.BaseStream.Position >= inLength)
                throw new NotEnoughDataException(currentOutSize, decompressedSize);

            flags = reader.ReadByte();
            mask = 0x80;
        }
        else
        {
            mask >>= 1;
        }

        if ((flags & mask) > 0) // Compressed block
        {
            if (reader.BaseStream.Position + 1 >= inLength)
                throw new NotEnoughDataException(currentOutSize, decompressedSize);

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
            if (reader.BaseStream.Position >= inLength)
                throw new NotEnoughDataException(currentOutSize, decompressedSize);

            byte next = reader.ReadByte();
            output[currentOutSize++] = next;

            buffer[bufferOffset] = next;
            bufferOffset = (bufferOffset + 1) % bufferLength;
        }
    }

    // Dont Validate alignment (I kinda just ignored inlength :P)
    // if (reader.BaseStream.Position < inLength)
    // {
    //     long remaining = inLength - reader.BaseStream.Position;
    //     if (remaining > 4 || ((reader.BaseStream.Position ^ (reader.BaseStream.Position & 3)) + 4 < inLength))
    //         throw new Exception("idk something went wrong.");
    // }

    return new ReadOnlySpan<byte>(output);
}
}