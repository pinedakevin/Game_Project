using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Represents a power up
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerups
    {
        /// <summary>
        /// Represents the Id 
        /// </summary>
        [JsonProperty(PropertyName = "power")]
        public int Id { get; set; }

        /// <summary>
        /// Represent the powerup location
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; }

        /// <summary>
        /// represents the powerup being picked by player
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool died { get; internal set; }

        /// <summary>
        /// timer to use for spawning
        /// </summary>
        public int spawnTimer = 0;

        /// <summary>
        /// Represents the maximum spawn time for the powerup
        /// </summary>
        public const int maxSpawnTime = 1650;

        /// <summary>
        /// Default constructor necessary for the Json to work
        /// </summary>
        public Powerups()
        {
            location = new Vector2D(-100, -100);
            this.Id = Id++;
            died = false;
        }

        /// <summary>
        /// Serializes the object into a Json String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// sets the powerup initially to a random location in the world
        /// </summary>
        internal int RandomLocation()
        {
            Random random = new Random();
            return spawnTimer = random.Next(100 , maxSpawnTime);
        }
    }
}
