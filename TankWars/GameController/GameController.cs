using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.WebSockets;
using System.Threading;
using System.Windows.Forms;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Timer = System.Threading.Timer;


namespace TankWars
{
    /// <summary>
    /// This is our game controller class.  This class will communicate with the 
    /// game server on behalf of the view.  This controller has methods 
    /// that register tank movement and send it to the server.
    /// The controller also receives data from the server and updates the 
    /// world and notifies the view to redraw
    /// 
    /// @Author Andrew Porter & Adam Scott
    /// </summary>
    public class GameController
    {
        //Event to tell the view to redraw the panel
        public delegate void EventHandler();
        public event EventHandler UpdateWorld;

        //Event that will display an error to the user
        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        //Allows the view to give the drawing panel the players ID to center the game screen
        public delegate void PlayerInfoGiven(int info);
        public event PlayerInfoGiven PlayerIDGiven;

        // State representing the connection with the server
        private SocketState server = null;

        //Tank object representing us
        private Tank selfTank;
        private string playerName;
        private int playerID;

        //bools to register key strokes
        bool leftKeyPressed = false;
        bool rightKeyPressed = false;
        bool upKeyPressed = false;
        bool downKeyPressed = false;
        bool leftClickPressed = false;
        bool rightClickPressed = false;
        bool tankInfoReceived = false;

        //Contains information of the game world
        private World theWorld;

        //Timer to make sure beam drawing does not stay indefinetely 
        private Timer beamTimer;

        //
        Stack<string> movementOrder = new Stack<string>();

        /// <summary>
        /// Returns the world object for the view
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return theWorld;
        }

        /// <summary>
        /// This is a connect method for the view once the player presses the 
        /// connect button. 
        /// <parameter> serverName gives the names of the server to connect to</parameter>
        /// </summary>
        public void Connect(string serverName, string name)
        {
            playerName = name;
            Networking.ConnectToServer(OnConnect, serverName, 11000);
        }

        /// <summary>
        /// Once server connects, run this part of the handshake which sets server
        /// as a member variable representing the connection to the host
        /// </summary>
        /// <param name="ss"></param> Socket state to represent the server connection
        private void OnConnect(SocketState ss)
        {
            //Check if the socket state shows that an error occurred
            if (ss.ErrorOccured)
            {
                Error(ss.ErrorMessage);
                return;
            }

            //Set the callback to our startup world drawing method
            ss.OnNetworkAction = ReceiveStartup;

            //Send player name to socket
            Networking.Send(ss.TheSocket, playerName);

            //Server is now assigned to the SocketState
            server = ss;

            //Start receiving beginning setup
            Networking.GetData(ss);

        }

        /// <summary>
        /// This method receives the beginning data for the world 
        /// and sets up the main framework
        /// </summary>
        /// <param name="ss"></param>
        private void ReceiveStartup(SocketState ss)
        {
            //Check for error
            if (ss.ErrorOccured)
            {
                Error(ss.ErrorMessage);
            }

            //Change network action to receive normal flow of data method
            ss.OnNetworkAction = ReceiveMessage;

            //Extract the ID data and world data for setup
            string startUp = ss.GetData();
            string[] elements = startUp.Split('\n');
            playerID = int.Parse(elements[0]);
            int worldSize = int.Parse(elements[1]);

            //Setup the world
            theWorld = new World(worldSize);

            //Clear startup data. We have what is needed
            ss.ClearData();

            //Call the receive message method indirectly
            Networking.GetData(ss);

        }

        /// <summary>
        /// This method begins the callback for receiving messages
        /// </summary>
        /// <param name="socket"></param>
        private void ReceiveMessage(SocketState socket)
        {
            //Check for error
            if (socket.ErrorOccured)
            {
                Error(socket.ErrorMessage);
                return;
            }

            //Update the world on the new Json info
            ProcessMessage(socket);

            //Start Event loop
            Networking.GetData(socket);
        }

