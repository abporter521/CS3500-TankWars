using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// This class represents the Beam object. The Beam object represents a special attack that
    /// can be used after the tank collects a Power Up object.
    /// This class has properties representing its collected status, location, 
    /// and ID number.
    /// 
    /// @authors: Andrew Porter & Adam Scott
    /// @Date: 11-7-2020
    /// </summary>

    [JsonObject(MemberSerialization.OptIn)]

    public class Beam
    {
        // Unique int ID number based on Beam objects
        [JsonProperty(PropertyName = "beam")]
        private int id;

        // Vector2D object representing the origin of the Beam object
        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;

        // Vector2D object representing the direction of the Beam object
        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        // Int value correlating to the ID of the owner of the Beam object
        [JsonProperty(PropertyName = "owner")]
        private int tankOwner;

        // Constructor for Beam object, contains a Vector2D object representing its origin location,
        // a Vector2D object representing its direction, an int representing its unique ID number, and
        // an int representing the owner's ID number
        public Beam(Vector2D org, Vector2D dir, int ident, int tankOwner)
        {
            id = ident;
            origin = org;
            direction = dir;
            this.tankOwner = tankOwner;
        }

        // Method that returns the ID number of the Beam object
        public int GetID()
        {
            return id;
        }

        // Get Set for the origin of the Beam object
        public Vector2D Origin
        {
            get { return origin; }
            set { origin = value; }
        }

        // Get Set for the direction of the Beam object
        public Vector2D Direction
        {
            get { return direction; }
            set { direction = value; }
        }
    
        // Method that returns the ID number of the owner of the Beam object
        public int GetOwner()
        {
            return tankOwner;
        }
    }

}
