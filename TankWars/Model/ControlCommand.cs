using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// This class will represent movement and firing requests
    /// of a tank.  To be used by the GameController in sending 
    /// messages about tank movement.
    /// 
    /// @Author - Adam Scott & Andrew Porter
    /// </summary>
    /// 
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        //String representing whether player wants to move or not and direction
        [JsonProperty]
        private string moving;

        //String representing which weapon if any, the player used
        [JsonProperty]
        private string fire;

        //Direction the player is aiming
        [JsonProperty]
        private Vector2D tdir;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="upDownLeftRightNone"></param> direction if any of the tank
        /// <param name="noneMainAlt"></param>  which weapon was fired if any
        /// <param name="fireDirection"></param> Where the turret is directed
        public ControlCommand(string upDownLeftRightNone, string noneMainAlt, Vector2D fireDirection)
        {
            moving = upDownLeftRightNone;
            fire = noneMainAlt;
            tdir = fireDirection;
        }
    }
}
