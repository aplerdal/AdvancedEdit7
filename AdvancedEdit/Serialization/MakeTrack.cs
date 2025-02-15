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
        public int ItemProbabilityIndex
        {
            get => EEItemProba[1] >> 1;
            set => EEItemProba = new byte[] { 0, (byte)(value << 1) };
        }

        private readonly Dictionary<string, byte[]> _fields;

        public byte[] this[string name]
        {
            get => _fields[name];
            set
            {
                if (!_fields.ContainsKey(name))
                {
                    _fields.Add(name, null);
                }

                _fields[name] = value;
            }
        }

        /// <summary>
        /// GP Start Position X.
        /// </summary>
        private byte[] SpStx
        {
            get => this["SP_STX"];
            set => this["SP_STX"] = value;
        }

        /// <summary>
        /// GP Start Position Y.
        /// </summary>
        private byte[] SpSty
        {
            get => this["SP_STY"];
            set => this["SP_STY"] = value;
        }

        /// <summary>
        /// GP Start Position Width (2nd Row Offset).
        /// </summary>
        private byte[] SpStw
        {
            get => this["SP_STW"];
            set => this["SP_STW"] = value;
        }

        /// <summary>
        /// Lap Line Area X.
        /// </summary>
        private byte[] SpLspx
        {
            get => this["SP_LSPX"];
            set => this["SP_LSPX"] = value;
        }

        /// <summary>
        /// Lap Line Area Y.
        /// </summary>
        private byte[] SpLspy
        {
            get => this["SP_LSPY"];
            set => this["SP_LSPY"] = value;
        }

        /// <summary>
        /// Lap Line Area Width.
        /// </summary>
        private byte[] SpLspw
        {
            get => this["SP_LSPW"];
            set => this["SP_LSPW"] = value;
        }

        /// <summary>
        /// Lap Line Area Height.
        /// </summary>
        private byte[] SpLsph
        {
            get => this["SP_LSPH"];
            set => this["SP_LSPH"] = value;
        }

        /// <summary>
        /// Lap Line Y.
        /// </summary>
        private byte[] SpLsly
        {
            get => this["SP_LSLY"];
            set => this["SP_LSLY"] = value;
        }

        /// <summary>
        /// Theme.
        /// </summary>
        private byte[] SpRegion
        {
            get => this["SP_REGION"];
            set => this["SP_REGION"] = value;
        }

        /// <summary>
        /// Battle Starting Position for Player 1.
        /// </summary>
        private byte[] EEBattleStart1
        {
            get => this["EE_BATTLESTART1"];
            set => this["EE_BATTLESTART1"] = value;
        }

        /// <summary>
        /// Battle Starting Position for Player 2.
        /// </summary>
        private byte[] EEBattleStart2
        {
            get => this["EE_BATTLESTART2"];
            set => this["EE_BATTLESTART2"] = value;
        }

        /// <summary>
        /// Object Tileset.
        /// </summary>
        private byte[] EEObjTileset
        {
            get => this["EE_OBJTILESET"];
            set => this["EE_OBJTILESET"] = value;
        }

        /// <summary>
        /// Object Interaction.
        /// </summary>
        private byte[] EEObjInteract
        {
            get => this["EE_OBJINTERACT"];
            set => this["EE_OBJINTERACT"] = value;
        }

        /// <summary>
        /// Object Routine.
        /// </summary>
        private byte[] EEObjRoutine
        {
            get => this["EE_OBJROUTINE"];
            set => this["EE_OBJROUTINE"] = value;
        }

        /// <summary>
        /// Object Palettes.
        /// </summary>
        private byte[] EEObjPalettes
        {
            get => this["EE_OBJPALETTES"];
            set => this["EE_OBJPALETTES"] = value;
        }

        /// <summary>
        /// Object Flashing.
        /// </summary>
        private byte[] EEObjFlashing
        {
            get => this["EE_OBJFLASHING"];
            set => this["EE_OBJFLASHING"] = value;
        }

        /// <summary>
        /// Item probability set index.
        /// </summary>
        private byte[] EEItemProba
        {
            get => this["EE_ITEMPROBA"];
            set => this["EE_ITEMPROBA"] = value;
        }

        // Object Behavior.
        // NOTE: Data ignored by Epic Edit, supported differently.
        // private byte[] SP_OPN;

        /// <summary>
        /// Tile Map.
        /// </summary>
        private byte[] MapBytes
        {
            get => this["MAP"];
            set => this["MAP"] = value;
        }

        // NOTE: Data ignored by Epic Edit, supported differently.
        // private byte[] MapMask;

        /// <summary>
        /// Overlay Tiles.
        /// </summary>
        private byte[] Gpex
        {
            get => this["GPEX"];
            set => this["GPEX"] = value;
        }

        /// <summary>
        /// AI.
        /// </summary>
        private byte[] Area
        {
            get => this["AREA"];
            set => this["AREA"] = value;
        }

        /// <summary>
        /// Objects.
        /// </summary>
        private byte[] Obj
        {
            get => this["OBJ"];
            set => this["OBJ"] = value;
        }

        /// <summary>
        /// Object View Areas.
        /// </summary>
        private byte[] AreaBorder
        {
            get => this["AREA_BORDER"];
            set => this["AREA_BORDER"] = value;
        }
        public List<AiSector> Ai {get=>GetAi();}
        
        /// <summary>
        /// Loads the MAKE track file data.
        /// </summary>
        public MakeTrack(string filePath)
        {
            _fields = new Dictionary<string, byte[]>();
            InitData();
            
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

        private void InitData()
        {
            SpStx = new byte[2];
            SpSty = new byte[2];
            SpStw = new byte[2];
            SpLspx = new byte[2];
            SpLspy = new byte[2];
            SpLspw = new byte[2];
            SpLsph = new byte[2];
            SpLsly = new byte[2];
            SpRegion = [0, 2];

            // EEBattleStart1 = [0x00, 0x02, 0x78, 0x02];
            // EEBattleStart2 = [0x00, 0x02, 0x88, 0x01];
            // EEObjTileset = new byte[2];
            // EEObjInteract = new byte[2];
            // EEObjRoutine = new byte[2];
            // EEObjPalettes = new byte[4];
            // EEObjFlashing = new byte[2];
            // EEItemProba = new byte[2];

            MapBytes = new byte[128*128];
            
            var area = new byte[4064];
            for (var i = 0; i < area.Length; i++)
            {
                area[i] = 0xFF;
            }

            Area = area;
        }
        /// <summary>
        /// Load SMK track over mksc one. Does not override tileset.
        /// </summary>
        /// <param name="track"></param>
        public void LoadOverTrack(ref Track track)
        {
            track.Size = new Point(128, 128); // All SMK tracks have this size
            track.Tilemap.Layout = GetLayout();
            track.Tilemap.RegenMap();
            track.AiSectors = GetAi();
        }
        private List<AiSector> GetAi(){
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
                    0=>ZoneShape.Rectangle,
                    2=>ZoneShape.TopLeft,
                    4=>ZoneShape.TopRight,
                    8=>ZoneShape.BottomLeft,
                    6=>ZoneShape.BottomRight,
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

        private byte[,] GetLayout()
        {
            var indices = this["MAP"];
            byte[,] layout = new byte[128, 128];
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                layout[x, y] = indices[y * 128 + x];
            }

            return layout;
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