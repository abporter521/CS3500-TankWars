using System;
using System.Collections.Generic;
using System.Drawing;
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

            int width = 60;
            int height = 60;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //TODO: find the relative file location of sprit image based on tank ID
            //Switch case here
            e.Graphics.DrawImage(@"")
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
