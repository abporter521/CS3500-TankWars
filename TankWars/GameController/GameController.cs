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
            if(socket.ErrorOccured)
            {
                Error("Lost connection to server");
                return;
            }
            ProcessMessage(socket);
            Networking.GetData(socket);
        }

        private void ProcessMessage(SocketState socket)
        {
            string JsonMessage = socket.GetData();
            string[] parsedMessage = JsonMessage.Split('\n');
            JObject curObj;
            JToken curToken;
            foreach (string curMessage in parsedMessage)
            {
                curObj = JObject.Parse(curMessage);
               
                curToken = curObj["tank"];
                if(curToken != null)
                {
                    Tank curTank = JsonConvert.DeserializeObject<Tank>(curMessage);
                    if(theWorld.Tanks.ContainsKey(curTank.GetID())) {
                        theWorld.Tanks.Remove(curTank.GetID());
                    }
                   theWorld.Tanks.Add(curTank.GetID(), curTank);
                    continue;
                }
                curToken = curObj["wall"];
                if(curToken != null)
                {
                    Wall curWall = JsonConvert.DeserializeObject<Wall>(curMessage);
                    theWorld.Walls.Add(curWall.GetID(), curWall);
                    continue;
                }
                curToken = curObj["proj"];
                if(curToken != null)
                {
                    Projectile curProjectile = JsonConvert.DeserializeObject<Projectile>(curMessage);
                    if(theWorld.Projectiles.ContainsKey(curProjectile.getID()))
                    {
                        theWorld.Projectiles.Remove(curProjectile.getID());
                    }
                    theWorld.Projectiles.Add(curProjectile.getID(), curProjectile);
                    continue;
                }
            }
        }
           // JsonMessage = JsonConvert.DeserializeObject()

    }
    }
