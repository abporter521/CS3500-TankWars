using NetworkUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace TankWars
{
    //TODO: FIX COOLDOWN OF PROJECTILE FIRE

    /// <summary>
    /// This is the server class that will handle all logic of the game TankWars
    /// This class will keep track of the locations of the players, collision logic
    /// and when to push world updates.  
    /// 
    /// @Author Adam Scott & Andrew Porter
    /// </summary>
    class Server
    {
        //Dictionary to hold all the client socket state
        private Dictionary<SocketState, int> connections;

        //Variables holding the settings of the game
        private World serverWorld;
        private int MSPerFrame;
        private int framesPerShot;
        private int respawnRate;
        private int frameCounter = 0;
        private int maxPowerUpNumber = 2;
        //Keep track of stats
        private int engineForce = 3;
        private int projectileForce = 25;
        private int powerUpCount = 0;
        //Keep track of player ID
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
                delayWatch.Start();
                //Busy loop to delay updating the world
                while (delayWatch.ElapsedMilliseconds < server.MSPerFrame)
                {
                    //Do nothing
                }
                //Reset the watch back to 0
                delayWatch.Reset();

                server.GeneratePowerUp();
                server.UpdateWorld();
            }

        }

        /// <summary>
        /// Zero argument contructor for the server class
        /// </summary>
        public Server()
        {
            connections = new Dictionary<SocketState, int>();
        }

        ///Begins loop of accepting new clients
        private void StartServer()
        {
            Networking.StartServer(AcceptNewClients, 11000);
        }

        /// <summary>
        /// Adds the connection to our connection ID
        /// </summary>
        /// <param name="client"></param>
        private void AcceptNewClients(SocketState client)
        {
            if (client.ErrorOccured)
            {
                Console.WriteLine(client.ErrorMessage);
            }

            //Set OnNetworkAction to receivePlayerInfo
            client.OnNetworkAction = GetPlayerInfo;

            //Get the player name from the socket state
            Networking.GetData(client);
        }

        /// <summary>
        /// We will receive the player's name and assign an ID number
        /// At this time we will also send to the client world size, the ID number
        /// and the walls
        /// </summary>
        /// <param name="client"></param>
        private void GetPlayerInfo(SocketState client)
        {
            if (client.ErrorOccured)
            {
                Console.WriteLine(client.ErrorMessage);
            }

            //Set the new callback action
            client.OnNetworkAction = GetActionDataFromClient;

            //Generate Location
            Random randLoc = new Random();
            int x = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
            int y = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;

            //Get the player name
            string playerName = client.GetData().Trim('\n');

            //Create a new tank representing the player at a random location
            Tank newPlayer = new Tank(playerNumber, playerName, new Vector2D((double)x, (double)y));

            //We don't spawn on walls or powerups
            while (CheckForCollision(newPlayer, 1))
            {
                x = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
                y = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
                newPlayer.Location = new Vector2D(x, y);
            }

            //Add player to our connections
            lock (connections)
            {
                //Add socket state to the collection of players with their ID number
                connections.Add(client, playerNumber);
            }
            Console.WriteLine("Player " + playerNumber.ToString() + ": " + playerName + " has connected.");

            //Add player to server world
            lock (serverWorld.Tanks)
            {
                serverWorld.Tanks.Add(newPlayer.GetID(), newPlayer);
            }

            //Send ID and worldsize info
            Networking.Send(client.TheSocket, playerNumber.ToString() + "\n" + serverWorld.Size.ToString() + "\n");

            //Create a string builder info to serialize and send all the walls
            StringBuilder wallinfo = new StringBuilder();
            foreach (Wall wall in serverWorld.Walls.Values)
            {
                wallinfo.Append(JsonConvert.SerializeObject(wall) + "\n");
            }

            //Send walls to the client
            Networking.Send(client.TheSocket, wallinfo.ToString());

            //Increase player ID number
            playerNumber++;

            //Empty the socket state of data
            client.ClearData();

            //Begin receive loop
            Networking.GetData(client);

        }

        /// <summary>
        /// This method will implement changes created from Control Commands
        /// </summary>
        /// <param name="connection"></param>
        private void GetActionDataFromClient(SocketState connectionToClient)
        {
            //Gets message from the client
            if (connectionToClient.ErrorOccured)
            {
                Console.WriteLine("Player " + connections[connectionToClient].ToString() + " has disconnected");

                lock (connections)
                {
                    int tankID = connections[connectionToClient];
                    serverWorld.Tanks[tankID].HasDisconnected = true;
                    serverWorld.Tanks[tankID].HasDied = true;
                    return;
                }
            }
            string wholeData = connectionToClient.GetData();
            string completeMessage;

            //Make sure data is complete
            if (!wholeData.EndsWith("\n"))
            {
                //find the last instance of the newline and split the string at that point
                int completedPoint = wholeData.LastIndexOf('\n');
                completeMessage = wholeData.Substring(0, completedPoint);
            }
            //Message is complete and we can move forward as normal
            else
                completeMessage = wholeData;

            //Split string by newline
            string[] movementUpdates = completeMessage.Split('\n');
            foreach (string command in movementUpdates)
            {
                //Skip over empty strings
                if (command == "")
                    continue;

                //Process command
                ControlCommand newCommand = JsonConvert.DeserializeObject<ControlCommand>(command);
                UpdateTankState(newCommand, connectionToClient);
            }
            //Remove old data
            connectionToClient.RemoveData(0, completeMessage.Length);

            //Begin loop
            Networking.GetData(connectionToClient);
        }

        /// <summary>
        /// This method is designed to take input data from the player.
        /// This method will take care of tank movement, turret rotation
        /// and projectile creation.
        /// 
        /// </summary>
        /// <param name="cc"></param> Control command received by client
        /// <param name="player"></param> The SocketState connection of the player
        private void UpdateTankState(ControlCommand cc, SocketState player)
        {
            //Get the ID associated with the connection
            int ID = connections[player];

            //Get the tank and update location and projectile status
            Tank curTank = serverWorld.Tanks[ID];

            lock (serverWorld.Tanks)
            {
                //Update tank state
                TankMovement(cc.GetDirection(), curTank);
                //Change tank turret
                curTank.AimDirection = cc.GetTurretDirection();
                //Update world on for tank
                serverWorld.Tanks[ID] = curTank;
            }

            //Check fire state
            lock (serverWorld.Projectiles)
            {
                ProjectileCreation(cc, curTank);
            }
        }

        /// <summary>
        /// This method updates the tank location
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="tank"></param>
        private void TankMovement(String dir, Tank tank)
        {

            //Each case adjusts tank orientation, and movement
            Vector2D velocity = new Vector2D(0, 0);
            Vector2D oldLoc = tank.Location;
            switch (dir)
            {
                case "left":
                    velocity = new Vector2D(-engineForce, 0.0);
                    tank.Location += velocity;
                    tank.Orientation = new Vector2D(-1, 0);
                    break;
                case "right":
                    velocity = new Vector2D(engineForce, 0.0);
                    tank.Location += velocity;
                    tank.Orientation = new Vector2D(1, 0);
                    break;
                case "up":
                    velocity = new Vector2D(0, -engineForce);
                    tank.Location += velocity;
                    tank.Orientation = new Vector2D(0, -1);
                    break;
                case "down":
                    velocity = new Vector2D(0, engineForce);
                    tank.Location += velocity;
                    tank.Orientation = new Vector2D(0, 1);
                    break;
                case "none":
                    break;
            }

            //Check for wall collisions and world out of bounds
            if (CheckForCollision(tank, 1))
                tank.Location = oldLoc;
        }

        /// <summary>
        /// This method will create a projectile object or beam object
        /// </summary>
        /// <param name="cc"></param>
        /// <param name="tank"></param>
        private void ProjectileCreation(ControlCommand cc, Tank tank)
        {
            //figure out what kind of weapon
            String fireStatus = cc.GetFire();

            //Get aim direction
            Vector2D turretDirection = cc.GetTurretDirection();
            if (fireStatus == "main")// Normal attack
            {
                // Make changes to ID of projectile so two projectiles do not have the same ID if spawned at same time
                Projectile proj = new Projectile(tank.GetID(), tank.Location, turretDirection, tank.GetID());
                lock (serverWorld.Projectiles)
                {
                    serverWorld.Projectiles[proj.getID()] = proj;
                }
            }

            //This is a beam attack
            else if (fireStatus == "alt")
            {
                //Check if a tank has picked up a powerup
                if (tank.PowerUpNumber > 0)
                {
                    // Make changes to ID of beam so two beams do not have the same ID if spawned at same time
                    Beam beam = new Beam(tank.Location, turretDirection, tank.GetID(), tank.GetID());
                    //Decrement tanks powerup number
                    tank.UsePowerup();
                    lock (serverWorld.Beams)
                    {
                        serverWorld.Beams.Add(beam.GetID(), beam);
                    }
                }
            }

        }
        /// <summary>
        /// Generates powerups every 1650 frames as default.  Will place in 
        /// server's collection of powerups.
        /// </summary>
        private void GeneratePowerUp()
        {
            //Variable to count the 
            frameCounter++;
            Random randLoc = new Random();
            
            //If enough frames have passed and the number of powerups does not exceed the max, create a powerup
            if (frameCounter > 1650 && serverWorld.PowerUps.Count <= maxPowerUpNumber)
            {
                //Get location
                int x = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
                int y = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;


                //Create a new powerUp at a random location
                Vector2D powerUpPos = new Vector2D(x, y);
                PowerUp power = new PowerUp(powerUpPos, powerUpCount);
                powerUpCount++;

                //We don't spawn on walls or tanks
                while (CheckForCollision(power, 2))
                {
                    x = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
                    y = randLoc.Next(-1 * serverWorld.Size, serverWorld.Size) / 2;
                    power.position = new Vector2D(x, y);
                }
                //Add the the server's collection of powerups
                lock (serverWorld.PowerUps)
                {
                    serverWorld.PowerUps.Add(powerUpCount, power);
                }

                //reset the counter
                frameCounter = 0;
            }
        }
        /// <summary>
        /// This Method is called once per frame according to the settings.
        /// This method will update any projectiles, powerups and beams,
        /// then send all the information to each client in a Json message
        /// </summary>
        private void UpdateWorld()
        {
            //Check if tank has  disconnected

            //Check proj collisions w/ tank or wall
            UpdateProjectileState();


            //Check on powerups


            //Build JSON and send to each client
            StringBuilder newWorld = new StringBuilder();

            lock (serverWorld.PowerUps)
            {
                foreach (PowerUp power in serverWorld.PowerUps.Values)
                {
                    newWorld.Append(JsonConvert.SerializeObject(power) + "\n");
                    if (power.collected)
                    {
                        serverWorld.PowerUps.Remove(power.getID());
                    }
                }
            }
            lock (serverWorld.Tanks)
            {
                foreach (Tank t in serverWorld.Tanks.Values)
                {
                    newWorld.Append(JsonConvert.SerializeObject(t) + "\n");
                    if (t.HasDisconnected)
                        serverWorld.Tanks.Remove(t.GetID());
                }
            }
            lock (serverWorld.Projectiles)
            {
                foreach (Projectile p in serverWorld.Projectiles.Values)
                {
                    newWorld.Append(JsonConvert.SerializeObject(p) + "\n");
                    //Remove from server world if dead
                    if (p.Died)
                        serverWorld.Projectiles.Remove(p.getID());
                }

            }
            lock (serverWorld.Beams)
            {
                foreach (Beam b in serverWorld.Beams.Values)
                {
                    newWorld.Append(JsonConvert.SerializeObject(b) + "\n");
                    // Remove beam as soon as it has been shot for one frame
                    serverWorld.Beams.Remove(b.GetID());
                }
            }
            lock (serverWorld.PowerUps)
            {
                foreach (PowerUp p in serverWorld.PowerUps.Values)
                {
                    newWorld.Append(JsonConvert.SerializeObject(p) + "\n");
                    // remove from server world if collected
                    if (p.collected)
                    {
                        serverWorld.PowerUps.Remove(p.getID());
                    }
                }
            }
            lock (connections)
            {
                foreach (SocketState clients in connections.Keys)
                {
                    if (clients.TheSocket.Connected)
                        Networking.Send(clients.TheSocket, newWorld.ToString());
                }
            }
        }

        /// <summary>
        /// Method that checks and updates projectile location then checks if any collisions happen.
        /// </summary>
        private void UpdateProjectileState()
        {
            lock (serverWorld.Projectiles)
            {
                foreach (Projectile proj in serverWorld.Projectiles.Values)
                {
                    //Update the proj location
                    proj.Direction.Normalize();
                    proj.Location += proj.Direction * projectileForce;

                    //Check for collision and if true, set Died property to true
                    if (CheckForCollision(proj, 0))
                    {
                        serverWorld.Projectiles[proj.getID()].Died = true;
                    }

                }
            }
        }

        /// <summary>
        /// General purpose helper method that checks if an object collides with
        /// anything else in the world.
        /// </summary>
        /// <param name="o"></param> The object we want to check against everything else
        /// <param name="objectType"></param> A integer code that tells the method what kind of object o is
        /// <returns></returns>
        private bool CheckForCollision(Object o, int objectType)
        {
            //Collision flags
            bool collisionStatusX = false;
            bool collisionStatusY = false;

            //If o is a projectile
            if (objectType == 0)
            {
                Projectile proj = o as Projectile;
                //Check collsion against walls
                foreach (Wall curWall in serverWorld.Walls.Values)
                {
                    //Get range of values for x
                    double high = Math.Max(curWall.GetP2().GetX(), curWall.GetP1().GetX()) + 25;
                    double low = Math.Min(curWall.GetP2().GetX(), curWall.GetP1().GetX()) - 25;

                    //Check if projectile is in between range of x values
                    if (proj.Location.GetX() < high && proj.Location.GetX() > low)
                        collisionStatusX = true;

                    //Get range of values for y
                    high = Math.Max(curWall.GetP2().GetY(), curWall.GetP1().GetY()) + 25;
                    low = Math.Min(curWall.GetP2().GetY(), curWall.GetP1().GetY()) - 25;

                    //Check if projectile is in between range of y values
                    if (proj.Location.GetY() < high && proj.Location.GetY() > low)
                        collisionStatusY = true;

                    //If both are within range, projectile collided with wall so return true
                    if (collisionStatusX && collisionStatusY)
                        return true;
                    else //Projectile did not collide with this wall, reset flags and move on to next segment
                    {
                        collisionStatusY = false;
                        collisionStatusX = false;
                        continue;
                    }
                }

                //Check collision against tanks
                lock (serverWorld.Tanks)
                {
                    foreach (Tank t in serverWorld.Tanks.Values)
                    {
                        //Create radius around tank 
                        Vector2D radius = proj.Location - t.Location;

                        //If distance between projectile and tank is less than 30, its a hit
                        if (radius.Length() < 30 && proj.getID() != t.GetID())
                        {
                            //decrement tank health
                            t.HealthLevel -= 1;

                            //if health is 0, then it has died
                            if (t.HealthLevel == 0)
                                t.HasDied = true;
                            return true;
                        }
                    }
                }
            }
            //object is a tank
            else if (objectType == 1)
            {
                Tank t = o as Tank;

                //Check for powerup collision and wall collision
                foreach (Wall curWall in serverWorld.Walls.Values)
                {
                    //Get range of values for x
                    double high = Math.Max(curWall.GetP2().GetX(), curWall.GetP1().GetX()) + 55;
                    double low = Math.Min(curWall.GetP2().GetX(), curWall.GetP1().GetX()) - 55;

                    //Check if projectile is in between range of x values
                    if (t.Location.GetX() < high && t.Location.GetX() > low)
                        collisionStatusX = true;

                    //Get range of values for y
                    high = Math.Max(curWall.GetP2().GetY(), curWall.GetP1().GetY()) + 55;
                    low = Math.Min(curWall.GetP2().GetY(), curWall.GetP1().GetY()) - 55;

                    //Check if projectile is in between range of y values
                    if (t.Location.GetY() < high && t.Location.GetY() > low)
                        collisionStatusY = true;

                    //If both are within range, projectile collided with wall so return true
                    if (collisionStatusX && collisionStatusY)
                        return true;
                    else //Projectile did not collide with this wall, reset flags and move on to next segment
                    {
                        collisionStatusY = false;
                        collisionStatusX = false;
                        continue;
                    }
                }

                lock (serverWorld.PowerUps)
                {
                    foreach (PowerUp curPower in serverWorld.PowerUps.Values)
                    {
                        //Create radius around tank 
                        Vector2D radius = curPower.position - t.Location;

                        //If distance between projectile and tank is less than 30, its a hit
                        if (radius.Length() < 30)
                        {
                            // Increment powerUp number
                            t.CollectPowerup();

                            //if health is 0, then it has died
                            curPower.collected = true;
                            return false;
                        }
                    }
                }
            }
            else if (objectType == 2)
            {
                PowerUp power = o as PowerUp;
                foreach (Wall curWall in serverWorld.Walls.Values)
                {
                    //Get range of values for x
                    double high = Math.Max(curWall.GetP2().GetX(), curWall.GetP1().GetX()) + 25;
                    double low = Math.Min(curWall.GetP2().GetX(), curWall.GetP1().GetX()) - 25;

                    //Check if powerUp is in between range of x values
                    if (power.position.GetX() < high && power.position.GetX() > low)
                        collisionStatusX = true;

                    //Get range of values for y
                    high = Math.Max(curWall.GetP2().GetY(), curWall.GetP1().GetY()) + 25;
                    low = Math.Min(curWall.GetP2().GetY(), curWall.GetP1().GetY()) - 25;

                    //Check if powerUp is in between range of y values
                    if (power.position.GetY() < high && power.position.GetY() > low)
                        collisionStatusY = true;

                    //If both are within range, powerUp collided with wall so return true
                    if (collisionStatusX && collisionStatusY)
                        return true;
                    else //PowerUp did not collide with this wall, reset flags and move on to next segment
                    {
                        collisionStatusY = false;
                        collisionStatusX = false;
                        continue;
                    }
                }
                lock (serverWorld.Tanks)
                {
                    foreach (Tank t in serverWorld.Tanks.Values)
                    {
                        //Create radius around tank 
                        Vector2D radius = power.position - t.Location;

                        //If distance between powerUp and tank is less than 30, it can't spawn
                        if (radius.Length() < 30)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
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
    }
}
