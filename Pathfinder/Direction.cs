using System;

namespace Pathfinder
{
    public enum Direction
    {
        West, NorthWest, NorthEast, East, SouthEast, SouthWest
    }

    public static class DirectionUtils
    {
        public static Direction Left(Direction d)
        {
            d = d - 1;
            if (d < Direction.West)
                d = Direction.SouthWest;
            return d;
        }

        public static Direction Right(Direction d)
        {
            d++;
            if (d > Direction.SouthWest)
                d = Direction.West;
            return d;
        }

        public static Direction Inverse(Direction d)
        {
            return (Direction) (((int) d + 3) % 6);
        }

        public static bool IsNeighbourOrSame(Direction a, Direction b) => a == b || IsNeighbour(a, b);
        public static bool IsNeighbour(Direction a, Direction b)
        {
            return (a == Direction.West && b == Direction.SouthWest)
                   || (a == Direction.SouthWest && b == Direction.West)
                   || Math.Abs((int) a - (int) b) <= 1;
        }

        public static int Distance(Direction a, Direction b)
        {
            var ai = (int)a;
            var bi = (int) b;
            var diff = (Math.Max(ai, bi) - Math.Min(ai, bi));
            if (diff > 3)
                return 6 - diff;
            return diff;
        }
    }
}