﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.WebSockets;
using System.Threading;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TankWars
{
    //TODO:  KEY HANDLERS
    //       SET UP EVENTS FOR COMMUNICATION WITH VIEW
    //       ADD LOCKS TO WORLD UPDATE METHOD

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
        /// 
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
            if(tankInfoReceived)
                PlayerIDGiven(playerID);

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

        public void TurretMouseAngle(Point mousePos)
        {
            double x = mousePos.X - selfTank.Location.GetX();
            double y = mousePos.Y - selfTank.Location.GetY();
            Vector2D newAim = new Vector2D(x, y);
            selfTank.AimDirection = newAim;
            UpdateWorld();

        }

        /// <summary>
        /// This method is called when a keyevent is registered
        /// Sets the boolean to true allowing the movement method
        /// to be called
        /// </summary>
        /// <param name="keyPressed"></param>
        public void Movement(string keyPressed)
        {
            //Sets the boolean when appropriate key is pressed
            switch (keyPressed)
            {
                //Set left to true and others to false
                case "left":
                    leftKeyPressed = true;
                    rightKeyPressed = false;
                    upKeyPressed = false;
                    downKeyPressed = false;
                    break;
                //Set Right to true and others to false
                case "right":
                    rightKeyPressed = true;
                    leftKeyPressed = false;
                    upKeyPressed = false;
                    downKeyPressed = false;
                    break;

                //Set Up to true and others to false
                case "up":
                    upKeyPressed = true;
                    downKeyPressed = false;
                    leftKeyPressed = false;
                    rightKeyPressed = false;
                    break;
                //Set Down to true and other to false
                case "down":
                    downKeyPressed = true;
                    upKeyPressed = false;
                    leftKeyPressed = false;
                    rightKeyPressed = false;
                    break;

            }

            //Send tank update
            SendTankUpdate(selfTank);
        }

        /// <summary>
        /// Method when the tank registers no key pressed
        /// </summary>
        public void MovementStopped()
        {
            downKeyPressed = false;
            upKeyPressed = false;
            leftKeyPressed = false;
            rightKeyPressed = false;

            //Send tank update
            SendTankUpdate(selfTank);
        }

        /// <summary>
        /// Method that sends the tanks updated stats to the server
        /// </summary>
        private void SendTankUpdate(Tank t)
        {
            //Set up parameters for Control command object
            string direction;
            string fire;

            //Check movement state
            if (upKeyPressed)
                direction = "up";
            else if (downKeyPressed)
                direction = "down";
            else if (leftKeyPressed)
                direction = "left";
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
            //Create the control command object
            ControlCommand cc = new ControlCommand(direction, fire, t.AimDirection);

            //Send to server
            Networking.Send(server.TheSocket, JsonConvert.SerializeObject(cc) + '\n');

            //Reset weapon states
            leftClickPressed = false;
            rightClickPressed = false;

            //Redraw the world
            UpdateWorld();
        }

        /// <summary>
        /// The method takes the current JsonMessage from ProcessMessage and
        /// updates the models within the world.
        /// </summary>
        /// <param name="JsonMessage">Message passed from ProccessMessage</param>
        /// <param name="objectType">Int number representing the objects type</param>
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
                        // Re-add the tank back into the world
                        theWorld.Tanks.Add(curTank.GetID(), curTank);
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
                    // Convert Json message into Wall object
                    Projectile curProjectile = JsonConvert.DeserializeObject<Projectile>(JsonMessage);
                    // Check if the world contains the object already
                    lock (theWorld.Projectiles)
                    {
                        if (theWorld.Projectiles.ContainsKey(curProjectile.getID()))
                        {
                            // Remove the projectile so that it can be updated
                            theWorld.Projectiles.Remove(curProjectile.getID());
                        }
                        // Re-add the projectile back into the world
                        theWorld.Projectiles.Add(curProjectile.getID(), curProjectile);
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
                        // Re-add the PowerUp back into the world
                        theWorld.PowerUps.Add(curpowerUp.getID(), curpowerUp);
                    }
                    break;
                case 4:
                    // Convert Json message into Beam object
                    Beam curBeam = JsonConvert.DeserializeObject<Beam>(JsonMessage);
                    // Check if the world contains the object already
                    lock (theWorld.Beams)
                    {
                        if (theWorld.Beams.ContainsKey(curBeam.GetID()))
                        {
                            // Remove the Beam so that it can be updated
                            theWorld.Beams.Remove(curBeam.GetID());
                        }
                        // Re-add the Beam back into the world
                        theWorld.Beams.Add(curBeam.GetID(), curBeam);
                    }
                    break;
            }
        }
    }
}
