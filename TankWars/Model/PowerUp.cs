using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// This class represents the Power Up object. These power up objects have the potential 
    /// to spawn anywhere on the map and can be collected by players to unlock the special 
    /// "beam" attack. This class has properties representing its collected status, location, 
    /// and ID number.
    /// 
    /// @authors: Andrew Porter & Adam Scott
    /// @Date: 11-7-2020
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerUp
    {
        // Unique int ID number based on Power Up objects
        [JsonProperty(PropertyName = "power")]
        private int id;

        // Vector2D object representing the objects location on the map
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        // Boolean value representing whether or not the powerup has been collected
        [JsonProperty(PropertyName = "died")]
        private bool beenCollected;

        // Constructor for power up object, contains bool value whether it has been collected or not
        // a Vector2D object of its location and an int containing its ID number.
        public PowerUp(Vector2D loc, int ident)
        {
            beenCollected = false;
            location = loc;
            id = ident;
        }

        // Get Set for collected boolean status
        public bool collected
        {
            get { return beenCollected; }
            set { beenCollected = value; }
        }

        // Get Set for collected Vector2D position
        public Vector2D position
        {
            get { return location; }
            set { location = value; }
        }

        // Method that returns the ID number of the power up object
        public int getID()
        {
            return id;
        }
    }
}
