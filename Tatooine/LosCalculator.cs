using System;
using System.Buffers;
using Avalonia;

namespace Tatooine
{
    
    public class LosCalculator
    {
        private const double Radius = 0.5;
        private const double Height = 1;
        private const double Width = 0.866025403784439;
        private const double DeltaX = Width;
        private const double DeltaY = Height * 0.75;
        
        private static (double x, double y)[] Vertices = new (double, double)[6];
        private static (double x, double y)[] LosVertices = new (double, double)[7];

        static LosCalculator()
        {
            var r = Radius + 0.05;
            for (int c = 0; c < 6; c++)
            {
                var angle = c * Math.PI / 3;
                Vertices[c] = (Math.Sin(angle) * r, Math.Cos(angle) * r);
                LosVertices[c] = (Vertices[c].x / 2, Vertices[c].y / 2);
            }
        }
        
        private readonly Func<(int coll, int row), GameField.Terrain> _accessor;

        public LosCalculator(Func<(int coll, int row), GameField.Terrain> accessor)
        {
            _accessor = accessor;
        }

        public static (double x, double y) Center((int col, int row) hex) =>
            (hex.row % 2 == 0)
                ? (DeltaX * hex.col, DeltaY * hex.row)
                : (hex.col * DeltaX + DeltaX / 2, hex.row * DeltaY);

        
        
        static int Turns(double x0,double y0,double x1,double y1,double x2,double y2) {
            double cross;
            cross = (x1-x0)*(y2-y0) - (x2-x0)*(y1-y0);
            return ((cross > 0.0) ? -1 : ((cross == 0.0) ? 0 : 1));
        }

        
        static double Side(Point p, Point q, Point a, Point b)
        {
            var z1 = (b.X - a.X) * (p.Y - a.Y) - (p.X - a.X) * (b.Y - a.Y);
            var z2 = (b.X - a.X) * (q.Y - a.Y) - (q.X - a.X) * (b.Y - a.Y);
            return z1 * z2;
        }

/* Check whether segment P0P1 intersects with triangle t0t1t2 */
        static bool Intersecting(Point p0, Point p1, Point t0, Point t1, Point t2)
        {
            /* Check whether segment is outside one of the three half-planes
             * delimited by the triangle. */
            double f1 = Side(p0, t2, t0, t1), f2 = Side(p1, t2, t0, t1);
            double f3 = Side(p0, t0, t1, t2), f4 = Side(p1, t0, t1, t2);
            double f5 = Side(p0, t1, t2, t0), f6 = Side(p1, t1, t2, t0);
            /* Check whether triangle is totally inside one of the two half-planes
             * delimited by the segment. */
            double f7 = Side(t0, t1, p0, p1);
            double f8 = Side(t1, t2, p0, p1);

            /* If segment is strictly outside triangle, or triangle is strictly
             * apart from the line, we're not intersecting */
            if ((f1 < 0 && f2 < 0) || (f3 < 0 && f4 < 0) || (f5 < 0 && f6 < 0)
                || (f7 > 0 && f8 > 0))
                return false;// NOT_INTERSECTING;

            /* If segment is aligned with one of the edges, we're overlapping */
            if ((f1 == 0 && f2 == 0) || (f3 == 0 && f4 == 0) || (f5 == 0 && f6 == 0))
                return true;// OVERLAPPING;

            /* If segment is outside but not strictly, or triangle is apart but
             * not strictly, we're touching */
            if ((f1 <= 0 && f2 <= 0) || (f3 <= 0 && f4 <= 0) || (f5 <= 0 && f6 <= 0)
                || (f7 >= 0 && f8 >= 0))
                return false;// TOUCHING;

            /* If both segment points are strictly inside the triangle, we
             * are not intersecting either */
            if (f1 > 0 && f2 > 0 && f3 > 0 && f4 > 0 && f5 > 0 && f6 > 0)
                return false;// NOT_INTERSECTING;

            /* Otherwise we're intersecting with at least one edge */
            return true; //INTERSECTING;
        }
        
        static bool Intersects((int col, int row) hex, double x0, double y0, double x1, double y1)
        {
            
            var (cx, cy) = Center(hex);
            
            var cp = new Point(cx, cy);
            int side1, i, j;
            double hx(int v) => cx + Vertices[v].x;
            double hy(int v) => cy + Vertices[v].y;

            if(cy==y1)
                Console.Write("");
            for (var c = 0; c < 6; c++)
            {
                var n = c + 1;
                if (n >= 6)
                    n = 0;
                var v1 = new Point(hx(c), hy(c));
                var v2 = new Point(hx(n), hy(n));
                if (Intersecting(new Point(x0, y0), new Point(x1, y1), cp, v1, v2))
                    return true;
            }
            

            return false;
        }



        public bool IsOnLine((int col, int row) from, (int col, int row) to, (int col, int row) what)
        {
            var origin = Center(from);
            var target = Center(to);
            return Intersects(what, origin.x, origin.y, target.x, target.y);
        }
        
        public bool HasLos((int col, int row) from, (int col, int row) to, int vision)
        {
            var sourceTerrain = _accessor(from);
            if (sourceTerrain == GameField.Terrain.Rocks || sourceTerrain == GameField.Terrain.Invalid)
                return false;
            var targetTerrain = _accessor(to);
            var cubeOrigin = GameField.HexToCube(from);
            var cubeTarget = GameField.HexToCube(to);
            var origin = Center(from);
            var targetCenter = Center(to);

            var distance = GameField.CubeDistance(cubeOrigin, cubeTarget);
            if (distance > vision)
                return false;
            if (distance == 1)
                return true;

            bool Check((double x, double y) target)
            {
                for (var x = -distance; x <= distance; x++)
                for (var y = -distance; y <= distance; y++)
                for (var z = -distance; z <= distance; z++)
                    if (x + y + z == 0)
                    {
                        var (c, r) = GameField.CubeToHex(cubeOrigin.x + x, cubeOrigin.y + y, cubeOrigin.z + z);
                        if ((c, r) == from || (c, r) == to)
                            continue;
                        var terrain = _accessor((c, r));
                        if (terrain == GameField.Terrain.Rocks
                            || (targetTerrain != GameField.Terrain.Rocks && terrain == GameField.Terrain.Hills))
                        {
                            if (Intersects((c, r), origin.x, origin.y, target.x, target.y))
                                return false;
                        }
                    }

                return true;
            }

            for (var c = 0; c < LosVertices.Length; c++)
            {
                if (Check((targetCenter.x + LosVertices[c].x, targetCenter.y + LosVertices[c].y)))
                    return true;
            }

            return false;
        }
        
        
        
    }
    

}