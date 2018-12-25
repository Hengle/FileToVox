﻿using SchematicToVox.Extensions;
using SchematicToVox.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public static class SchematicWriter
    {
        private const int HEIGHT_SIZE_HEIGHTMAP = 100;

        private static bool _excavate;
        private static bool _heightmap;

        public static Schematic WriteSchematic(string path, bool heightmap, bool excavate)
        {
            _excavate = excavate;
            _heightmap = heightmap;

            return WriteSchematicFromImage(path);
        }

        private static Schematic WriteSchematicFromImage(string path)
        {
            FileInfo info = new FileInfo(path);
            Bitmap bitmap = new Bitmap(info.FullName);

            if (bitmap.Width > 2016 || bitmap.Height > 2016)
                throw new Exception("Image is too big");

            Schematic schematic = new Schematic
            {
                Width = (short)bitmap.Width,
                Heigth = 1,
                Length = (short)bitmap.Height,
                Blocks = new List<HashSet<Block>>()
            };
            SchematicReader.LengthSchematic = schematic.Length;
            SchematicReader.WidthSchematic = schematic.Width;
            schematic.Blocks.Add(new HashSet<Block>());
            using (var progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write schematic from picture...");
                int size = schematic.Width * schematic.Length;
                for (int i = 0, global = 0; i < size; i++)
                {
                    int x = i % schematic.Width;
                    int y = i / schematic.Width;
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0)
                    {
                        if (_heightmap)
                        {
                            int intensity = color.R + color.G + color.B;
                            float position = intensity / (float)765;
                            int height = (int)(position * HEIGHT_SIZE_HEIGHTMAP);

                            if (_excavate)
                            {
                                bool createAll = true;
                                if (x - 1 > 0 && x + 1 < bitmap.Width && y - 1 > 0 && y + 1 < bitmap.Height)
                                {
                                    var colorLeft = bitmap.GetPixel(x - 1, y);
                                    var colorTop = bitmap.GetPixel(x, y - 1);
                                    var colorRight = bitmap.GetPixel(x + 1, y);
                                    var colorBottom = bitmap.GetPixel(x, y + 1);

                                    if (color == colorLeft && color == colorTop && color == colorRight && color == colorBottom)
                                    {
                                        createAll = false;
                                    }
                                }

                                if (createAll)
                                {
                                    for (int z = 0; z < height; z++)
                                    {
                                        Block block = new Block(x, z, y, 1, 1, new Tools.Color32(211, 211, 211, 255));
                                        AddBlock(ref schematic, ref global, block);
                                    }
                                }
                                else
                                {
                                    Block block = new Block(x, height - 1, y, 1, 1, new Tools.Color32(211, 211, 211, 255));
                                    AddBlock(ref schematic, ref global, block);
                                }
                            }
                            else
                            {
                                for (int z = 0; z < height; z++)
                                {
                                    Block block = new Block(x, z, y, 1, 1, new Tools.Color32(211, 211, 211, 255));
                                    AddBlock(ref schematic, ref global, block);
                                }
                            }

                        }
                        else
                        {
                            Block block = new Block(x, 1, y, 1, 1, color);
                            AddBlock(ref schematic, ref global, block);
                        }
                    }
                    progressbar.Report((i / (float)size));
                }
            }

            Console.WriteLine("[LOG] Done.");

            return schematic;
        }

        private static void AddBlock(ref Schematic schematic, ref int global, Block block)
        {
            try
            {
                schematic.Blocks[global].Add(block);
            }
            catch (OutOfMemoryException)
            {
                global++;
                schematic.Blocks.Add(new HashSet<Block>());
                schematic.Blocks[global].Add(block);
            }
        }
    }
}