        /// <summary>
        /// This method receives a socket state from receive message, getting the socket's message
        /// and updating the world's models using Json messages.
        /// </summary>
        /// <param name="socket">Socket containing message data</param>
        private void ProcessMessage(SocketState socket)
        {
            // Get string data from socket state 
            string JsonMessage = socket.GetData();

            //This is a string that will contain all the complete data i.e. no partial Json strings at the end
            string completeMessage;
            //If the Json message does not end with newline, it means partial Json message at end so 
            //we take the part that we can process
            if (!JsonMessage.EndsWith("\n"))
            {
                //find the last instance of the newline and split the string at that point
                int completedPoint = JsonMessage.LastIndexOf('\n');
                completeMessage = JsonMessage.Substring(0, completedPoint);
            }
            //Message is complete and we can move forward as normal
            else
                completeMessage = JsonMessage;

            // Separate objects within Json message using new line 
            string[] parsedMessage = completeMessage.Split('\n');
            JObject curObj;
            JToken curToken;

            // Loop through each Json segment and identify its type to update model.
            // Once object type has been found pass in the Json message along with an int 
            // value that references it's type within the UpdateWorldModel method.
            foreach (string curMessage in parsedMessage)
            {
                //Skip any strings that are empty so to not throw error
                if (curMessage == "")
                    continue;

                //Parse the Json object and compare to other objects
                curObj = JObject.Parse(curMessage);

                // Check if object is tank
                curToken = curObj["tank"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 0);
                    tankInfoReceived = true;
                    continue;
                }

                //Update world model with walls
                curToken = curObj["wall"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 1);
                    continue;
                }

