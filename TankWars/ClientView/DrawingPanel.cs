using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

namespace TankWars
{
    /// <summary>
    /// This is the drawing that is used by the view to dispaly the world to the user.
    /// This has an onPaint method that will redraw the world every frame received by the 
    /// game controller.  Holds the images and sounds that are displayed.
    /// 
    /// @Author Adam Scott & Andrew Porter
    /// </summary>
    public class DrawingPanel : Panel
    {
        //Variable containing the current world of the game
        private World World;
        private int selfTankID;
        private int timescalled;

        //Images for Tanks, walls, and background
        readonly Image wallSegment = Image.FromFile(@"..\..\..\Resources\Images\WallSprite.png");
        readonly Image background = Image.FromFile(@"..\..\..\Resources\Images\Background.png");
        readonly Image redTank = Image.FromFile(@"..\..\..\Resources\Images\RedTank.png");
        readonly Image yellowTank = Image.FromFile(@"..\..\..\Resources\Images\YellowTank.png");
        readonly Image blueTank = Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png");
        readonly Image darkTank = Image.FromFile(@"..\..\..\Resources\Images\DarkTank.png");
        readonly Image greenTank = Image.FromFile(@"..\..\..\Resources\Images\GreenTank.png");
        readonly Image purpleTank = Image.FromFile(@"..\..\..\Resources\Images\PurpleTank.png");
        readonly Image orangeTank = Image.FromFile(@"..\..\..\Resources\Images\OrangeTank.png");
        readonly Image lightGreenTank = Image.FromFile(@"..\..\..\Resources\Images\LightGreenTank.png");

        //Images for turret and projectiles
        readonly Image blueTurret = Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png");
        readonly Image darkTurret = Image.FromFile(@"..\..\..\Resources\Images\DarkTurret.png");
        readonly Image lgTurret = Image.FromFile(@"..\..\..\Resources\Images\LightGreenTurret.png");
        readonly Image greenTurret = Image.FromFile(@"..\..\..\Resources\Images\GreenTurret.png");
        readonly Image redTurret = Image.FromFile(@"..\..\..\Resources\Images\RedTurret.png");
        readonly Image orangeTurret = Image.FromFile(@"..\..\..\Resources\Images\OrangeTurret.png");
        readonly Image yellowTurret = Image.FromFile(@"..\..\..\Resources\Images\YellowTurret.png");
        readonly Image purpleTurret = Image.FromFile(@"..\..\..\Resources\Images\PurpleTurret.png");
        readonly Image blueShot = Image.FromFile(@"..\..\..\Resources\Images\shot_blue.png");
        readonly Image greyShot = Image.FromFile(@"..\..\..\Resources\Images\shot_grey.png");
        readonly Image redShot = Image.FromFile(@"..\..\..\Resources\Images\shot-red.png");
        readonly Image violetShot = Image.FromFile(@"..\..\..\Resources\Images\shot_violet.png");
        readonly Image brownShot = Image.FromFile(@"..\..\..\Resources\Images\shot-brown.png");
        readonly Image greenShot = Image.FromFile(@"..\..\..\Resources\Images\shot-green.png");
        readonly Image yellowShot = Image.FromFile(@"..\..\..\Resources\Images\shot-yellow.png");
        readonly Image powerup = Image.FromFile(@"..\..\..\Resources\Images\powerUp.png");

        //Images for explosion
        readonly Image ex1 = Image.FromFile(@"..\..\..\Resources\Images\bubble_explo2.png");
        readonly Image ex2 = Image.FromFile(@"..\..\..\Resources\Images\bubble_explo3.png");
        readonly Image ex3 = Image.FromFile(@"..\..\..\Resources\Images\bubble_explo4.png");
        readonly Image ex4 = Image.FromFile(@"..\..\..\Resources\Images\bubble_explo5.png");

        //Sounds and flags for shot sounds and explosions. Flags are kept to make sure they do not overlap themselves
        private System.Media.SoundPlayer explosionSound = new System.Media.SoundPlayer(@"..\..\..\Resources\Images\bangship.wav");
        private System.Media.SoundPlayer standardShotSound = new System.Media.SoundPlayer(@"..\..\..\Resources\Images\multimedia_retro_game_gun_shot.wav");
        private System.Media.SoundPlayer beamShotSound = new System.Media.SoundPlayer(@"..\..\..\Resources\Images\zapsplat_sound_design_buzz_laser_style_44561.wav");
        private bool explosionSoundFlag = true;
        private bool shotSoundFlag = false;
        private bool beamShotSoundFlag = false;

