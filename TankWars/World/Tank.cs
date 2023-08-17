using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Represents a tank in the Game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        /// <summary>
        /// Represents the tank Id
        /// </summary>
        [JsonProperty(PropertyName = "tank")]
        public int Id { get; internal set; }

        /// <summary>
        /// Represents the tank location in the world
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; } = null;

        /// <summary>
        /// Represents the tank orientation in the world
        /// </summary>
        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation { get; internal set; } = null;

        /// <summary>
        /// Represents the turrent direction
        /// </summary>
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming = new Vector2D(0, -1);

        /// <summary>
        /// Represents the name of the player
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string name { get; internal set; } = null;

        /// <summary>
        /// Represent the health points of the players' tank
        /// </summary>
        [JsonProperty(PropertyName = "hp")]
        public int hitPoints = MaxHP; //Constants.MaxHP;

        /// <summary>
        /// Represents the score earn by the player
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public int score = 0;

        /// <summary>
        /// Represents the player diying
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool died = false;

        /// <summary>
        /// Represents the player disconnecting from server
        /// </summary>
        [JsonProperty(PropertyName = "dc")]
        public bool disconnected = false;

        /// <summary>
        /// Represents the player joining the server
        /// </summary>
        [JsonProperty(PropertyName = "join")]
        public bool joined = false;

        /// <summary>
        /// State of the shot when is delayed
        /// </summary>
        public bool setShotDelay = false;

        /// <summary>
        /// State of the beam when is delayed
        /// </summary>
        public bool setBeamDelay = false;

        /// <summary>
        /// Represents the player joining the server
        /// </summary>
        public int powerUpCount = 0;

        /// <summary>
        /// rate at which tank will appear
        /// </summary>
        public int RespawnRate = 0;

        /// <summary>
        /// speed at which the projectile will be fired
        /// </summary>
        public int shotFPS = 0;

        /// <summary>
        /// speed at which the beam will be fired
        /// </summary>
        public int beamFPS = 0;

        /// <summary>
        /// Velocity at which tank moves
        /// </summary>
        public Vector2D Velocity { get; internal set; }

        /// <summary>
        /// Represents health of new player
        /// </summary>
        private const int MaxHP = 3;

        /// <summary>
        /// speed of the tank
        /// </summary>
        public const double EnginePower = 3;

        /// <summary>
        /// size of tank
        /// </summary>
        public const double Size = 60;

        /// <summary>
        /// virtual thinkness for collision purposes
        /// </summary>
        private const double Thickness = 60;

        /// <summary>
        /// Default constructor necessary to serialize Json
        /// </summary>
        public Tank()
        {

        }

        /// <summary>
        /// Constructor used for inizializing object in the world
        /// </summary>
        public Tank(int id, string name)
        {
            Random rand = new Random();
            Id = id;
            location = new Vector2D(0, 0);
            orientation = new Vector2D(0, -1);
            aiming = orientation;
            this.name = name;
            hitPoints = MaxHP;
            score = 0;
            died = false;
            joined = true;
            Velocity = new Vector2D(0, 0);
        }

        /// <summary>
        /// Serializes the object into a Json String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
