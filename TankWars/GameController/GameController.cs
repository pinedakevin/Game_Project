using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TankWars;

namespace GC
{
    /// <summary>
    /// Represents the Controller on the MVC model
    /// </summary>
    public class GameController
    {
        /// <summary>
        /// Delegate used to pass the player's and world's info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="size"></param>
        public delegate void GetWorldInfo(int id, int size);

        /// <summary>
        /// Event to trigger to get the world Info
        /// </summary>
        public event GetWorldInfo GetInfo;

        /// <summary>
        /// Delegate to get turrent information
        /// its location and properties
        /// </summary>
        /// <param name="v"></param>
        public delegate void TurrentInfo(Vector2D v);

        /// <summary>
        /// informs the view of the turrent's info
        /// </summary>
        public event TurrentInfo TurrentLocation;

        /// <summary>
        /// informs the view of an error connecting
        /// </summary>
        public delegate void ErrorHandler(string err);

        /// <summary>
        /// informs the view of an error connecting
        /// </summary>
        public event ErrorHandler Error;

        /// <summary>
        /// Represent the players name
        /// </summary>
        /// 
        public string playerName { get; private set; }

        /// <summary>
        /// Represents an specific Id that identifies the player
        /// </summary>
        public int Id { get; private set; }


        /// <summary>
        /// WorldSize is square
        /// </summary>
        private int WorldSize;

        /// <summary>
        ///  World is a simple container for Players and Powerups
        /// </summary>

        private World TheWorld;

        /// <summary>
        /// informs the view about changes in the world
        /// </summary>
        public event Action UpdateArrived;

        /// <summary>
        /// Represents an Up key being pressed
        /// </summary>
        private bool MovingPressedUp = false;

        /// <summary>
        /// Represents a down key being pressed
        /// </summary>
        private bool MovingPressedDown = false;

        /// <summary>
        /// Represents a left key being pressed
        /// </summary>
        private bool MovingPressedLeft = false;

        /// <summary>
        /// Represents a right key being pressed
        /// </summary>
        private bool MovingPressedRight = false;

        /// <summary>
        /// Represents the mouse moving on the screen
        /// </summary>
        private bool MouseMoving = false;

        /// <summary>
        /// Represents a projectile being shot
        /// </summary>
        private bool mouseRightPressed = false;

        /// <summary>
        /// Represents a beam being shot
        /// </summary>
        private bool mouseLeftPressed = false;

        /// <summary>
        /// Represents the control commands used to move the tank
        /// </summary>
        public ControlCommands ctrlcmd;
       
        /// <summary>
        /// Represents the socket that connects to the server
        /// </summary>
        private SocketState theServer = null;

        /// <summary>
        /// Game controller's constructor
        /// </summary>
        public GameController()
        {
            this.TheWorld = new World(WorldSize);
            ctrlcmd = new ControlCommands();
        }

        /// <summary>
        /// Gets the the world
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return TheWorld;
        }

        /// <summary>
        /// Connects the player to the server
        /// </summary>
        /// <param name="player"></param>
        /// <param name="host"></param>
        public void Connect(string player, string host)
        {
            playerName = player;
            Networking.ConnectToServer(OnConnect, host, 11000);
        }

        /// <summary>
        /// Performs the initial conexion to the server
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {  
            if (state.ErrorOccured)
            {
                Error("Error connecting to server");
                return;
            }

            theServer = state;
            Networking.Send(state.TheSocket, playerName + "\n");
            state.OnNetworkAction = RecieveStartupInfo;
            Networking.GetData(state);
        }

        /// <summary>
        /// Represents the initial handshake with the server
        /// </summary>
        /// <param name="state"></param>
        private void RecieveStartupInfo(SocketState state)
        {
            if (state.ErrorOccured)
            {
                // inform the view through the delegate
                Error("Lost connection to server");
                return;
            }

            // the server sends two pieces of important data, the player's Id and the size of the world
            string info = state.GetData();
            string[] parts = Regex.Split(info, @"(?<=[\n])");
            if (parts.Length < 2 || !parts[1].EndsWith("\n"))
            {
                Networking.GetData(state);
                return;
            }

            Id = int.Parse(parts[0]);
            WorldSize = int.Parse(parts[1]);
            GetInfo(Id, WorldSize);

            TheWorld.worldSize = WorldSize;
            state.RemoveData(0, parts[0].Length + parts[1].Length);
            state.OnNetworkAction = RecieveJson;
            Networking.GetData(state);
        }

