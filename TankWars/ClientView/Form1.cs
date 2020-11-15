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

        //Sets up the view of the client
        private const int viewSize = 500;
        private const int menuSize = 40;
        private DrawingPanel panel;

        //These are our client window comoponents
        Button connect;
        TextBox IPName;
        TextBox playerName;


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

            //Save our boxes to variables
            connect = connectButton;
            IPName = hostText;
            playerName = playerText;

            // Set up key and mouse handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            theController.PlayerIDGiven += InitializeWithID;
            theController.Error += DisplayErrorMessage;
        }

        /// <summary>
        /// This handler method displays a message if one occurrs to the user
        /// </summary>
        /// <param name="err"></param>
        private void DisplayErrorMessage(string err)
        {
            MessageBox.Show(err);
        }

        /// <summary>
        /// Once network sends player ID, we send this to the drawing panel 
        /// This allows us to know which area to center the view on
        /// </summary>
        /// <param name="info"></param>
        private void InitializeWithID(int info)
        {
            panel = new DrawingPanel(theWorld, info);
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

        private void connectButton_Click(object sender, EventArgs e)
        {
            playerName.Enabled = false;
            connectButton.Enabled = false;
            IPName.Enabled = false;
            //Enable the global form to capture key presses
            KeyPreview = true;

            //Make sure user enters a name
            if (playerName.Text == "")
                DisplayErrorMessage("Please enter player name");
            //Connect with Server
            theController.Connect(IPName.Text, playerName.Text);
        }
    }
}