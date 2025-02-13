#region GPL statement
/*Epic Edit is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion
// Modified from Epic Edit's implementation.

/*
MAKE param names:
SP_STX, SP_STY = gp position start x and y
SP_STW, = gp position 2nd row offset

EE_OBJTILESET = obj tileset, might be EE specific
EE_OBJPALETTES = obj palettes

MAP = track map
AREA = Ai (zones?)
OBJ = Objects
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace AdvancedEdit.Serialization
{

    /// <summary>
    /// Represents the data found in a MAKE exported track file.
    /// </summary>
    public class MakeTrack
    {
        Dictionary<string, byte[]> _fields = new Dictionary<string, byte[]>();
        public byte[] this[string name]
        {
            get => _fields[name];
            set
            {
                if (!_fields.ContainsKey(name))
                {
                    _fields.Add(name, value);
                }
                _fields[name] = value;
            }
        }
        
        public List<AiSector> Ai {get=>GetAi();}
        
        /// <summary>
        /// Loads the MAKE track file data.
        /// </summary>
        public MakeTrack(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (TextReader reader = new StreamReader(fs))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0 || line[0] != '#')
                    {
                        continue;
                    }

                    var index = line.IndexOf(' ');
                    var fieldName = index == -1 ? line : line.Substring(0, index);
                    fieldName = fieldName.Substring(1); // Remove leading #

                    if (_fields.TryGetValue(fieldName, out var data))
                    { 
                        if (data.Length <= 4)
                        {
                            LoadLineData(data, line);
                        }
                        else
                        {
                            LoadBlockData(data, reader);
                        }
                    }
                }
            }
        }
        public List<AiSector> GetAi(){
            List<AiSector> ai = new();
            var aiData = this["AREA"]; // One AI Sector per line
            var count = aiData.Length / 32;
            for (int i = 0; i < count && aiData[i*32]!=0xFF; i++) {
                int lineOffset = i*32;
                // Reorder the target data 
                int flags = aiData[lineOffset];
                bool intersection = (flags&0x80)==0x80;
                int speed = flags&0x3;

                Point target = new Point(aiData[lineOffset+1], aiData[lineOffset+2]);

                // Probably doing this wrong
                ZoneShape shape = aiData[lineOffset + 16] switch {
                    0b0001=>ZoneShape.Rectangle,
                    0b0010=>ZoneShape.TopLeft,
                    0b0100=>ZoneShape.TopRight,
                    0b1000=>ZoneShape.BottomLeft,
                    0b10000=>ZoneShape.BottomRight,
                    _=>throw new Exception("Error reading AI"),
                };
                Rectangle zone = new Rectangle(
                    aiData[lineOffset + 17],
                    aiData[lineOffset + 18],
                    aiData[lineOffset + 19],
                    aiData[lineOffset + 20]
                );
                ai.Add(new AiSector([target,target,target], shape, zone, [speed, speed, speed], [intersection,intersection,intersection]));
            }
            return ai;
        }
        private static void LoadLineData(byte[] data, string line)
        {
            var space = line.IndexOf(' ');
            line = line.Substring(space).Trim();
            if (line.Length != data.Length * 2)
            {
                // Data length is higher or lower than expected
                throw new ArgumentException("Invalid data length. Import aborted.", nameof(data));
            }

            LoadBytesFromHexString(data, line);
        }

        private static void LoadBlockData(byte[] data, TextReader reader)
        {
            var index = 0;
            var line = reader.ReadLine();
            while (!string.IsNullOrEmpty(line) && line[0] == '#')
            {
                var lineBytes = HexStringToBytes(line.Substring(1));
                var lineBytesLength = lineBytes.Length;

                if (index + lineBytesLength > data.Length)
                {
                    // Data length is higher than expected
                    throw new ArgumentException("Invalid data length. Import aborted.", nameof(data));
                }

                Buffer.BlockCopy(lineBytes, 0, data, index, lineBytesLength);
                line = reader.ReadLine();
                index += lineBytesLength;
            }

            if (index != data.Length)
            {
                // Data length is lower than expected
                throw new ArgumentException("Invalid data length. Import aborted.", nameof(data));
            }
        }

        private static byte[] HexStringToBytes(string data)
        {
            var bytes = new byte[data.Length / 2];
            LoadBytesFromHexString(bytes, data);
            return bytes;
        }

        private static void LoadBytesFromHexString(byte[] bytes, string hex)
        {
            var bl = bytes.Length;
            for (var i = 0; i < bl; ++i)
            {
                bytes[i] = (byte)((hex[2 * i] > 'F' ? hex[2 * i] - 0x57 : hex[2 * i] > '9' ? hex[2 * i] - 0x37 : hex[2 * i] - 0x30) << 4);
                bytes[i] |= (byte)(hex[2 * i + 1] > 'F' ? hex[2 * i + 1] - 0x57 : hex[2 * i + 1] > '9' ? hex[2 * i + 1] - 0x37 : hex[2 * i + 1] - 0x30);
            }
        }
    }
}