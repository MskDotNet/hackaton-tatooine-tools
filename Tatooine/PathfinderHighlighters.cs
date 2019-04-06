using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder;

namespace Tatooine
{
    public class PathfinderHighlighters
    {
        static Location3 Convert((int c, int r) l)
        {
            var cube = GameField.HexToCube(l);
            return new Location3(cube.x, cube.y, cube.z);
        }
        
        static Graph GetGraph(GameField f)
        {

            IEnumerable<MapCell> GetCells()
            {
                for(var c=-f.Radius; c<=f.Radius;c++)
                for (var r = -f.Radius; r <= f.Radius; r++)
                {
                    if (f.IsValid((c, r)))
                    {
                        yield return new MapCell
                        {
                            Location = Convert((c,r)),
                            SurfaceType = (SurfaceType) (f[c, r] - GameField.TerrainUtil.MinValidValue)
                        };
                    }
                }
            }

            return new Graph(GetCells(), Convert(f.Start), Convert(f.Finish));

        }
        
        public static Func<Location2D, bool> CreateNeighbourHighlighter(GameField f, Location2D from)
        {
            var graph = GetGraph(f);
            var nodes = graph.GetNodes(Convert(from)).ToList();
            if (nodes.Count == 0)
                return _ => false;
            return l =>
            {
                var converted = Convert(l);
                return nodes.Any(node => node.Neighbours.Any(n => n.Location == converted));
            };

        }
        
        public static Func<Location2D, bool> CreatePathHighlighter(GameField f, Location2D from)
        {
            if (!(f[from.Col, from.Row] == GameField.Terrain.Plains || f[from.Col, from.Row] == GameField.Terrain.Hills))
                return _ => false;
            var graph = GetGraph(f);
            var node = graph.GetNodes(Convert(from)).OrderBy(n => n.Speed).FirstOrDefault();
            var paths = new PathMatrix(graph, node);

            foreach (var finish in graph.GetNodes(Convert(f.Finish)))
            {
                var path = paths.GetPath(finish);
                if (path == null)
                    continue;
                var cpath = path.Select(n => GameField.CubeToHex(n.Location.X, n.Location.Y, n.Location.Z)).ToList();
                return l => cpath.Contains(l);
            }
            return _ => false;
        }
    }
}