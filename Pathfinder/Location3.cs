using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pathfinder
{
    [DebuggerDisplay("{X}:{Y}:{Z}")]
    public struct Location3
    {
        public bool Equals(Location3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override string ToString() => $"{X}:{Y}:{Z}";

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Location3 other && Equals(other);
        }

        public static bool operator ==(Location3 loc, Location3 loc2) => loc.Equals(loc2);

        public static bool operator !=(Location3 loc, Location3 loc2) => !(loc == loc2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }

        private static readonly Dictionary<Direction, Location3> DirectionOffsets = new Dictionary<Direction, Location3>
        {

            [Pathfinder.Direction.West] = new Location3(-1, 1, 0),
            [Pathfinder.Direction.NorthWest] = new Location3(0, 1, -1),
            [Pathfinder.Direction.NorthEast] = new Location3(1, 0, -1),
            [Pathfinder.Direction.East] = new Location3(1, -1, 0),
            [Pathfinder.Direction.SouthEast] = new Location3(0, -1, 1),
            [Pathfinder.Direction.SouthWest] = new Location3(-1, 0, 1)
        };

        private static readonly Dictionary<Location3, Direction> OffsetDirections =
            DirectionOffsets.ToDictionary(x => x.Value, x => x.Key);
        
        public Location3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        
        public static Location3 operator +(Location3 left, Location3 right)
        {
            return new Location3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Location3 operator -(Location3 left, Location3 right)
        {
            return new Location3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public int Distance(Location3 other) =>
            (Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z)) / 2;
        
        public Location3 Direction(Direction d) => this + DirectionOffsets[d];

        public Direction GetDirection(Location3 other) => OffsetDirections[other - this];

        public IEnumerable<Location3> GetNeighbours()
        {
            foreach (var d in DirectionOffsets.Values)
                yield return this + d;
        }

        public IEnumerable<Location3> Ring(int radius)
        {
            var current = this + new Location3(0, -radius, radius);
            for(var c=0; c<6; c++)
            {
                for (var s = 0; s < radius; s++)
                {
                    current = current.Direction((Direction) c);
                    yield return current;
                }
            }
            yield break;
        }
    }
}