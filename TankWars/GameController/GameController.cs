using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TankWars
{
    public class GameController
    {

        public delegate void EventHandler(IEnumerable<string> JsonMessage);
        public event EventHandler updateWorld;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        // State representing the connection with the server
        private SocketState server = null;

        //Contains information of the game world
        private World theWorld;

        /// <summary>
        /// This is a connect method for the view once the player presses the 
        /// connect button. 
        /// <parameter> serverName gives the names of the server to connect to</parameter>
        /// </summary>
        public void Connect(string serverName)
        {
            Networking.ConnectToServer(OnConnect, serverName, 11000);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param> Socket state to represent the server connection
        public void OnConnect(SocketState ss)
        {
            //Check if the socket state shows that an error occurred
            if (ss.ErrorOccured)
            {
                Error("Connection interrupted");
                return;
            }

            //Inform the view that we connected
            Connected();

            //Server is now assigned to the SocketState
            server = ss;

            //Now change the action to receiving messages
            ss.OnNetworkAction = ReceiveMessage;
            //Start receiving messages with event loop
            Networking.GetData(ss);

        }

        private void ReceiveMessage(SocketState socket)
        {
            if (socket.ErrorOccured)
            {
                Error("Lost connection to server");
                return;
            }
            ProcessMessage(socket);
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
            // Separate objects within Json message using new line 
            string[] parsedMessage = JsonMessage.Split('\n');
            JObject curObj;
            JToken curToken;
            
            // Loop through each Json segment and identify its type to update model.
            // Once object type has been found pass in the Json message along with an int 
            // value that references it's type within the UpdateWorldModel method.
            foreach (string curMessage in parsedMessage)
            {
                curObj = JObject.Parse(curMessage);

                // Check if object is tank
                curToken = curObj["tank"];
                if (curToken != null)
                {
                    UpdateWorldModel(curMessage, 0);
                    continue;
                }

                // Check if object is wall
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
                    // Check if the world contains the object already
                    if (theWorld.Tanks.ContainsKey(curTank.GetID()))
                    {
                        // Remove the tank so that it can be updated
                        theWorld.Tanks.Remove(curTank.GetID());
                    }
                    // Re-add the tank back into the world
                    theWorld.Tanks.Add(curTank.GetID(), curTank);
                    break;
                case 1:
                    // Convert Json message into Wall object
                    Wall curWall = JsonConvert.DeserializeObject<Wall>(JsonMessage);
                    // No check needed to see if it already exists since walls will only be added once
                    // Add the wall into the world 
                    theWorld.Walls.Add(curWall.GetID(), curWall);
                    break;
                case 2:
                    // Convert Json message into Wall object
                    Projectile curProjectile = JsonConvert.DeserializeObject<Projectile>(JsonMessage);
                    // Check if the world contains the object already
                    if (theWorld.Projectiles.ContainsKey(curProjectile.getID()))
                    {
                        // Remove the projectile so that it can be updated
                        theWorld.Projectiles.Remove(curProjectile.getID());
                    }
                    // Re-add the projectile back into the world
                    theWorld.Projectiles.Add(curProjectile.getID(), curProjectile);
                    break;
                case 3:
                    // Convert Json message into PowerUp object
                    PowerUp curpowerUp = JsonConvert.DeserializeObject<PowerUp>(JsonMessage);
                    // Check if the world contains the object already
                    if (theWorld.PowerUps.ContainsKey(curpowerUp.getID()))
                    {
                        // Remove the PowerUp so that it can be updated
                        theWorld.PowerUps.Remove(curpowerUp.getID());
                    }
                    // Re-add the PowerUp back into the world
                    theWorld.PowerUps.Add(curpowerUp.getID(), curpowerUp);
                    break;
                case 4:
                    // Convert Json message into Beam object
                    Beam curBeam = JsonConvert.DeserializeObject<Beam>(JsonMessage);
                    // Check if the world contains the object already
                    if (theWorld.Beams.ContainsKey(curBeam.GetID()))
                    {
                        // Remove the Beam so that it can be updated
                        theWorld.Beams.Remove(curBeam.GetID());
                    }
                    // Re-add the Beam back into the world
                    theWorld.Beams.Add(curBeam.GetID(), curBeam);
                    break;
            }
        }
    }
}
