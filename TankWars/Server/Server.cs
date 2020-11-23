using NetworkUtil;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;

namespace TankWars
{
    class Server
    {
        private Dictionary<long, SocketState> connections;
        private World serverWorld;
        private Server server;
        int MSPerFrame;
        int framesPerShot;
        int respawnRate;



        /// <summary>
        /// The main entry point into the program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Server Initiated");
            Server server = new Server();

            //Set up the world and such from XML settings file
            server.ReadFromFile();

            //Start the event loop of connecting clients
            server.StartServer();

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
        private void ReadFromFile()
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
                                    //Read thrice since settings will have the x and y coordinate two tags after the wall
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();

                                    //Get P1 x coordinate
                                    int x1 = int.Parse(reader.Value);

                                    //Reads end tag of x, start tag of y, and value of P1 y
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();

                                    //Gets the P1 y coordinate
                                    int y1 = int.Parse(reader.Value);

                                    //Build vector for P1
                                    Vector2D point1 = new Vector2D(x1, y1);

                                    //Moves to the P2 x coordinate
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();

                                    //Get P2 x coordinate
                                    int x2 = int.Parse(reader.Value);

                                    //Reads end tag of x, start tag of y, and value of P2 y
                                    reader.Read();
                                    reader.Read();
                                    reader.Read();

                                    //Gets the P2 y coordinate
                                    int y2 = int.Parse(reader.Value);

                                    //Build vector for P2
                                    Vector2D point2 = new Vector2D(x2, y2);

                                    //Add the walls to the world model
                                    lock (serverWorld.Walls)
                                    {
                                        serverWorld.Walls.Add(serverWorld.Walls.Count, new Wall(serverWorld.Walls.Count, point1, point2));
                                    }
                                    break;
                            }
                        }
                        //If the element is not a start element, it must be an end element
                        else
                            continue;
                    }
                }
            }
            //If the file was not found
            catch (Exception)
            {
                Console.WriteLine("Error found in the settings XML file.");
            }
        }

        private void AcceptNewClients(SocketState client)
        {
            connections.Add(client.ID, client);
        }
    }
}
