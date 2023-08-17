using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Represents a projectile in the TankWars game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectiles
    {
        /// <summary>
        /// Represents the Id of the projectile
        /// </summary>
        [JsonProperty(PropertyName = "proj")]
        public int Id { get; internal set; }

        /// <summary>
        /// Represents the location of projectile
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D Location { get; internal set; } 

        /// <summary>
        /// Represents the orientation of the projectile
        /// </summary>
        [JsonProperty(PropertyName = "dir")]
        public Vector2D Orientation { get; internal set; } 

        /// <summary>
        /// Represents the projectile dissaperance 
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool Died { get; internal set; } = false;

        /// <summary>
        /// Represents the tank id that owns this projectile
        /// </summary>
        [JsonProperty(PropertyName = "owner")]
        public int OwnerID { get; internal set; }

        /// <summary>
        /// Represents the size of the projectile
        /// </summary>
        public int Size { get; internal set; } = 30;

        /// <summary>
        /// Represents the rate at which a projectile should be fired
        /// </summary>
        public int FramesPerShot { get; internal set; }

        /// <summary>
        /// keeps the score 
        /// </summary>
        public bool scoreKeeper { get; internal set; } = false;

        /// <summary>
        /// speed for a normal projectile
        /// </summary>
        public const int projectileSpeed = 25;
        
        /// <summary>
        /// Default constructor necessary for the Json to work
        /// </summary>
        public Projectiles() 
        {
            
        }

        /// <summary>
        /// Constructor to use in the world to inizialize it
        /// </summary>
        /// <param name="location"></param>
        /// <param name="aiming"></param>
        /// <param name="id"></param>
        public Projectiles(Vector2D location, Vector2D aiming, int id)
        {
            Location = location;
            Orientation = aiming;
            OwnerID = id;
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
