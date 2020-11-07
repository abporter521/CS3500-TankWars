using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        private int beamNum;

        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        [JsonProperty(PropertyName = "owner")]
        private int tankOwner;

        public Beam()
        {

        }


    }
}
