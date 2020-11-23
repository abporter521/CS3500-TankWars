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
    /// <summary>
    /// This is the form class for PS8.  It handles all of the client inputs and communicates
    /// them with the game controller class.  This will display the tank and world
    /// to the user. 
    /// 
    /// @Author Andrew Porter & Adam Scott
    /// </summary>
    public partial class Form1 : Form
    {
        // The controller handles updates from the "server"
        // and notifies us via an event
        private GameController theController;

        // World is a simple container for Players and Powerups
        // The controller owns the world, but we have a reference to it
        private World theWorld;

        private bool worldExists = false;

        //Sets up the view of the client
        private const int viewSize = 850;
        private const int menuSize = 40;
        private DrawingPanel panel;

        //These are our client window comoponents
        TextBox IPName;
        TextBox playerName;


        public Form1(GameController ctl)
        {
            //Set up the general handlers and variables of the form
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
            IPName = hostText;
            playerName = playerText;

            // Set up key and mouse handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;

            //Allow enter button to connect to server
            AcceptButton = connectButton;

            //Set up handlers for controls            
            panel.MouseMove += OnMouseMove;
            panel.MouseClick += HandleMouseClick;
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
            //Set up Drawing Panel with the created world and player ID
            panel.SetWorld(theController.GetWorld());
            panel.SetPlayerId(info);
            theWorld = theController.GetWorld();

            //Set boolean to true so that other events are activated
            worldExists = true;
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
            //handle key directions
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

            //Closes the form with escape key
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }



        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            //Tank movement stopped
            //handle key directions
            if (e.KeyCode == Keys.S)
                theController.MovementStopped("down");
            if (e.KeyCode == Keys.W)
                theController.MovementStopped("up");
            if (e.KeyCode == Keys.A)
                theController.MovementStopped("left");
            if (e.KeyCode == Keys.D)
                theController.MovementStopped("right");
        }

        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseClick(object sender, MouseEventArgs e)
        {
            //Makes sure game is loaded before registering mouse clicks
            if (worldExists)
            {
                //If normal weapon was used
                if (e.Button == MouseButtons.Left)
                {
                    theController.WeaponFire("left");
                    panel.StandardShotSoundChanger();
                }

                //If alternate weapon was used
                if (e.Button == MouseButtons.Right)
                {
                    theController.WeaponFire("right");
                    panel.BeamShotSoundChanger();
                }
            }
        }

        //Tracks mouse movement to move turret
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //If world exists, send mouse movement to controller
            if (worldExists)
            {
                theController.TurretMouseAngle(new Point(e.Location.X, e.Location.Y));
            }
        }

        /// <summary>
        /// This method will connect to the game server when the connect button is clicked
        /// There are flags so that user cannot have empty name or server name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            //Make sure user enters a name
            if (playerName.Text == "")
            {
                DisplayErrorMessage("Please enter player name");
                return;
            }

            //Make sure server name is entered
            if (hostText.Text == "")
            {
                DisplayErrorMessage("Please enter server name");
                return;
            }

            //disable buttons after connecting
            playerName.Enabled = false;
            connectButton.Enabled = false;
            IPName.Enabled = false;
            //Enable the global form to capture key presses
            KeyPreview = true;

            //Connect with Server
            theController.Connect(IPName.Text, playerName.Text);
        }
    }
}