        //Paintbrushes
        private Pen redPen = new Pen(Color.Red);
        private Pen yellowPen = new Pen(Color.Yellow);
        private Pen greenPen = new Pen(Color.LawnGreen);
        //Create a pen to draw a whiteline
        Pen whitePen = new Pen(Color.White, 2);

        //Font for the player display name
        private Font playerStyle = new Font("Ariel", 12, FontStyle.Regular);

        //The constructor for the DrawingPanwl
        public DrawingPanel(World theWorld)
        {
            this.DoubleBuffered = true;
            World = theWorld;
        }

        /// <summary>
        /// Method that sets the drawing panel's world
        /// </summary>
        /// <param name="theWorld"></param>
        public void SetWorld(World theWorld)
        {
            World = theWorld;
        }

        /// <summary>
        /// Method that sets the Player ID number
        /// </summary>
        /// <param name="IDnum"></param>
        public void SetPlayerId(int IDnum)
        {
            selfTankID = IDnum;
        }

        /// <summary>
        /// Helper method to convert world coordinates to image coordinates
        /// </summary>
        /// <param name="size"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        //Delegate for object drawing methods
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// Helper method that draws objects onto the drawing panel
        /// </summary>
        /// <param name="e"></param>
        /// <param name="o"></param>
        /// <param name="worldSize"></param>
        /// <param name="worldX"></param>
        /// <param name="worldY"></param>
        /// <param name="angle"></param>
        /// <param name="drawer"></param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            //"push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            //"pop" the trasnform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// This method draws the tank to the client
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DrawTank(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            //TODO: find the relative file location of sprit image based on tank ID
            //Switch case here
            int tankID = (t.GetID() % 8);

            // Get the tanks ID number and assign them a tank color based on that
            switch (tankID)
            {
                //blue tank
                case 0:
                    e.Graphics.DrawImage(blueTank, -30, -30, 60, 60);
                    break;
                //dark tank
                case 1:
                    e.Graphics.DrawImage(darkTank, -30, -30, 60, 60);
                    break;
                //green tank
                case 2:
                    e.Graphics.DrawImage(greenTank, -30, -30, 60, 60);
                    break;
                //light green tank
                case 3:
                    e.Graphics.DrawImage(lightGreenTank, -30, -30, 60, 60);
                    break;
                //orange tank
                case 4:
                    e.Graphics.DrawImage(orangeTank, -30, -30, 60, 60);
                    break;
                //purple tank
                case 5:
                    e.Graphics.DrawImage(purpleTank, -30, -30, 60, 60);
                    break;
                //red tank
                case 6:
                    e.Graphics.DrawImage(redTank, -30, -30, 60, 60);
                    break;
                //yellow tank
                case 7:
                    e.Graphics.DrawImage(yellowTank, -30, -30, 60, 60);
                    break;

            }
        }

        /// <summary>
        /// Method will draw the turrets on top of the tanks
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            //Extract tank from object so that we can assign turret color to correct tank
            Tank t = o as Tank;

            //Switch case here based on tank id
            int tankID = (t.GetID() % 8);

            // Get the tanks ID number and assign them a tank color based on that
            switch (tankID)
            {
                //blue turret
                case 0:
                    e.Graphics.DrawImage(blueTurret, -25, -25, 50, 50);
                    break;

                //Dark turret
                case 1:
                    e.Graphics.DrawImage(darkTurret, -25, -25, 50, 50);
                    break;

                //Green turret
                case 2:
                    e.Graphics.DrawImage(greenTurret, -25, -25, 50, 50);
                    break;

                //Light Green turret
                case 3:
                    e.Graphics.DrawImage(lgTurret, -25, -25, 50, 50);
                    break;
                //Orange turret
                case 4:
                    e.Graphics.DrawImage(orangeTurret, -25, -25, 50, 50);
                    break;

                //Purple turret
                case 5:
                    e.Graphics.DrawImage(purpleTurret, -25, -25, 50, 50);
                    break;

                //Red turret
                case 6:
                    e.Graphics.DrawImage(redTurret, -25, -25, 50, 50);
                    break;

                //Yellow turret
                case 7:
                    e.Graphics.DrawImage(yellowTurret, -25, -25, 50, 50);
                    break;
            }
        }

        /// <summary>
        /// Method that draws the wall objects
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            //Variables for the loops
            int start;
            int end;
            Wall w = o as Wall;

