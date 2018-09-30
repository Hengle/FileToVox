﻿using SchematicReader;
using SchematicToVox.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Vox
{
    public class VoxWriter : VoxParser
    {
        private int _width = 0;
        private int _length = 0;
        private int _height = 0;

        private int _childrenChunkSize = 0;

        public bool WriteModel(string absolutePath, Schematic schematic)
        {
            _width = _length = _height = 0;
            using (var writer = new BinaryWriter(File.Open(absolutePath, FileMode.Create)))
            {
                writer.Write(HEADER);
                writer.Write(VERSION);
                writer.Write(MAIN);
                writer.Write(0); //MAIN CHUNK has a size of 0
                writer.Write(CountChildrenSize(schematic));
                WriteChunk(writer, schematic);
            }
            return true;
        }

        private int CountChildrenSize(Schematic schematic)
        {
            _width = (int)Math.Ceiling(((decimal)schematic.Width / 126));
            _length = (int)Math.Ceiling(((decimal)schematic.Length / 126));
            _height = (int)Math.Ceiling(((decimal)schematic.Heigth / 126));

            int countSize = _width * _length * _height;
            _childrenChunkSize = (12 * countSize); //SIZE CHUNK
            _childrenChunkSize += (4 + schematic.Blocks.Count * 4); //XYZI CHUNK
            return _childrenChunkSize;
        }

        private List<Block> GetBlocksInRegion(Vector3 min, Vector3 max, Schematic schematic)
        {
            return schematic.Blocks.Where(t => t.X >= min.x && t.Y >= min.y && t.Z >= min.z
            && t.X < max.x && t.Y < max.y && t.Z < max.z).ToList();
        }

        private void WriteChunk(BinaryWriter writer, Schematic schematic)
        {
            int countSize = _width * _length * _height;
            for (int i = 0; i < countSize; i++)
            {
                writer.Write(SIZE);
                writer.Write(126);
                writer.Write(126);
                writer.Write(126);
                writer.Write(XYZI);
                var blocks = GetBlocksInRegion(new Vector3(i * 126, i * 126, i * 126), new Vector3((i * 126) + 126, (i * 126) + 126, (i * 126) + 126), schematic);
                writer.Write(blocks.Count);
                foreach (Block block in blocks)
                {
                    writer.Write(block.X);
                    writer.Write(block.Y);
                    writer.Write(block.Z);
                    writer.Write((byte)default_palette[1]); //TODO: Apply color of the block
                }
            }

        }
    }
}