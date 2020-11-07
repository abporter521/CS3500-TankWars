using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    class PowerUp
    {
        [JsonProperty(PropertyName = "power")]
        private int id;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "died")]
        private bool collected;

        public PowerUp()
        {

        }

    }
}
