using System;
using Newtonsoft.Json;

namespace TankWars

{
    /// <summary>
    /// Class for a single Tank object.  The tank should 
    /// have a unique ID number.  Also contains information 
    /// about the location of the tank, its aim direction
    /// score, player name, and its death and connected state.
    /// 
    /// @Author Adam Scott & Andrew Porter
    /// @Date 6 November 2020
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        //A unique ID number for the tank
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        //Location of the tank in the world
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        //Orientation of the body of the tank
        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        //Orientation of the turret of the tank
        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0, -1);

        //Player name assigned to the tank
        [JsonProperty(PropertyName = "name")]
        private string name;

        //Health points of the tank object
        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = 3;

        //Player's score
        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        //State of the player
        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        //If player is connected to the game
        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        //If the player has joined a game
        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        private int powerUpNumber = 0;
        /// <summary>
        /// Constructor for the tank object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="playerName"></param>
        /// <param name="location"></param>
        public Tank(int id, string playerName, Vector2D location)
        {
            ID = id;
            name = playerName;
            this.location = location;
            orientation = new Vector2D(0, -1);
        }

        //Get method for ID 
        public int GetID()
        {
            return ID;
        }
        //Get method for Name
        public string GetName()
        {
            return name;
        }

        //Get method for Score
        public int GetScore()
        {
            return score;
        }

        //Get Set property related to location of projectile
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        //Get Set property related to location of projectile
        public Vector2D AimDirection
        {
            get { return aiming; }
            set { aiming = value; }
        }

        //Get Set for the score of the player
        public int Score
        {
            get  => score;
            set => score = value;
        }

        //Get Set for the health of the player
        public int HealthLevel
        {
            get => hitPoints;
            set => hitPoints = value;
        }

        //Get Set for death state of the player
        public bool HasDied
        {
            get => died;
            set => died = value;
        }
        //Get Set for death state of the player
        public Vector2D Orientation
        {
            get => orientation;
            set => orientation = value;
        }

        //Get Set for connectivity of the player
        public bool HasDisconnected
        {
            get => disconnected;
            set => disconnected = value;
        }

        public int PowerUpNumber
        {
            get => powerUpNumber;
        }
        // Increment number of powerups tank has
        public void CollectPowerup()
        {
            powerUpNumber++;
        }

        public void UsePowerup()
        {
            powerUpNumber--;
        }
    }

}
