using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int id;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "died")]
        private bool dead;

        [JsonProperty(PropertyName = "owner")]
        private int tankOwner;

        public Projectile()
        {

        }

    }
}
