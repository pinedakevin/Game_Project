using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Represents a control command for the game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        /// <summary>
        /// Represents the tank moving
        /// </summary>
        [JsonProperty(PropertyName = "moving")]
        public string moving { get; internal set; } = null;

        /// <summary>
        /// Represents the tank firing the projectile
        /// </summary>
        [JsonProperty(PropertyName = "fire")]
        public string fire { get; internal set; } = null;

        /// <summary>
        /// Represents the turrents direction on the screen
        /// </summary>
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D turretDirection { get; internal set; } = null;

        /// <summary>
        /// Default constructor necessary for the Json to work
        /// </summary>
        public ControlCommands() 
        {

        }
    }
}
