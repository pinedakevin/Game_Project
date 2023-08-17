using System;
using System.Collections.Generic;
using TankWars;


namespace TankWars
{
    /// <summary>
    /// Represents the world of the Game
    /// </summary>
    public class World
    {
        /// <summary>
        /// Represents the size of the world
        /// </summary>
        public int worldSize { get; set; }

        /// <summary>
        /// Represents walls
        /// </summary>
        public readonly Dictionary<int, Wall> walls;

        /// <summary>
        /// Represents beams
        /// </summary>
        public readonly Dictionary<int, Beams> beams;

        /// <summary>
        /// Represents powerups
        /// </summary>
        public readonly Dictionary<int, Powerups> powerups;

        /// <summary>
        /// Represents projectiles
        /// </summary>
        public readonly Dictionary<int, Projectiles> projectiles;

        /// <summary>
        /// Represents tanks
        /// </summary>
        public readonly Dictionary<int, Tank> tanks;

        /// <summary>
        /// Represents the control commands for the player
        /// </summary>
        public Dictionary<int, ControlCommands> CtrlCmds = new Dictionary<int, ControlCommands>();

        /// <summary>
        /// Repersents the FramesPerShot used in the settings
        /// </summary>
        public int FramePerShot;

        /// <summary>
        /// Represents the RespawnRate used in the Settings
        /// </summary>
        public int RespawnRate;
       

        /// <summary>
        /// constructor for world
        /// </summary>
        /// <param name="worldSize"></param>
        public World(int worldSize)
        {
            this.worldSize = worldSize;
            walls = new Dictionary<int, Wall>();
            beams = new Dictionary<int, Beams>();
            powerups = new Dictionary<int, Powerups>();
            projectiles = new Dictionary<int, Projectiles>();
            tanks = new Dictionary<int, Tank>();
        }

        /// <summary>
        /// An update method that is called to the server to update the world state. 
        /// </summary>
        public void Update()
        {
            //Section for all controls being sent from the player. Moving and shooting. 
            ControlsCommandSection();

            //Collision section for projectiles, powerups, and tanks.
            CollisionSection();

            //Respawn Section for powerups and tanks.
            RespawnSection();

            //Delay section & Scoring Section for tanks, powerups, and projectiles.
            DelayScoringSection();
        }

