using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// This is a class containg the basic information for a wall object
    /// Contains fields for the ID number of the wall, and 2 location points
    /// 
    /// @Author- Adam Scott & Andrew Porter
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {   
        //A unique identifying number to this wall segment
        [JsonProperty(PropertyName = "wall")]
        private int wallID;

        //Contains the first point of the wall
        [JsonProperty(PropertyName = "p1")]
        private Vector2D firstPoint;

        //Contains the second point of the wall
        [JsonProperty(PropertyName = "p2")]
        private Vector2D secondPoint;

        /// <summary>
        /// Constructor for a wall object
        /// </summary>
        public Wall(int idNum, Vector2D start, Vector2D end)
        {
            wallID = idNum;
            firstPoint = start;
            secondPoint = end;
        }

        //Get method for ID method
        public int GetID()
        {
            return wallID;
        }

        //Get method for wall start point
        public Vector2D GetP1()
        {
            return firstPoint;
        }

        //Get method for wall start point
        public Vector2D GetP2()
        {
            return secondPoint;
        }
    }
}
