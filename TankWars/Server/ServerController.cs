using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NetworkUtil;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// 
    /// </summary>
    public class ServerController
    {
        /// <summary>
        /// Represents the settings object in the project
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Represents the game states world
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Represents a dictionary of all clients and their ids
        /// </summary>
        private Dictionary<int, SocketState> clients = new Dictionary<int, SocketState>();

        /// <summary>
        /// Reperents the start up information that is passed to the server.
        /// </summary>
        private string startupInfo;

        /// <summary>
        /// A data structure that holds all clients that need to be removed from the game state
        /// when disconnected. 
        /// </summary>
        private List<int> forRemoval = new List<int>();

        /// <summary>
        /// The controller for the server settings
        /// </summary>
        /// <param name="settings"></param>
        public ServerController(Settings settings)
        {
            this.settings = settings;
            theWorld = new World(settings.UniverseSize);

            foreach (Wall wall in settings.Walls)
            {
                theWorld.walls[wall.Id] = wall;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(theWorld.worldSize);
            sb.Append("\n");

            foreach (Wall wall in theWorld.walls.Values)
            {
                sb.Append(wall.ToString());
            }

            startupInfo = sb.ToString();
        }

        /// <summary>
        /// The start method of a server
        /// </summary>
        internal void Start()
        {
            Networking.StartServer(NewClient, 11000);
            Console.WriteLine("Server is running. Accepting clients.");
            Thread t = new Thread(Update);
            t.Start();

            lock (theWorld) {
                theWorld.RespawnRate = settings.RespawnRate;
                theWorld.FramePerShot = settings.FramesPerShot;

                //Powerup initial Spawn 
                theWorld.powerups[0] = new Powerups();
                theWorld.powerups[1] = new Powerups();
                theWorld.powerups[1].Id++;
                Respawn(theWorld.powerups[0]);
                Respawn(theWorld.powerups[1]);
            }
        }

        /// <summary>
        /// Method that updates the server object at every frame. 
        /// </summary>
        /// <param name="obj"></param>
        private void Update(object obj)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();


            while (true)
            {
                while (watch.ElapsedMilliseconds < settings.MSPerFrame)
                    ;
                watch.Restart();
                StringBuilder sb = new StringBuilder();
                lock (theWorld)
                {
                    theWorld.Update();
                    foreach (Tank tank in theWorld.tanks.Values)
                    {
                        foreach (SocketState client in clients.Values)
                        {
                            if (!client.TheSocket.Connected)
                            {

                                theWorld.tanks[(int)client.ID].disconnected = true;
                                forRemoval.Add((int)client.ID);
                            }
                        }

                        sb.Append(tank.ToString());
                    }

                    foreach (Powerups powerup in theWorld.powerups.Values)
                    {
                        sb.Append(powerup.ToString());
                    }

                    foreach (Projectiles projectile in theWorld.projectiles.Values)
                    {
                        sb.Append(projectile.ToString());
                    }

                    foreach (Beams beam in theWorld.beams.Values)
                    {
                        sb.Append(beam.ToString());
                    }

                }

                string frame = sb.ToString();

                lock (clients)
                {
                    foreach (SocketState client in clients.Values)
                    {
                        Networking.Send(client.TheSocket, frame);
                    }
                }

                //Removal
                foreach (int tankID in forRemoval)
                {

                    clients.Remove(tankID);
                }
            }
        }

        /// <summary>
        /// Adds a new client for every player name that is received.
        /// </summary>
        /// <param name="client"></param>
        private void NewClient(SocketState client)
        {
            client.OnNetworkAction = RecievePlayerName;
            Networking.GetData(client);


        }

        /// <summary>
        /// Recieves the player name for each client that is pushed. 
        /// </summary>
        /// <param name="client"></param>
        private void RecievePlayerName(SocketState client)
        { 
            string name = client.GetData();
            if (!name.EndsWith("\n"))
            {
                client.GetData();
                return;
            }
            client.RemoveData(0, name.Length);
            name = name.Trim();

            Networking.Send(client.TheSocket, client.ID + "\n");
            Networking.Send(client.TheSocket, startupInfo);

            Console.WriteLine("player: " + "(" + (int)client.ID + ") " + "\"" + name + "\"" + " joined");

            lock (theWorld)
            {
                //Tank initial Spawn 
                theWorld.tanks[(int)client.ID] = new Tank((int)client.ID, name);
                Respawn(theWorld.tanks[(int)client.ID]);
            }

            lock (clients)
            {
                clients.Add((int)client.ID, client);
            }

            client.OnNetworkAction = ReceiveControlCommand;
            Networking.GetData(client);
        }

       
        /// <summary>
        /// To be used when the control commands are needed to be recieved. 
        /// </summary>
        /// <param name="client"></param>
        private void ReceiveControlCommand(SocketState client)
        {
            //prevent issues when recieving controls from a disconnected client. 
            if (!client.TheSocket.Connected)
            {
                return;
            }

            string totalData = client.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            foreach (string p in parts)
            {
                if (p.Length == 0)
                    continue;

                if (p[p.Length - 1] != '\n')
                    break;

                ControlCommands ctrlCmd = JsonConvert.DeserializeObject<ControlCommands>(p);

                lock (theWorld)
                {
                    theWorld.CtrlCmds[(int)client.ID] = ctrlCmd;
                }

                client.RemoveData(0, p.Length);
            }

            Networking.GetData(client);
        }


        /// <summary>
        /// Method to use when needing to respawn a projectile or tank. Uses a 
        /// random vector location and checks if it collides with a wall. If it
        /// collides with a wall, then it will continously loop until it finds a vector
        /// that does not collide with any existing wall.
        /// 
        /// The Logic can be expanded to other objects that need to detect if it
        /// spawns in a wall. 
        /// </summary>
        /// <param name="obj">Item to be spawned</param>
        private void Respawn(Object obj)
        {
            lock (theWorld.tanks)
            {
                if (obj is Powerups p)
                {
                    bool collision = true;
                    Random random = new Random();
                    Vector2D newSpawn = new Vector2D(random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2),
                    (random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2)));

                    int counter = 0;

                    //Goes through a loop and checks the vector, if that vector collides with any wall
                    //a new vector is made until it finds a new one. Uses a counting method to check
                    //every wall. 
                    while (collision)
                    {
                        newSpawn = new Vector2D(random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2),
                            (random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2)));

                        foreach (Wall wall in theWorld.walls.Values)
                        {
                            if (wall.WallCollisionCheck(newSpawn, 30))
                            {
                                counter++;
                            }
                        }

                        if (counter == 0)
                        {
                            p.location = newSpawn;
                            collision = false;
                            break;
                        }
                        counter = 0;
                    }
                }
                if (obj is Tank t)
                {
                    bool collision = true;
                    Random random = new Random();
                    Vector2D newSpawn = new Vector2D(random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2),
                    (random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2)));

                    int counter = 0;

                    while (collision)
                    {
                        //Goes through a loop and checks the vector, if that vector collides with any wall
                        //a new vector is made until it finds a new one. Uses a counting method to check
                        //every wall. 
                        newSpawn = new Vector2D(random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2),
                            (random.Next(-theWorld.worldSize / 2, theWorld.worldSize / 2)));

                        foreach (Wall wall in theWorld.walls.Values)
                        {
                            if (wall.WallCollisionCheck(newSpawn, 60))
                            {
                                counter++;
                            }
                        }

                        if (counter == 0)
                        {
                            collision = false;

                            t.location = newSpawn;
                            t.hitPoints = 3;
                            t.died = false;
                            break;
                        }

                        counter = 0;
                    }
                }
            }
        }

    }
}