        /// <summary>
        /// A section of logic that is to be used with the Update method to
        /// recieve all commands from the user when needing to shoot or move. 
        /// </summary>
        private void ControlsCommandSection()
        {
            //locking the threads
            lock (tanks)
            {
                foreach (KeyValuePair<int, ControlCommands> ctrlCmd in CtrlCmds)
                {
                    Tank tank = tanks[ctrlCmd.Key];

                    // updating the turrent direction
                    tank.aiming = ctrlCmd.Value.turretDirection;

                    if (!tank.died)
                    {
                        // updating the tank location and orientation
                        switch (ctrlCmd.Value.moving)
                        {
                            case "up":
                                tank.orientation = new Vector2D(0, -1);
                                tank.Velocity = new Vector2D(0, -1);
                                break;

                            case "down":
                                tank.orientation = new Vector2D(0, 1);
                                tank.Velocity = new Vector2D(0, 1);
                                break;

                            case "left":
                                tank.orientation = new Vector2D(-1, 0);
                                tank.Velocity = new Vector2D(-1, 0);
                                break;

                            case "right":
                                tank.orientation = new Vector2D(1, 0);
                                tank.Velocity = new Vector2D(1, 0);
                                break;

                            default:
                                tank.Velocity = new Vector2D(0, 0);
                                break;
                        }

                        tank.Velocity *= Tank.EnginePower;
                    }
                }
            }
            //locking the threads
            lock (tanks)
            {
                foreach (KeyValuePair<int, ControlCommands> ctrlCmd in CtrlCmds)
                {
                    Tank tank = tanks[ctrlCmd.Key];

                    if (!tank.died)
                    {
                        // updating the tank location and orientation
                        // Sets fields if a tank shoots a beam or projectile. 
                        switch (ctrlCmd.Value.fire)
                        {
                            case "none":
                                break;

                            case "main":

                                if (tank.setShotDelay)
                                {
                                    break;
                                }
                                else
                                {
                                    lock (projectiles)
                                    {
                                        projectiles[tank.Id] = new Projectiles();
                                        projectiles[tank.Id].OwnerID = tank.Id;
                                        projectiles[tank.Id].Id = tank.Id;
                                        projectiles[tank.Id].Location = tank.location;
                                        projectiles[tank.Id].Orientation = tank.aiming;
                                    }
                                    tank.setShotDelay = true;
                                }
                                break;

                            case "alt":
                                if (tank.powerUpCount != 0 && !tank.died)
                                {
                                    beams[tank.Id] = new Beams(tank.location, tank.aiming, tank.Id);
                                    tank.powerUpCount--;
                                    tank.setBeamDelay = true;
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Section for Collisions of projectiles, tanks, and power ups to be used in the
        /// Update method. 
        /// </summary>
        private void CollisionSection()
        {
            //Projectiles && Projectile Collision
            lock (projectiles)
            {

                foreach (Projectiles proj in projectiles.Values)
                {
                    //per the assignment: Projectiles are created with a velocity in the direction of the tank turret's orientation,
                    //with length equal to server's definition of projectile speed

                    //The vector math is done by normalizing the orientation. The location is what we need to be changed
                    //every frame. Using the orientation of the tank, and using a new vector to do the math, we multiply it 
                    //by the scalar of the projectile speed. This causes the projectile to move at the desired frame rate. If we do not
                    //scale the vector it moves very slow. Finally, we add the vector to the location of the projectiles vector to change it
                    //everytime the update method is called. 
                    proj.Orientation.Normalize();
                    Vector2D newLocationVector = proj.Orientation;
                    newLocationVector *= Projectiles.projectileSpeed;
                    proj.Location += newLocationVector;

                    // COLLISION CHECKS FOR PROJECTILES
                    foreach (Wall wall in walls.Values)
                    {
                        if (wall.WallCollisionCheck(proj.Location, proj.Size))
                        {
                            proj.Died = true;
                        }
                    }

                    //COLLISION CHECKS FOR TANKS
                    //If a tank has the same id as the projectile than it is its own and we do not let 
                    //the tank hurt itself. If not then we check if a projectile is already dead and if we 
                    //collide with a tank.
                    foreach (Tank tank in tanks.Values)
                    {
                        bool friendlyTank = (tank.Id == proj.Id);

                        if (CollisionDetection(tank.location, proj.Location) && !friendlyTank && !proj.Died)
                        {
                            proj.Died = true;
                            tank.hitPoints--;

                            if (tank.hitPoints == 0)
                            {
                                tank.died = true;
                                tank.Velocity = new Vector2D(0, 0);
                                proj.scoreKeeper = true;
                            }
                        }
                    }
                }
            }

            lock (tanks)
            {

                //Tank Collision
                CtrlCmds.Clear();
                foreach (Tank tank in tanks.Values)
                {
                    if (tank.Velocity.Length() == 0)
                        continue;

                    Vector2D newLoc = tank.location + tank.Velocity;
                    bool collision = false;

                    //Tank to wall collsions
                    foreach (Wall wall in walls.Values)
                    {
                        if (wall.CollidesTank(newLoc))
                        {
                            collision = true;
                            tank.Velocity = new Vector2D(0, 0);
                            break;
                        }
                    }

                    if (!collision)
                    {
                        tank.location = newLoc;

                    }

                    lock (powerups)
                    {
                        //Tank to powerup collisions
                        foreach (Powerups powerup in powerups.Values)
                        {

                            if (CollisionDetection(tank.location, powerup.location) && !powerup.died)
                            {
                                powerup.died = true;

                                //A tank can only have 3 powerups max
                                if (tank.powerUpCount < 3)
                                {
                                    tank.powerUpCount++;
                                }
                            }
                        }
                    }
                }
            }

            //Beam collision
            lock (beams)
            {

                foreach (Beams beam in beams.Values)
                {
                    Tank oldTank = new Tank();

                    foreach (Tank tank in tanks.Values)
                    {
                        if (Intersects(beam.origin, beam.direction, tank.location, Tank.Size / 2))
                        {
                            tank.died = true;
                            tank.Velocity = new Vector2D(0, 0);
                            tank.hitPoints = 0;
                            beam.scoreKeeper++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A section that is used as the Respawn for a tanks and powerups to be used
        /// in the Update method. 
        /// </summary>
        private void RespawnSection()
        {
            lock (tanks)
            {
                foreach (Powerups powerup in powerups.Values)
                {
                    //checks for any dead powerup, if so it goes through a delay and uses
                    //the Respawn method.
                    if (powerup.died)
                    {
                        powerup.spawnTimer++;

                        if (powerup.spawnTimer == powerup.RandomLocation())
                        {
                            Respawn(powerup);
                        }
                    }
                }

                foreach (Tank tank in tanks.Values)
                {
                    // if there are any disconnected tanks, it assigns their fields first
                    if (tank.disconnected)
                    {
                        tank.died = true;
                        tank.hitPoints = 0;
                    }

                    //checks any dead tank in the dictionary
                    if (tank.died)
                    {
                        tank.RespawnRate++;

                        //if a tank is dead & disconnected, we remove it from the dictionary after 30 frames
                        if (tank.disconnected && tank.RespawnRate == 30)
                        {
                            Console.WriteLine("Player: " + tank.name + " ID: " + tank.Id + " Disconnected!");
                            tanks.Remove(tank.Id);
                        }

                        //if a tank is not disconnected, it will delay it by the respawn rate
                        //set in the settings and respawn the tank. 
                        if (tank.RespawnRate == RespawnRate && !tank.disconnected)
                        {
                            Respawn(tank);
                            tank.powerUpCount = 0;
                            tank.RespawnRate = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A section to be used as the delay shooting and scoring logic to be used in the
        /// Update method. Delays a shot or beam that depends on the value. Calculates a score 
        /// for a projectile and calculates the score for a beam. A beam can have more than one score
        /// per beam that depends on the amount of { player object } in its trajectory. 
        /// </summary>
        private void DelayScoringSection()
        {
            lock (tanks)
            {
                //Checks if the field setShotDelay is true, than it would mean a tank shot.
                //If so the tank will start a delay (dependent on the settings) before a new
                //shot can be made
                foreach (Tank tank in tanks.Values)
                {
                    if (tank.setShotDelay)
                    {
                        tank.shotFPS++;
                        if (tank.shotFPS == FramePerShot)
                        {
                            tank.setShotDelay = false;
                            tank.shotFPS = 0;
                        }
                    }

                    //checks if the field setBeamDelay is true, that it would mean a beam was shot.
                    //If so the tank will start a delay that is set at 6 frames before the beam disappears. 
                    //These 6 frames can catch any tank in its path.
                    foreach (Beams beam in beams.Values)
                    {
                        if (tank.setBeamDelay)
                        {
                            tank.beamFPS++;

                            //need to calculate at frame one or we encounter a the score to be added every frame. 
                            if (tank.beamFPS == 1)
                            {
                                tank.score += beam.scoreKeeper;
                            }
                            if (tank.beamFPS == 6)
                            {
                                tank.setBeamDelay = false;
                                tank.beamFPS = 0;
                                beams.Remove(tank.Id);
                            }
                        }
                    }

                    //Calculates the tanks score if it finds the projectiles scoreKeeper to be true.
                    //If so, the tank score increases by one. 
                    foreach (Projectiles projectile in projectiles.Values)
                    {
                        if (tank.Id == projectile.Id && projectile.scoreKeeper)
                        {
                            tank.score++;
                            projectile.scoreKeeper = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A collision detection that uses a similar method that was explained in 
        /// lecture 23. It grabs the value of subtracting two points and seeing if its within
        /// the range of 30. If both x and y ranges are within it then the method
        /// return true or else it will be false. Can be used to detect if a collision is made 
        /// between two vectors
        /// </summary>
        /// <param name="tank">Vector point 1</param>
        /// <param name="worldObject">Vector point 2</param>
        /// <returns>true if there is a detection else false</returns>
        public bool CollisionDetection(Vector2D tank, Vector2D worldObject)
        {
            bool xRange = false;
            bool yRange = false;

            double deltaX = Math.Abs(tank.GetX() - worldObject.GetX());
            double deltaY = Math.Abs(tank.GetY() - worldObject.GetY());

            //x range           
            if (deltaX <= 30)
            {
                xRange = true;
            }

            //y range
            if (deltaY <= 30)
            {
                yRange = true;
            }

            bool withinRange = (xRange && yRange);

            return withinRange;
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
            lock (tanks)
            {
                if (obj is Powerups p)
                {
                    bool collision = true;
                    Random random = new Random();
                    Vector2D newSpawn = new Vector2D(random.Next(-worldSize / 2, worldSize / 2),
                    (random.Next(-worldSize / 2, worldSize / 2)));

                    int counter = 0;

                    //Goes through a loop and checks the vector, if that vector collides with any wall
                    //a new vector is made until it finds a new one. Uses a counting method to check
                   //every wall. 
                    while (collision)
                    {
                        newSpawn = new Vector2D(random.Next(-worldSize / 2, worldSize / 2),
                            (random.Next(-worldSize / 2, worldSize / 2)));

                        foreach (Wall wall in walls.Values)
                        {
                            if (wall.WallCollisionCheck(newSpawn, 30))
                            {
                                counter++;
                            }
                        }

                        if (counter == 0)
                        {
                            p.location = newSpawn;
                            p.died = false;
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
                    Vector2D newSpawn = new Vector2D(random.Next(-worldSize / 2, worldSize / 2),
                    (random.Next(-worldSize / 2, worldSize / 2)));

                    int counter = 0;

                    while (collision)
                    {
                        //Goes through a loop and checks the vector, if that vector collides with any wall
                        //a new vector is made until it finds a new one. Uses a counting method to check
                        //every wall. 
                        newSpawn = new Vector2D(random.Next(-worldSize / 2, worldSize / 2),
                            (random.Next(-worldSize / 2, worldSize / 2)));

                        foreach (Wall wall in walls.Values)
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

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }
    }
}
