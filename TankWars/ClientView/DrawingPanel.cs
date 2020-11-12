using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    //TODO: CREATE COMMENTS
    //      CREATE OTHER METHODS FOR DRAWING OBJECTS
    //      ADD LOCKS ON DRAW
    class DrawingPanel
    {
        //Variable containing the current world of the game
        private World World;
        //The constructor for the DrawingPanwl
        public DrawingPanel(World theWorld)
        {
            World = theWorld;
        }

        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        public delegate void ObjectDrawer(object o, PaintEventArgs e);
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

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //TODO: find the relative file location of sprit image based on tank ID
            //Switch case here
            int tankID = (t.GetID() % 8);

            // Get the tanks ID number and assign them a tank color based on that
            switch (tankID)
            {

                case 0:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 1:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\DarkTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 2:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\GreenTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 3:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\LightGreenTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 4:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\OrangeTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 5:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\PurpleTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 6:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\RedTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;
                case 7:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\YellowTank.png"), -30, -30);
                    DrawObjectWithTransform(e, t, World.Size, t.Location.GetX(), t.Location.GetY(), t.AimDirection.ToAngle(), turretDrawer);
                    break;

            }
        }

        private void turretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //TODO: find the relative file location of sprit image based on tank ID
            //Switch case here
            int tankID = (t.GetID() % 8);

            // Get the tanks ID number and assign them a tank color based on that
            switch (tankID)
            {

                case 0:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png"), -25, -25);
                    break;
                case 1:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\DarkTurret.png"), -25, -25);
                    break;
                case 2:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\GreenTurret.png"), -25, -25);
                    break;
                case 3:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\LightGreenTankTurret.png"), -25, -25);
                    break;
                case 4:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\OrangeTurret.png"), -25, -25);
                    break;
                case 5:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\PurpleTurret.png"), -25, -25);
                    break;
                case 6:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\RedTurret.png"), -25, -25);
                    break;
                case 7:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\YellowTurret.png"), -25, -25);
                    break;
            }
        }

        private void wallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;

            e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\WallSprite.png"), -25, -25);
        }

        private void powerUpDrawer(object o, PaintEventArgs e)
        {
            PowerUp p = o as PowerUp;

            e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\WallSprite.png"), -8, -8);
        }

        private void projectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //TODO: find the relative file location of sprit image based on projectile ID
            //Switch case here
            int projID = (p.getOwner() % 8);
            // Get the projectile's ID number and assign them a tank color based on that
            switch (projID)
            {

                case 0:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot_blue.png"), -15, -15);
                    break;
                case 1:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot_grey.png"), -15, -15);
                    break;
                case 2:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-green.png"), -15, -15);
                    break;
                case 3:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-green.png"), -15, -15);
                    break;
                case 4:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-brown.png"), -15, -15);
                    break;
                case 5:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot_violet.png"), -15, -15);
                    break;
                case 6:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-red.png"), -15, -15);
                    break;
                case 7:
                    e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-yellow.png"), -15, -15);
                    break;
            }
        }

        private void beamDrawer(object o, PaintEventArgs e)
        {
            Beam b = o as Beam;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            e.Graphics.DrawImage(Image.FromFile(@"..\..\..\Resources\Images\shot-white"), -15, -15);

        }
        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected void OnPaint(PaintEventArgs e)
        {
            lock (World.Tanks)
            {
                // Draw the players
                foreach (Tank tank in World.Tanks.Values)
                {
                    DrawObjectWithTransform(e, tank, World.Size, tank.Location.GetX(), tank.Location.GetY(), tank.Orientation.ToAngle(), DrawTank);
                }
            }

            lock (World.Powerups)
            {
                // Draw the powerups
                foreach (Powerup pow in theWorld.Powerups.Values)
                {
                    DrawObjectWithTransform(e, pow, theWorld.size, pow.GetLocation().GetX(), pow.GetLocation().GetY(), 0, LabDrawer);
                }

                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);
            }
        }
    }
}
