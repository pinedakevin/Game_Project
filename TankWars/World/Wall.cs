using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Represents a Wall in the Game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        /// <summary>
        /// Represents the wall Id
        /// </summary>
        [JsonProperty(PropertyName = "wall")]
        public int Id { get; internal set; } = 0;

        /// <summary>
        /// Represents a coordinate point in the wall
        /// </summary>
        [JsonProperty]
        public Vector2D p1 { get; internal set; } = null;

        /// <summary>
        /// Represents a coordinate point in the wall
        /// </summary>
        [JsonProperty]
        public Vector2D p2 { get; internal set; } = null;

        private const double Thickness = 60;
        /// <summary>
        /// Represents the sides of the wall
        /// </summary>
        double top, bottom, left, right;

        /// <summary>
        /// Represents the Id for next wall
        /// </summary>
        private static int nextId = 0;

        /// <summary>
        ///   Default constructor in order for the Json libraries to work
        /// </summary>
        public Wall()
        {

        }

        /// <summary>
        /// Constructor to initialize object in the world
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(Vector2D p1, Vector2D p2)
        {
            Id = nextId++;
            this.p1 = p1;
            this.p2 = p2;

            double expansion = Thickness / 2 + Tank.Size / 2;
            left = Math.Min(this.p1.GetX(), this.p2.GetX()) - expansion;
            right = Math.Max(this.p1.GetX(), this.p2.GetX()) + expansion;
            top = Math.Min(this.p1.GetY(), this.p2.GetY()) - expansion;
            bottom = Math.Max(this.p1.GetY(), this.p2.GetY()) + expansion;
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
        /// Checks if wall collides with tank
        /// </summary>
        /// <param name="tankLoc"></param>
        /// <returns></returns>
        public bool CollidesTank(Vector2D tankLoc)
        {
            return left < tankLoc.GetX()
                && tankLoc.GetX() < right
                && top < tankLoc.GetY()
                && tankLoc.GetY() < bottom;
        }

        /// <summary>
        /// Checks if wall collides with object in the world. 
        /// </summary>
        /// <param name="objVector"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool WallCollisionCheck(Vector2D objVector, double size)
        {
            double leftc, rightc, topc, bottomc;
            double expansion = Thickness / 2 + size / 2;
            leftc = Math.Min(this.p1.GetX(), this.p2.GetX()) - expansion;
            rightc = Math.Max(this.p1.GetX(), this.p2.GetX()) + expansion;
            topc = Math.Min(this.p1.GetY(), this.p2.GetY()) - expansion;
            bottomc = Math.Max(this.p1.GetY(), this.p2.GetY()) + expansion;

            return leftc < objVector.GetX()
            && objVector.GetX() < rightc
            && topc < objVector.GetY()
            && objVector.GetY() < bottomc;
        }
    }
}
