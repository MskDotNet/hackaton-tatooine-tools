using System;
using System.Runtime.Serialization;

namespace Tatooine
{
    public class GameField
    {
        private LosCalculator _los;
        public GameField(int radius, Terrain? fillWith = null)
        {
            
            _los = new LosCalculator(c=>this[c.coll, c.row]);
            Radius = radius;
            _field = new Terrain[Radius * 2 + 1, Radius * 2 + 1];
            Populate(false, fillWith);
        }
        public int Radius { get; }
        public Terrain[,] _field { get; }

        public int SightRadius { get; set; } = 3;

        public Terrain this[int cell, int row]
        {
            get
            {
                if (!IsValid((cell, row)))
                    return Terrain.Invalid;
                return _field[Radius + cell, Radius + row];
            }
            set { _field[Radius + cell, Radius + row] = value; }
        }

        public static (int x, int y, int z) HexToCube((int col, int row) c) => HexToCube(c.col, c.row);
        public static (int x, int y, int z) HexToCube(int col, int row)
        {
            var x = col - (row - (row & 1)) / 2;
            var z = row;
            var y = -x - z;
            return (x, y, z);
        }

        public static (int col, int row) CubeToHex((int x, int y, int z) c) => CubeToHex(c.x, c.y, c.z);
        public static (int col, int row) CubeToHex(int x, int y, int z)
        {
            var col = x + (z - (z & 1)) / 2;
            var row = z;
            return (col, row);
        }

        public static int CubeDistance((int x, int y, int z) a, (int x, int y, int z) b )
        {
            return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
        }

        public static (int q, int r) CubeToLos((int x, int y, int z) c)
        {
            return (-c.z, c.y);
        }

        public static (int x, int y, int z) AxialToCube((int q, int r) c)
        {
            var z = -c.q;
            var y = c.r;
            var x = -z - y;
            return (x, y, z);
        }

        //static int AxialFloor2(int x) => x >= 0 ? x >> 1 : ((x - 1) / 2);
        //static int AxialCeil2(int x) => x >= 0 ? ((x + 1) >> 1) : x / 2;
        
        static int DistanceAxial((int q, int r) a, (int q, int r)b )
        {
            var dq = b.q - a.q;
            var dr = b.r - a.r;

            if (Math.Sign(dq) == Math.Sign(dr))
                return Math.Max(Math.Abs(dq), Math.Abs(dr));
            return Math.Abs(dq) + Math.Abs(dr);
        }

        public bool IsVisibleHex((int col, int row) from, (int col, int row) what)
        {
            //return _los.IsOnLine(from, Finish, what);
            return _los.HasLos(from, what, SightRadius);
        }
        
        
        
        public enum Terrain
        {
            Invalid, 
            Plains,
            Rocks,
            Hills,
            Pits,
            TerraIncognita
        }

        public static class TerrainUtil
        {
            public const Terrain MaxValidValue = Terrain.Pits;
            public const Terrain MinValidValue = Terrain.Plains;
        }
        
        public (int col, int row) Start { get; set; }
        public (int col, int row) Finish { get; set; }

        public void Populate(bool randomize, Terrain? fillWith)
        {
            var rnd = new Random();
            for(var x=-Radius;x<=Radius; x++)
            for(var y=-Radius;y<=Radius; y++)
            for(var z=-Radius;z<=Radius; z++)
                if (x + y + z == 0)
                {
                    var (c, r) = CubeToHex(x, y, z);
                    if (randomize)
                        this[c, r] = (Terrain) rnd.Next((int) TerrainUtil.MinValidValue, (int) TerrainUtil.MaxValidValue);
                    else
                        this[c, r] = fillWith ?? TerrainUtil.MinValidValue;
                }

            Start = (0, 0);
            Finish = (Radius, 0);
        }

        public GameField Clone(int radius, int sightRadius, Terrain? fillWith = null)
        {
            var clone = new GameField(radius, fillWith);
            var mr = Math.Min(Radius, radius);
            for(var x=-mr;x<=mr; x++)
            for(var y=-mr;y<=mr; y++)
            for(var z=-mr;z<=mr; z++)
                if (x + y + z == 0)
                {
                    var (c, r) = CubeToHex(x, y, z);
                    clone[c, r] = this[c, r];
                }

            Location2D Clamp(Location2D loc) =>
                (Math.Clamp(loc.Col, -radius, radius), Math.Clamp(loc.Row, -radius, radius));

            clone.Start = Clamp(Start);
            clone.Finish = Clamp(Finish);
            clone.SightRadius = sightRadius;
            return clone;
        }

        public bool IsValid(Location2D nearest)
        {
            return !(Math.Abs(nearest.Col) > Radius || Math.Abs(nearest.Row) > Radius);
        }

        public GameField Mirror()
        {
            var clone = new GameField(Radius);
            var mr = Radius;
            for(var x=-mr;x<=mr; x++)
            for(var y=-mr;y<=mr; y++)
            for(var z=-mr;z<=mr; z++)
                if (x + y + z == 0)
                {
                    var (c, r) = CubeToHex(x, y, z);
                    var (nc, nr) = CubeToHex(z, x, y);
                    clone[nc, nr] = this[c, r];
                }

            (int, int) Mirror((int c, int r) p)
            {
                var (x, y, z) = HexToCube(p.c, p.r);
                return CubeToHex(z, x, y);
            }

            clone.Start = Mirror(Start);
            clone.Finish = Mirror(Finish);
            return clone;
        }
    }
    
    
}