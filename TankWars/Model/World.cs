using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    public class World
    {
        private Dictionary<int, Tank> tanks;
        private Dictionary<int, Wall> walls;
        private Dictionary<int, Projectile> projectiles;
        private Dictionary<int, Beam> beams;
        private Dictionary<int, PowerUp> powerUps;
        private int size;

        public World(int worldSize)
        {
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            projectiles = new Dictionary<int, Projectile>();
            beams = new Dictionary<int, Beam>();
            powerUps = new Dictionary<int, PowerUp>();
            size = worldSize;
        }

        public Dictionary<int, Tank> Tanks
        {
            get { return tanks; }
        }

        public Dictionary<int, Wall> Walls
        {
            get { return walls; }
        }

        public Dictionary<int, Projectile> Projectiles
        {
            get { return projectiles; }
        }

        public int Size
        {
            get { return size; }
        }

        public Dictionary<int, Beam> Beams
        {
            get { return beams; }
        }

        public Dictionary<int, PowerUp> PowerUps
        {
            get { return powerUps; }
        }


    }
}
