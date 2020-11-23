using NetworkUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace TankWars
{
    class Server
    {
        private Dictionary<long, SocketState> connections;
        private World serverWorld;
        private Server server;
        private int MSPerFrame;
        private int framesPerShot;
        private int respawnRate;

        private int playerNumber = 0;

        /// <summary>
        /// The main entry point into the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Server Initiated");
            Server server = new Server();

            //Start the event loop of connecting clients
            server.StartServer();

            //Set up the world and such from XML settings file
            server.ReadFromSettingsFile();

            //Sanity Check
            Console.WriteLine("Settings file read");

            Stopwatch delayWatch = new Stopwatch();
            while (true)
            {
                //Busy loop to delay updating the world
                while (delayWatch.ElapsedMilliseconds < server.MSPerFrame)
                {
                    //Do nothing
                }
                //Reset the watch back to 0
                delayWatch.Reset();

                server.UpdateWorld();
            }

        }

        /// <summary>
        /// Zero argument contructor for the server class
        /// </summary>
        public Server()
        {
            connections = new Dictionary<long, SocketState>();
        }

        ///Begins loop of accepting new clients
        private void StartServer()
        {
            Networking.StartServer(AcceptNewClients, 11000);
        }

        /// <summary>
        /// Reads from the settings XML file with tags. 
        /// Has settings for frame rate, respawn rate, world size, shot cooldown, and walls.
        /// </summary>
        private void ReadFromSettingsFile()
        {
            //Create the World from the settings file
            try
            {
                // Create an XmlReader inside this block, and automatically Dispose() it at the end.
                using (XmlReader reader = XmlReader.Create(@"..\..\..\..\RESOURCES\settings.xml"))
                {
                    //Read the element
                    while (reader.Read())
                    {
                        //If the element is a start element, move foward
                        if (reader.IsStartElement())
                        {
                            //Check the reader name
                            switch (reader.Name)
                            {
                                case "GameSettings":
                                    continue;

                                //case element is Universe Size
                                case "UniverseSize":
                                    //Gets name of cell
                                    reader.Read();
                                    //Get the world size
                                    string worldSize = reader.Value;

                                    //Create the world
                                    if (int.TryParse(worldSize, out int size))
                                    {
                                        serverWorld = new World(size);
                                    }
                                    break;

                                //Case where element is frame rate
                                case "MSPerFrame":
                                    //Assign the MS per frame
                                    reader.Read();

                                    string MSframes = reader.Value;
                                    if (int.TryParse(MSframes, out int speed))
                                    {
                                        MSPerFrame = speed;
                                    }
                                    break;

                                case "FramesPerShot":
                                    //Read the settings FramesPerShot.  This is how many frames must go by before tank can fire again
                                    reader.Read();
                                    string framesShot = reader.Value;
                                    if (int.TryParse(framesShot, out int shotCooldown))
                                    {
                                        framesPerShot = shotCooldown;
                                    }
                                    break;

                                case "RespawnRate":
                                    //Gets respawn time of tank
                                    reader.Read();
                                    string ressurrectionRate = reader.Value;
                                    if (int.TryParse(ressurrectionRate, out int imAlive))
                                    {
                                        respawnRate = imAlive;
                                    }
                                    break;

                                case "Wall":
                                    reader.Read();
                                    // Get wall points
                                    int x1 = 0, x2 = 0, y1 = 0, y2 = 0;

                                    //flags for being in point
                                    bool inP1 = false;
                                    bool inP2 = false;

                                    //We know the rest of the setting file contains walls until the end game settings tag is given
                                    //therefore we enter a loop that sets up the rest of the walls;
                                    while (reader.Name != "GameSettings")
                                    {
                                        //progress to the next tag
                                        reader.Read();
                                        //Read thrice since settings will have the x and y coordinate two tags after the wall
                                        if (reader.IsStartElement())
                                        {
                                            //Check the reader name
                                            switch (reader.Name)
                                            {
                                                //We are looking at coordinates in the first point
                                                case "p1":
                                                    inP1 = true;
                                                    break;

                                                //looking at coordinates for the second point
                                                case "p2":
                                                    inP2 = true;
                                                    break;

                                                //assign x based on which point we are looking at
                                                case "x":
                                                    reader.Read();
                                                    if (inP1)
                                                        x1 = int.Parse(reader.Value);
                                                    else if (inP2)
                                                        x2 = int.Parse(reader.Value);
                                                    break;

                                                //assign y based on which point we are looking at
                                                case "y":
                                                    reader.Read();
                                                    if (inP1)
                                                        y1 = int.Parse(reader.Value);

                                                    else if (inP2)
                                                        y2 = int.Parse(reader.Value);
                                                    break;

                                            }
                                        }
                                        //If the element is not a start element, it must be an end element
                                        else
                                        {
                                            //We have reached the end of one wall object, so build and add to world
                                            if (reader.Name == "Wall")
                                            {
                                                //Build vector for P1
                                                Vector2D point1 = new Vector2D(x1, y1);

                                                //Build vector for P2
                                                Vector2D point2 = new Vector2D(x2, y2);

                                                //Add the walls to the world model
                                                lock (serverWorld.Walls)
                                                {
                                                    serverWorld.Walls.Add(serverWorld.Walls.Count, new Wall(serverWorld.Walls.Count, point1, point2));
                                                }
                                            }
                                            //Triggers out of first point
                                            if (reader.Name == "p1")
                                                inP1 = false;
                                            //Triggers out of second point
                                            else if (reader.Name == "p2")
                                                inP2 = false;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            //If the file was not found
            catch (Exception)
            {
                Console.WriteLine("Error found in the settings XML file.");
            }
        }

        /// <summary>
        /// Adds the connection to our connection ID
        /// </summary>
        /// <param name="client"></param>
        private void AcceptNewClients(SocketState client)
        {
            //Set up random for new location
            Random randLoc = new Random();
            int x = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size);
            int y = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size);

            //Add socket state to the collection of players
            connections.Add(client.ID, client);

            //Get the player name from the socket state
            string playerName = client.GetData();

            //Create a new tank representing the player at a random location
            Tank newPlayer = new Tank(playerNumber, playerName, new Vector2D((double)x, (double)y));
            //Increase player ID number
            playerNumber++;

            client.OnNetworkAction = GetDataFromClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        private void GetDataFromClient(SocketState connection)
        {
            throw new NotImplementedException();
        }

        private void UpdateWorld()
        {

        }
    }
}