            //Check if Vertical wall segment
            if (w.GetP1().GetX() == w.GetP2().GetX())
            {
                //set offset from center of map
                int horizontalOffset = (int)w.GetP1().GetX() - 25;

                //Determines which point is start and which is end
                if ((int)w.GetP1().GetY() > (int)w.GetP2().GetY())
                {
                    start = (int)w.GetP2().GetY();
                    end = (int)w.GetP1().GetY();
                }
                else
                {
                    start = (int)w.GetP1().GetY();
                    end = (int)w.GetP2().GetY();
                }

                //Draw vertical walls
                for (int i = start; i <= end; i += 50)
                    e.Graphics.DrawImage(wallSegment, horizontalOffset, i - 25, 50, 50);
            }
            else //its a horizontal wall segment
            {
                //Determine vertical offset from center of map
                int verticalOffset = (int)w.GetP1().GetY() - 25;

                //Determine which point is end and which is start
                if ((int)w.GetP1().GetX() > (int)w.GetP2().GetX())
                {
                    start = (int)w.GetP2().GetX();
                    end = (int)w.GetP1().GetX();
                }
                else
                {
                    start = (int)w.GetP1().GetX();
                    end = (int)w.GetP2().GetX();
                }

                //Draw horizontal wall segments
                for (int i = start; i <= end; i += 50)
                    e.Graphics.DrawImage(wallSegment, i - 25, verticalOffset, 50, 50);
            }
        }

        /// <summary>
        /// Draws Powerup
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void PowerUpDrawer(object o, PaintEventArgs e)
        {
            PowerUp p = o as PowerUp;

            e.Graphics.DrawImage(powerup, -8, -8);
        }

        /// <summary>
        /// This is the method that draws projectiles
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;

            //Switch case here
            int projID = (p.getOwner() % 8);
            if (shotSoundFlag)
            {
                standardShotSound.Play();
                shotSoundFlag = false;
            }
            // Get the projectile's ID number and assign them a tank color based on that
            switch (projID)
            {

                case 0:
                    e.Graphics.DrawImage(blueShot, -15, -15, 30, 30);
                    break;
                case 1:
                    e.Graphics.DrawImage(greyShot, -15, -15, 30, 30);
                    break;
                case 2:
                    e.Graphics.DrawImage(redShot, -15, -15, 30, 30);
                    break;
                case 3:
                    e.Graphics.DrawImage(yellowShot, -15, -15, 30, 30);
                    break;
                case 4:
                    e.Graphics.DrawImage(blueShot, -15, -15, 30, 30);
                    break;
                case 5:
                    e.Graphics.DrawImage(violetShot, -15, -15, 30, 30);
                    break;
                case 6:
                    e.Graphics.DrawImage(redShot, -15, -15, 30, 30);
                    break;
                case 7:
                    e.Graphics.DrawImage(yellowShot, -15, -15, 30, 30);
                    break;
            }
        }

        public void StandardShotSoundChanger()
        {
            shotSoundFlag = true;
        }

        /// <summary>
        /// Draws the beam attack
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            if (beamShotSoundFlag)
            {
                beamShotSound.Play();
                beamShotSoundFlag = false;
            }

            Beam b = o as Beam;

            //Smooths out the beam
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Create points for beam attack
            Point endx = new Point(0, -1000);
            Point start = new Point(0, 0);

            //Draw beam
            e.Graphics.DrawLine(whitePen, start, endx);
        }

        public void BeamShotSoundChanger()
        {
            beamShotSoundFlag = true;
        }

        /// <summary>
        /// Draws the healthbar above the tank
        /// </summary>
        /// <param name="e"></param>
        /// 
        private void HealthDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            int health = t.HealthLevel;
            Rectangle healthbar;

            //Switch for health level bar
            switch (health)
            {
                //Do nothing for 0 health
                case 0:
                    break;

                //Draw Tiny Red Health Bar
                case 1:
                    healthbar = new Rectangle(-15, -40, 15, 5);
                    e.Graphics.FillRectangle(redPen.Brush, healthbar);
                    break;

                //Draw smaller Yellow HealthBar
                case 2:
                    healthbar = new Rectangle(-15, -40, 30, 5);
                    e.Graphics.FillRectangle(yellowPen.Brush, healthbar);
                    break;

                //Draw Green Health Bar
                case 3:
                    healthbar = new Rectangle(-23, -40, 45, 5);
                    e.Graphics.FillRectangle(greenPen.Brush, healthbar);
                    break;
            }
        }

        /// <summary>
        /// Method to draw player name and score
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void NameDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;
            byte[] imageBytes = Encoding.Unicode.GetBytes(t.GetName());
            e.Graphics.DrawString(t.GetName() + ": " + t.GetScore().ToString(), playerStyle, whitePen.Brush, -20, 27);
        }

        //Explodes the tank
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            //tank has died and draw explosion animation in it's place
            // if that is the case.
            timescalled++;
            Random rand = new Random();
            int explosion = rand.Next(0, 4);
            // We check to see how many times the explosion drawer has been called
            // and if it has been called over 250 times we know that our sound has ended
            // and want to replay it.
            if (timescalled > 150)
            {
                explosionSoundFlag = true;
            }
            if (explosionSoundFlag)
            {
                explosionSound.Play();
                explosionSoundFlag = false;
                timescalled = 0;
            }

            switch (explosion)
            {
                case 0:
                    e.Graphics.DrawImage(ex1, -30, -30, 45, 45);
                    break;
                case 1:
                    e.Graphics.DrawImage(ex2, -30, -10, 45, 45);
                    break;
                case 2:
                    e.Graphics.DrawImage(ex3, 0, -40, 45, 45);
                    break;
                case 3:
                    e.Graphics.DrawImage(ex4, -20, 0, 45, 45);
                    break;
            }
            return;
        }

        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            //Do not draw anything if the world has not been assigned yet
            if (World == null)
                return;

            //We want to make sure that the player (us) have been assigned an ID by the world first
            if (World.Tanks.ContainsKey(selfTankID))
            {
                double playerX = World.Tanks[selfTankID].Location.GetX();// (the player's world-space X coordinate)
                double playerY = World.Tanks[selfTankID].Location.GetY();//... (the player's world-space Y coordinate)

                // calculate view/world size ratio
                double ratio = (double)850 / (double)World.Size;
                int halfSizeScaled = (int)(World.Size / 2.0 * ratio);

                double inverseTranslateX = -WorldSpaceToImageSpace(World.Size, playerX) + halfSizeScaled;
                double inverseTranslateY = -WorldSpaceToImageSpace(World.Size, playerY) + halfSizeScaled;

                e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);
            }

            //Draw the world
            Rectangle rect = new Rectangle(0, 0, World.Size, World.Size);
            e.Graphics.DrawImage(background, rect);

            lock (World.Tanks)
            {
                // Draw the players
                foreach (Tank tank in World.Tanks.Values)
                {
                    //Draw tank body, then turret then the name
                    DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), tank.Orientation.ToAngle(), DrawTank);
                    DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), tank.AimDirection.ToAngle(), TurretDrawer);
                    DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), 0, NameDrawer);

                    //Draw the health level 
                    DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), 0, HealthDrawer);

                    //If a tank has 0 health, trigger the explosion
                    if (tank.HealthLevel == 0)
                        DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), 0, ExplosionDrawer);
                }
            }

            lock (World.PowerUps)
            {
                // Draw the powerups
                foreach (PowerUp pow in World.PowerUps.Values)
                {
                    DrawObjectWithTransform(e, pow, World.Size, pow.position.GetX(), pow.position.GetY(), 0, PowerUpDrawer);
                }
            }

            lock (World.Projectiles)
            {
                //Draw projectiles
                foreach (Projectile proj in World.Projectiles.Values)
                {
                    DrawObjectWithTransform(e, proj, World.Size, proj.Location.GetX(), proj.Location.GetY(), World.Tanks[proj.getOwner()].AimDirection.ToAngle(), ProjectileDrawer);
                }
            }

            lock (World.Walls)
            {
                //Draw walls
                foreach (Wall wall in World.Walls.Values)
                {
                    DrawObjectWithTransform(e, wall, World.Size, 0, 0, 0, WallDrawer);
                }
            }
            lock (World.Beams)
            {
                if (World.Beams.Count > 0)
                {
                    //Draw beams
                    foreach (Beam beam in World.Beams.Values)
                    {
                        beam.Direction.Normalize();
                        DrawObjectWithTransform(e, beam, World.Size, beam.Origin.GetX(), beam.Origin.GetY(), beam.Direction.ToAngle(), BeamDrawer);
                    }
                }
            }
            //Let the base form do anything it needs to move on
            base.OnPaint(e);
        }
    }
}
