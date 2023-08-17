using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Xml;

namespace TankWars
{
    public class Settings
    {
        public int UniverseSize { get; private set; }
        public int MSPerFrame { get; private set; }
        public int FramesPerShot { get; private set; }
        public int RespawnRate { get; private set; }

        public HashSet<Wall> Walls = new HashSet<Wall>(); 

        public Settings(string filepath)
        {
            
            ReadXML(filepath);
        }

        // Reading XML file to get world info
        private void ReadXML(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("Did not read correctly due to being null or empty.");
            }

            int p1X = 0;
            int p1Y = 0;
            int p2X = 0;
            int p2Y = 0;

            try
            {
                // Create an XmlReader inside this block, and automatically Dispose() it at the end.
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "UniverseSize":
                                    reader.Read();
                                    int.TryParse(reader.Value, out int worldSize);
                                    UniverseSize = worldSize;
                                    break;
                                case "MSPerFrame":
                                    reader.Read();
                                    int.TryParse(reader.Value, out int msPerFrame);
                                    MSPerFrame = msPerFrame;
                                    break;
                                case "FramesPerShot":
                                    reader.Read();
                                    int.TryParse(reader.Value, out int framesPerShot);
                                    FramesPerShot = framesPerShot;
                                    break;

                                case "RespawnRate":
                                    reader.Read();
                                    int.TryParse(reader.Value, out int respawnRate);
                                    RespawnRate = respawnRate;
                                    break;

                                case "p1":

                                    // gets p1 "x"
                                    reader.ReadToDescendant("x");
                                    reader.Read();
                                    int.TryParse(reader.Value, out int x);
                                    p1X = x;

                                    // gets p1 "y"
                                    reader.ReadToFollowing("y");
                                    reader.Read();
                                    int.TryParse(reader.Value, out int y);
                                    p1Y = y;
                                    break;

                                case "p2":

                                    // gets p2 "x"
                                    reader.ReadToDescendant("x");
                                    reader.Read();
                                    int.TryParse(reader.Value, out int x2);
                                    p2X = x2;

                                    // gets p2 "y"
                                    reader.ReadToFollowing("y");
                                    reader.Read();
                                    int.TryParse(reader.Value, out int y2);
                                    p2Y = y2;
                                    break;
                            }  
                        }
                        else
                        {
                            if (reader.Name == "Wall")
                            {
                                Walls.Add(new Wall(new Vector2D(p1X, p1Y), new Vector2D(p2X, p2Y)));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Did not read correctly due to issues " +
                             "opening, reading, or closing the file. Exception Message: " + e.Message);
            }
        }
    }
}
