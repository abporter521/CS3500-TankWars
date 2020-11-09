using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// This is the Projectile class that will be created when a tank object shoots.
    /// This object contains fields for location, orientation, the ID number of the 
    /// tank that fired the shot, and a unique identifying number for the projectile itself
    /// 
    /// @Authors Andrew Porter & Adam Scott\
    /// @Date 6 November 2020
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        //Contains the identifying number of the projectile
        [JsonProperty(PropertyName = "proj")]
        private int id;

        //Vector2D containing the location of the projectile
        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        //Vector 2D containing the direction of the projectile
        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        //Bool containing the status of the projectile
        //If true, projectile has collided with tank, wall,
        //or the destination. Otherwise it is traveling
        [JsonProperty(PropertyName = "died")]
        private bool dead;

        //Int with the ID number of the tank that fired the shot
        [JsonProperty(PropertyName = "owner")]
        private int tankOwner;

        /// <summary>
        /// This is a constructor for a projectile object
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="loc"></param>
        /// <param name="dir"></param>
        /// <param name="tankOwner"></param>
        public Projectile(int ident, Vector2D loc, Vector2D dir, int tankOwner)
        {
            id = ident;
            location = loc;
            orientation = dir;
            this.tankOwner = tankOwner;
            dead = false;
        }

        //Getter method to retrieve ID number of projectile
        public int getID()
        {
            return id;
        }

        //Get Set property related to location of projectile
        public Vector2D Location
        {
            get { return location; }
            set { location = value; }
        }

        //Get Set property related to direction of projectile
        public Vector2D Direction
        {
            get { return Direction; }
            set { orientation = value; }
        }

        //Retrieves ID number of tank who fired the shot
        public int getOwner()
        {
            return tankOwner;
        }

        //Get Set property related to projectile's lifespan
        public bool Died
        {
            get { return dead; }
            set { dead = value; }
        }
    }
}