                // Check if object is projectile
                curToken = curObj["proj"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 2);
                    continue;
                }

                // Check if object is PowerUp
                curToken = curObj["power"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 3);
                    continue;
                }

                // Check if object is Beam
                curToken = curObj["beam"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 4);
                    continue;
                }
            }

            //Send the player ID to view for drawing purposes
            //Only sends once we receive tank data about oursleves
            if (tankInfoReceived)
            {
                PlayerIDGiven(playerID);
                SendTankUpdate(selfTank);
            }

            //Clear old data
            socket.RemoveData(0, completeMessage.Length);

            //Notify the View to redraw the world
            UpdateWorld();

        }

        /// <summary>
        /// Method for registering an attack by a tank
        /// </summary>
        /// <param name="whichSide"></param>
        public void WeaponFire(string whichSide)
        {
            //Determines which weapon fired
            if (whichSide == "left")
                leftClickPressed = true;
            if (whichSide == "right")
                rightClickPressed = true;

            //Update server
            SendTankUpdate(selfTank);
        }

        /// <summary>
        /// This method receives the mouse position and 
        /// translates it to the tank's turret movement.
        /// The turret will always point in the direction of the mouse
        /// </summary>
        /// <param name="mousePos"></param>
        public void TurretMouseAngle(MouseEventArgs mousePos)
        {
            //Calculate the vector between mouse position and tank
            //425 is half of the view screen size.  Since tank is centered on the panel, that
            //describes  tank location as a fixed point
            double mouseWorldXPosition = mousePos.X - 425;
            double mouseWorldYPosition = mousePos.Y - 425;

            //Get the vector points between mouse and tank location
            Vector2D newAim = new Vector2D(mouseWorldXPosition, mouseWorldYPosition);

            //Normalize vector and set tank aim direction to this new vector
            newAim.Normalize();
            selfTank.AimDirection = newAim;

            //Update the tank
            SendTankUpdate(selfTank);
        }

        // To Fix Idea dump: Since there can be multiple keys being pressed at a time, we need to incorporate
        // a data structure that is able to track the order at which each key has been pressed. Once these key(s) 
        // are in the data structure we need to continue to execute the key that was first added until its key has 
        // been released. Once this key has been released it will be removed from the data structure and the next most 
        // recent key will start to be executed, assuming it has not been released yet. This will also fix our jitter issue
        // and slow movement on the tank as we will only be sending one movement a frame whereas now we have the potential to send
        // multiple a frame which is creating the slow moving laggy tank that we have right now.

        /// <summary>
        /// This method is called when a keyevent is registered
        /// Sets the boolean to true allowing the movement method
        /// to be called
        /// </summary>
        /// <param name="keyPressed"></param>
        public void Movement(string keyPressed)
        {
            //Sets the boolean when appropriate key is pressed
            lock (movementOrder)
            {
                switch (keyPressed)
                {

                    //Set left to true and others to false
                    case "left":
                        leftKeyPressed = true;
                        movementOrder.Push("left");
                        break;

                    //Set Right to true and others to false
                    case "right":
                        rightKeyPressed = true;
                        movementOrder.Push("right");
                        break;

                    //Set Up to true and others to false
                    case "up":
                        upKeyPressed = true;
                        movementOrder.Push("up");
                        break;

                    //Set Down to true and other to false
                    case "down":
                        downKeyPressed = true;
                        movementOrder.Push("down");
                        break;
                }
            }
            //Send tank update
            SendTankUpdate(selfTank);
        }

        /// <summary>
        /// Method when the tank registers key up, meaning no movement
        /// </summary>
        public void MovementStopped(string stopDir)
        {
            switch (stopDir)
            {
                //Set flags to false
                case "left":
                    leftKeyPressed = false;
                    break;
                case "right":
                    rightKeyPressed = false;
                    break;
                case "up":
                    upKeyPressed = false;
                    break;
                case "down":
                    downKeyPressed = false;
                    break;
            }
            //Send tank update
            SendTankUpdate(selfTank);
        }
        /// <summary>
        /// Method that sends the tanks updated stats to the server
        /// </summary>
        private void SendTankUpdate(Tank t)
        {
            //Set up parameters for Control command object
            string direction = "none";
            string fire;


            //string curMove = movementOrder.Peek();

            //switch (curMove)
            //{
            //    case "left":
            //        if (leftKeyPressed)
            //        {
            //            direction = curMove;
            //            break;
            //        }
            //        else
            //            movementOrder.Pop();
            //        break;
            //    case "right":
            //        if (rightKeyPressed)
            //        {
            //            direction = curMove;
            //            break;
            //        }
            //        else
            //            movementOrder.Pop();
            //        break;
            //    case "down":
            //        if (downKeyPressed)
            //        {
            //            direction = curMove;
            //            break;
            //        }
            //        else
            //            movementOrder.Pop();
            //        break;
            //    case "up":
            //        if (upKeyPressed)
            //        {
            //            direction = curMove;
            //            break;
            //        }
            //        else
            //            movementOrder.Pop();
            //        break;
            //}
            //if (movementOrder.Count == 0)
            //    direction = "none";


            //Check movement state
            if (leftKeyPressed || (leftKeyPressed && downKeyPressed))
                direction = "left";
            else if (downKeyPressed)
                direction = "down";
            else if (upKeyPressed)
                direction = "up";
            else if (rightKeyPressed)
                direction = "right";
            else
                direction = "none";

            //Check firing state
            if (leftClickPressed)
                fire = "main";
            else if (rightClickPressed)
                fire = "alt";
            else
                fire = "none";

            // Normalize the aim direction vector
            t.AimDirection.Normalize();

            //Create the control command object with direction, weapon, and turret direction
            ControlCommand cc = new ControlCommand(direction, fire, t.AimDirection);

            //Send to server
            Networking.Send(server.TheSocket, JsonConvert.SerializeObject(cc) + '\n');

            //Reset weapon states
            leftClickPressed = false;
            rightClickPressed = false;
        }

        //Callback from timer to remove beam from world
        public void RemoveBeam(object x)
        {
            Beam b = x as Beam;
            lock (theWorld.Beams)
            {
                theWorld.Beams.Remove(b.GetID());
            }
        }

        /// <summary>
        /// The method takes the current JsonMessage from ProcessMessage and
        /// updates the models within the world.
        /// </summary>
        /// <param name="JsonMessage">Message passed from ProccessMessage</param>
        /// <param name="objectType">Int number representing the object's type</param>
        private void UpdateWorldModel(string JsonMessage, int objectType)
        {
            // Enter switch case based on what object has identified as and 
            // update the models in the world
            switch (objectType)
            {
                case 0:
                    // Convert Json message into Tank object 
                    Tank curTank = JsonConvert.DeserializeObject<Tank>(JsonMessage);
                    //The tank object is us
                    if (curTank.GetID() == playerID)
                        selfTank = curTank;
                    lock (theWorld.Tanks)
                    {
                        // Check if the world contains the object already
                        if (theWorld.Tanks.ContainsKey(curTank.GetID()))
                        {
                            // Remove the tank so that it can be updated
                            theWorld.Tanks.Remove(curTank.GetID());
                        }
                        //Check if tank has died
                        if (curTank.HealthLevel > 0)
                        {
                            curTank.HasDied = false;
                            // Re-add the tank back into the world
                            theWorld.Tanks.Add(curTank.GetID(), curTank);
                        }
                        else
                        {
                            curTank.HasDied = true;
                        }
                    }
                    break;
                case 1:
                    // Convert Json message into Wall object
                    Wall curWall = JsonConvert.DeserializeObject<Wall>(JsonMessage);
                    // No check needed to see if it already exists since walls will only be added once
                    // Add the wall into the world 
                    lock (theWorld.Walls)
                    {
                        theWorld.Walls.Add(curWall.GetID(), curWall);
                    }
                    break;
                case 2:
                    // Convert Json message into Projectile object
                    Projectile curProjectile = JsonConvert.DeserializeObject<Projectile>(JsonMessage);
                    // Check if the world contains the object already
                    lock (theWorld.Projectiles)
                    {
                        if (theWorld.Projectiles.ContainsKey(curProjectile.getID()))
                        {
                            // Remove the projectile so that it can be updated
                            theWorld.Projectiles.Remove(curProjectile.getID());
                        }
                        //Check if projectile is still active
                        if (!curProjectile.Died)
                        {
                            // Re-add the projectile back into the world
                            theWorld.Projectiles.Add(curProjectile.getID(), curProjectile);
                        }
                    }
                    break;
                case 3:
                    // Convert Json message into PowerUp object
                    PowerUp curpowerUp = JsonConvert.DeserializeObject<PowerUp>(JsonMessage);
                    // Check if the world contains the object already
                    lock (theWorld.PowerUps)
                    {
                        if (theWorld.PowerUps.ContainsKey(curpowerUp.getID()))
                        {
                            // Remove the PowerUp so that it can be updated
                            theWorld.PowerUps.Remove(curpowerUp.getID());
                        }
                        //Check if the powerup was collected or not
                        if (!curpowerUp.collected)
                        {
                            // Re-add the PowerUp back into the world
                            theWorld.PowerUps.Add(curpowerUp.getID(), curpowerUp);
                        }
                    }
                    break;
                case 4:
                    // Convert Json message into Beam object
                    Beam curBeam = JsonConvert.DeserializeObject<Beam>(JsonMessage);
                    // Check if the world contains the object already
                    lock (theWorld.Beams)
                    {
                        // add the Beam into the world
                        theWorld.Beams.Add(curBeam.GetID(), curBeam);
                        //Trigger a timer with the beam so that it is removed from the world after a certain time
                        beamTimer = new Timer(new TimerCallback(RemoveBeam), curBeam, 500, -1);
                    }
                    break;
            }
        }
    }
}