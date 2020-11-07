using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        private int wallID;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D firstPoint;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D secondPoint;

        public Wall()
        {

        }
    }
}
