using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Represents the beams recieved by the client
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beams
    {
        /// <summary>
        /// Represents the beam Id
        /// </summary>
        [JsonProperty(PropertyName = "beam")]
        public int Id { get; internal set; }

        /// <summary>
        /// Represents the origin of the beam
        /// </summary>
        [JsonProperty(PropertyName = "org")]
        public Vector2D origin { get; internal set; }

        /// <summary>
        /// Reprsents the direction where the beam is shot
        /// </summary>
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; internal set; }

        /// <summary>
        /// Represents the owner of the beam
        /// </summary>
        [JsonProperty(PropertyName = "owner")]
        public int ownerID { get; internal set; }

        /// <summary>
        /// Represents the frames per second
        /// </summary>
        public int FPS { get; internal set; } = 60;

        /// <summary>
        /// Holds the score whenever a kill is made using
        /// a beam. 
        /// </summary>
        public int scoreKeeper { get; internal set; } = 0;

        /// <summary>
        /// Default constructor necessary for the Json to work
        /// </summary>
        public Beams()
        {

        }

        /// <summary>
        /// Contructor used to build a beam in the world
        /// </summary>
        /// <param name="location"></param>
        /// <param name="aiming"></param>
        /// <param name="id"></param>
        public Beams(Vector2D location, Vector2D aiming, int id)
        {
            this.origin = location;
            this.direction = aiming;
            ownerID = id;
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