        /// <summary>
        /// Recieves Json data from server and deserialize it and stores it in the world
        /// </summary>
        /// <param name="state"></param>
        private void RecieveJson(SocketState state)
        {
            string message = state.GetData();
            string[] parts = Regex.Split(message, @"(?<=[\n])");

            lock (TheWorld)
            {
                foreach (string part in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (part.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (part[part.Length - 1] != '\n')
                        break;
                    
                    // parse the objects and desirialize them
                    JObject obj = JObject.Parse(part);
                    JToken tokenWall = obj["wall"];
                    if (tokenWall != null)
                    {
                        Wall wall = JsonConvert.DeserializeObject<Wall>(part);
                        TheWorld.walls[wall.Id] = wall;
                    }

                    JToken tokenTank = obj["tank"];
                    if (tokenTank != null)
                    {
                        Tank tank = JsonConvert.DeserializeObject<Tank>(part);
                        TheWorld.tanks[tank.Id] = tank;
                    }

                    JToken tokenProj = obj["proj"];
                    if (tokenProj != null)
                    {
                        Projectiles projectile = JsonConvert.DeserializeObject<Projectiles>(part);
                        TheWorld.projectiles[projectile.Id] = projectile;
                    }

                    JToken tokenBeam = obj["beam"];
                    if (tokenBeam != null)
                    {
                        Beams beam = JsonConvert.DeserializeObject<Beams>(part);
                        TheWorld.beams[beam.Id] = beam;
                    }

                    JToken tokenPower = obj["power"];
                    if (tokenPower != null)
                    {
                        Powerups powerup = JsonConvert.DeserializeObject<Powerups>(part);
                        TheWorld.powerups[powerup.Id] = powerup;
                    }

                    state.RemoveData(0, part.Length);
                }
            }
            // inform the view of new data  and continue reciving data
            UpdateArrived?.Invoke();
            ProcessInputs();
            Networking.GetData(state);
        }



        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            theServer?.TheSocket.Close();
        }


        /// <summary>
        /// Checks which inputs are currently held down
        /// Normally this would send a message to the server
        /// </summary>
        private void ProcessInputs()
        {
            if (MovingPressedUp)
            {
                Console.WriteLine("moving up");
                ctrlcmd.moving = "up";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");
            }
            if (MovingPressedDown)
            {
                ctrlcmd.moving = "down";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");

            }

            if (MovingPressedLeft)
            {
                ctrlcmd.moving = "left";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");
            }

            if (MovingPressedRight)
            {
                ctrlcmd.moving = "right";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");
            }
            if (MouseMoving)
            {
                ctrlcmd.moving = "none";
                ctrlcmd.fire = "none";
                string move = JsonConvert.SerializeObject(ctrlcmd);

                Networking.Send(theServer.TheSocket, move + "\n");

            }

            if (mouseLeftPressed)
            {
                ctrlcmd.moving = "none";
                ctrlcmd.fire = "main";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");
            }

            if (mouseRightPressed)
            {
                ctrlcmd.moving = "none";
                ctrlcmd.fire = "alt";
                string move = JsonConvert.SerializeObject(ctrlcmd);
                Networking.Send(theServer.TheSocket, move + "\n");
            }
        }


        /// <summary>
        ///Handles movement request
        /// </summary>
        public void HandleMoveRequest(string s)
        {
            switch (s)
            {
                case "up":
                    MovingPressedUp = true;
                    break;
                case "down":
                    MovingPressedDown = true;
                    break;
                case "left":
                    MovingPressedLeft = true;
                    break;
                case "right":
                    MovingPressedRight = true;
                    break;
                default:
                    // code block
                    break;
            }
        }

        /// <summary>
        /// Cancels a movement request
        /// </summary>
        public void CancelMoveRequest(string s)
        {
            switch (s)
            {
                case "up":
                    MovingPressedUp = false;
                    break;
                case "down":
                    MovingPressedDown = false;
                    break;
                case "left":
                    MovingPressedLeft = false;
                    break;
                case "right":
                    MovingPressedRight = false;
                    break;
                default:
                    // code block
                    break;
            }
        }

        /// <summary>
        /// Handles a mouse request
        /// </summary>
        public void HandleMouseRequest(string s)
        {
            if (s == "left")
            {
                mouseLeftPressed = true;

            }

            else if (s == "right") mouseRightPressed = true;
        }

        /// <summary>
        /// Cancels a mouse request
        /// </summary>
        public void CancelMouseRequest(string s)
        {
            if (s == "left")
            {
                mouseLeftPressed = false;

            }

            else if (s == "right") mouseRightPressed = false;
        }

        /// <summary>
        /// Handles the mouse movement that controls the turrent
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v"></param>
        public void MouseMoved(string s, Vector2D v)
        {
            MouseMoving = true;
            v.Normalize();
            TurrentLocation(v);
        }
    }
}
