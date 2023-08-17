using GC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace View
{
    /// <summary>
    /// Represents a Panel in which the user can draw objects
    /// </summary>
    public class DrawingPanel : Panel
    {

        /// <summary>
        /// A delegate for DrawObjectWithTransform
        /// Methods matching this delegate can draw whatever they want using e  
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// Represent the world where all the objects 
        /// </summary>
        private World theWorld;

        /// <summary>
        /// Represents a list of images for the tanks
        /// </summary>
        private List<Image> tanks = new List<Image>();

        /// <summary>
        /// Represents a list of images for the turrent
        /// </summary>
        private List<Image> turrets = new List<Image>();

        /// <summary>
        /// Represents a list of images for the projectiles
        /// </summary>
        private List<Image> shots = new List<Image>();

        /// <summary>
        /// Represents the background image
        /// </summary>
        private Image background;

        /// <summary>
        /// Represent the sprite for the wall
        /// </summary>
        private Image wallSprite;

        /// <summary>
        /// Represents the explosion
        /// </summary>
        private Image explosionImage;

        /// <summary>
        /// Represents the world size send by the controller
        /// </summary>
        private int worldSize;

        /// <summary>
        /// Represents the controller instance of the game
        /// </summary>
        private GameController controller;

        /// <summary>
        /// Represents the player's Id
        /// </summary>
        private int currPlayerID;

        /// <summary>
        /// Represents the turrent location
        /// </summary>
        private Vector2D TurrentLocation;

        /// <summary>
        /// Constructor for the Drawing panel class
        /// </summary>
        /// <param name="w"></param>
        /// <param name="controller"></param>
        public DrawingPanel(World w, GameController controller)
        {
            DoubleBuffered = true;
            theWorld = w;
            this.controller = controller;
            controller.GetInfo += getPlayerID;
            controller.TurrentLocation += getTurrentLocation;
            
            //Background and general images
            background = Image.FromFile("../../../Resources/Images/Background.png");
            wallSprite = Image.FromFile("../../../Resources/Images/WallSprite.png");
            explosionImage = Image.FromFile("../../../Resources/Images/explosion.jpg");

            //turrets images
            Image blueTurret = Image.FromFile("../../../Resources/Images/BlueTurret.png");
            turrets.Add(blueTurret);
            Image darkTurret = Image.FromFile("../../../Resources/Images/DarkTurret.png");
            turrets.Add(darkTurret);
            Image greenTurret = Image.FromFile("../../../Resources/Images/GreenTurret.png");
            turrets.Add(greenTurret);
            Image lightGreenTurret = Image.FromFile("../../../Resources/Images/LightGreenTurret.png");
            turrets.Add(lightGreenTurret);
            Image orangeTurret = Image.FromFile("../../../Resources/Images/OrangeTurret.png");
            turrets.Add(orangeTurret);
            Image purpleTurret = Image.FromFile("../../../Resources/Images/PurpleTurret.png");
            turrets.Add(purpleTurret);
            Image redTurret = Image.FromFile("../../../Resources/Images/RedTurret.png");
            turrets.Add(redTurret);
            Image yellowTurret = Image.FromFile("../../../Resources/Images/YellowTurret.png");
            turrets.Add(yellowTurret);

            //tanks images
            Image blueTank = Image.FromFile("../../../Resources/Images/BlueTank.png");
            tanks.Add(blueTank);
            Image darkTank = Image.FromFile("../../../Resources/Images/DarkTank.png");
            tanks.Add(darkTank);
            Image greenTank = Image.FromFile("../../../Resources/Images/GreenTank.png");
            tanks.Add(greenTank);
            Image lightGreenTank = Image.FromFile("../../../Resources/Images/LightGreenTank.png");
            tanks.Add(lightGreenTank);
            Image orangeTank = Image.FromFile("../../../Resources/Images/OrangeTank.png");
            tanks.Add(orangeTank);
            Image purpleTank = Image.FromFile("../../../Resources/Images/PurpleTank.png");
            tanks.Add(purpleTank);
            Image redTank = Image.FromFile("../../../Resources/Images/RedTank.png");
            tanks.Add(redTank);
            Image yellowTank = Image.FromFile("../../../Resources/Images/YellowTank.png");
            tanks.Add(yellowTank);

            //shots images
            Image shotBlue = Image.FromFile("../../../Resources/Images/shot-blue.png");
            shots.Add(shotBlue);
            Image shotBrown = Image.FromFile("../../../Resources/Images/shot-brown.png");
            shots.Add(shotBrown);
            Image shotGreen = Image.FromFile("../../../Resources/Images/shot-green.png");
            shots.Add(shotGreen);
            Image shotGrey = Image.FromFile("../../../Resources/Images/shot-grey.png");
            shots.Add(shotGrey);
            Image shotRed = Image.FromFile("../../../Resources/Images/shot-red.png");
            shots.Add(shotRed);
            Image shotViolet = Image.FromFile("../../../Resources/Images/shot-violet.png");
            shots.Add(shotViolet);
            Image shotWhite = Image.FromFile("../../../Resources/Images/shot-white.png");
            shots.Add(shotWhite);
            Image shotYellow = Image.FromFile("../../../Resources/Images/shot-yellow.png");
            shots.Add(shotYellow);
        }

        /// <summary>
        /// Gets the turrent's location
        /// </summary>
        /// <param name="v"></param>
        private void getTurrentLocation(Vector2D v)
        {
            TurrentLocation = v;
        }

        /// <summary>
        /// Gets the player's ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="size"></param>
        public void getPlayerID(int id, int size)
        {
            currPlayerID = id;
            worldSize = size;
        }

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
           

        }

        /// <summary>
        /// Draws the wall image
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;

            int width = 50;
            int height = 50;

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
         
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
            e.Graphics.DrawImage(wallSprite, r);
        }

        /// <summary>
        /// Draws the tanks
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            int colorID = tank.Id % tanks.Count;
            Image tankImage = tanks[colorID];

            int tankWidth = 60;
            int tankHeight = 60;

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle tankRec = new Rectangle(-(tankWidth / 2), -(tankHeight / 2), tankWidth, tankHeight);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            e.Graphics.DrawImage(tankImage, tankRec);
        }

        /// <summary>
        /// Draws the turrents
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurrentDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;
            int colorID = tank.Id % tanks.Count;
            Image turretImage = turrets[colorID];
  
            int turretWidth = 50;
            int turretHeight = 50;
            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle turretRec = new Rectangle(-(turretWidth / 2), -(turretHeight / 2), turretWidth, turretHeight);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(turretImage, turretRec);
        }

        /// <summary>
        /// Draws the tank's information such as health bar, ID and name
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankInfoDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;

            int tankWidth = 50;
            int tankHeight = 50;

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle turretRec = new Rectangle(-(tankWidth / 2), -(tankHeight / 2), tankWidth, tankHeight);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //drawing the name and Id number
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 12);
            System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.AliceBlue);
            e.Graphics.DrawString(tank.name + " ID: " + tank.Id, drawFont, whiteBrush, -30, 30);

            //drawing the hitpoints
            System.Drawing.SolidBrush healthBarBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);
            Rectangle healthBarRec = new Rectangle(-(tankWidth / 2), -(tankHeight / 2) - 15, tankWidth, tankHeight - 45);

            if (tank.hitPoints >= 3)
            {
                e.Graphics.FillRectangle(healthBarBrush, healthBarRec);
            }
            else if (tank.hitPoints == 2)
            {
                healthBarBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow);
                healthBarRec = new Rectangle(-(tankWidth / 2), -(tankHeight / 2) - 15, tankWidth - 20, tankHeight - 45);
                e.Graphics.FillRectangle(healthBarBrush, healthBarRec);
            }
            else if (tank.hitPoints == 1)
            {
                healthBarBrush = new System.Drawing.SolidBrush(System.Drawing.Color.OrangeRed);
                healthBarRec = new Rectangle(-(tankWidth / 2), -(tankHeight / 2) - 15, tankWidth - 40, tankHeight - 45);
                e.Graphics.FillRectangle(healthBarBrush, healthBarRec);
            }
        }


        /// <summary>
        /// Draws the power ups
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerups p = o as Powerups;

            int width = 8;
            int height = 8;

            int width2 = 12;
            int height2 = 12;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                // Circles are drawn starting from the top-left corner.
                // So if we want the circle centered on the powerup's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
                Rectangle r2 = new Rectangle(-(width2 / 2), -(height2 / 2), width2, height2);

                e.Graphics.FillEllipse(yellowBrush, r2);
                e.Graphics.FillEllipse(redBrush, r);
            }
        }

        /// <summary>
        /// Draws the projectiles
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ShotDrawer(object o, PaintEventArgs e)
        {
            Projectiles p = o as Projectiles;
            int colorID = currPlayerID % tanks.Count;
             Image shotImage = shots[colorID];

            int width = 30;
            int height = 30;

            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(shotImage, r);
        }

        /// <summary>
        /// Draws the explosion's image
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            Projectiles p = o as Projectiles;

            int width = 30;
            int height = 30;

            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(explosionImage, r);
        }

        /// <summary>
        /// Draws the Beams
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beams B = o as Beams;

            Point p1 = new Point(0, 0);
            Point p2 = new Point(0, -worldSize);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.AliceBlue))
            {
                e.Graphics.DrawLine(pen, p1,p2);
            }
        }


        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (theWorld)
            {
                // don't draw until we have at least a tank
                if (theWorld.tanks.Count != 0)
                {
                    this.BackColor = Color.Black;
                    int viewSize = Size.Width;

                    // Center player's view as it moves
                    float playerX = (float)theWorld.tanks[currPlayerID].location.GetX();
                    float playerY = (float)theWorld.tanks[currPlayerID].location.GetY();
                    e.Graphics.TranslateTransform(-playerX + (viewSize / 2), -playerY + (viewSize / 2));
                    e.Graphics.DrawImage(background, (-worldSize / 2), (-worldSize / 2), worldSize, worldSize);

                    foreach (Tank tank in theWorld.tanks.Values)
                    {
                        if (tank.hitPoints == 0)
                        {
                            // draw explosion image
                            DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), 0,
                                ExplosionDrawer);
                            continue;
                        }
                        // draw tank, turrent and tank info
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(),
                       TankDrawer);
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.aiming.ToAngle(),
                      TurrentDrawer);
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), 0, TankInfoDrawer);
                    }

                    foreach (Wall w in theWorld.walls.Values)
                    {
                        //Checks if location of the wall if it is veritcal or horizontal
                        bool result = Math.Abs(w.p1.GetX() - w.p2.GetX()) <= 0;
                        int offset = 50;
                        if (result)
                        {
                            //horizontal wall positioning. If position1 y is greater that means
                            // that we need to off set it by subtracting to find the positioning
                            //due to the way the vectors work in the map. Cannot do i++ since
                            //it misses drawing some boxes.
                            if (w.p1.GetY() > w.p2.GetY())
                            {
                                for (double location = w.p1.GetY(); location >= w.p2.GetY(); location -= offset)
                                {
                                    //Here we draw using the x cord and then by the location. No angles
                                    DrawObjectWithTransform(e, w, w.p1.GetX(), location, 0, WallDrawer);
                                }
                            }
                            else
                            {
                                //horizontal wall positioning. If position1 y is less than that means
                                //that we need to off set it by adding to find the positioning
                                //due to the way the vectors work in the map. Cannot do i++ since
                                //it misses drawing some boxes
                                for (double location = w.p1.GetY(); location <= w.p2.GetY(); location += offset)
                                {
                                    //Here we draw using the x cord and then by the location. No angles
                                    DrawObjectWithTransform(e, w, w.p1.GetX(), location, 0, WallDrawer);
                                }
                            }
                        }
                        else
                        {
                            //Vertical wall positioning. If position1 x is greater that means
                            // that we need to off set it by adding to find the positioning
                            //due to the way the vectors work in the map. Cannot do i++ since
                            //it misses drawing some boxes
                            if (w.p1.GetX() > w.p2.GetX())
                            {
                                for (double location = w.p1.GetX(); location >= w.p2.GetX(); location -= offset)
                                {
                                    //Since we are now drawing the vertical positioning we must now draw
                                    //using the worldy and location using worldx.
                                    DrawObjectWithTransform(e, w, location, w.p1.GetY(), 0, WallDrawer);
                                }
                            }
                            else
                            {
                                //Vertical wall positioning. If position1 x is less than that means
                                // that we need to off set it by adding to find the positioning
                                //due to the way the vectors work in the map. Cannot do i++ since
                                //it misses drawing some boxes
                                for (double location = w.p1.GetX(); location <= w.p2.GetX(); location += offset)
                                {
                                    //Since we are now drawing the vertical positioning we must now draw
                                    //using the worldy and location using worldx.
                                    DrawObjectWithTransform(e, w, location, w.p1.GetY(), 0, WallDrawer);
                                }
                            }
                        }
                    }

                    foreach (Powerups p in theWorld.powerups.Values)
                    {
                        if (p.died)
                        {
                            continue;
                        }

                        DrawObjectWithTransform(e, p, p.location.GetX(), p.location.GetY(), 0, PowerupDrawer);
                    }

                    foreach (Projectiles pj in theWorld.projectiles.Values)
                    {
                        if (pj.Died)
                        {
                            continue;
                        }

                        DrawObjectWithTransform(e, pj, pj.Location.GetX(), pj.Location.GetY(), pj.Orientation.ToAngle(), ShotDrawer);
                    }

                    foreach (Beams b in theWorld.beams.Values)
                    {
                        DrawObjectWithTransform(e, b, b.origin.GetX(), b.origin.GetY(), b.direction.ToAngle(), BeamDrawer);
                        
                    }
                    theWorld.beams.Clear();
                    base.OnPaint(e);
                }
                else return;
            }
        }
    }
}