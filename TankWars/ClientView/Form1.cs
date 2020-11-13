using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    public partial class Form1 : Form
    {
        // The controller handles updates from the "server"
        // and notifies us via an event
        private GameController theController;

        // World is a simple container for Players and Powerups
        // The controller owns the world, but we have a reference to it
        private World theWorld;

        private const int viewSize = 500;
        private const int menuSize = 40;
        private DrawingPanel panel;

        public Form1(GameController ctl)
        {
            InitializeComponent();
            theController = ctl;
            theWorld = theController.GetWorld();
            theController.UpdateWorld += OnFrame;

            // Set up the form.
            panel = new DrawingPanel(theWorld);
            panel.Location = new Point(0, menuSize);
            panel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(panel);
            // Set the window size
            ClientSize = new Size(viewSize, viewSize + menuSize);

            // Set up key and mouse handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }


        /// <summary>
        /// Handler for the controller's UpdateArrived event
        /// </summary>
        private void OnFrame()
        {
            // Invalidate this form and all its children
            // This will cause the form to redraw as soon as it can
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
        }

        /// <summary>
        /// Key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S)
                theController.Movement("down");
            if (e.KeyCode == Keys.W)
                theController.Movement("up");
            if (e.KeyCode == Keys.A)
                theController.Movement("left");
            if (e.KeyCode == Keys.D)
                theController.Movement("right");

            

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }


        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
                theController.MovementStopped();
        }

        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                theController.WeaponFire("left");
            if(e.Button == MouseButtons.Right)
            {
                theController.WeaponFire("right");
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point mouseLocation;
            mouseLocation = e.Location;
            theController.TurretMouseAngle(mouseLocation);
        }
    }
}