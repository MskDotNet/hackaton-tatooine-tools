//#define NODRIFT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Pathfinder
{
    [DebuggerDisplay("{Location} {SurfaceType} {Speed} {Direction.Value} {HasNearbyTi}")]
    public class GraphNode
    {
         public SurfaceType SurfaceType { get; set; }
         public Direction Direction { get; set; }
         public Location3 Location { get; set; }
         public int Speed { get; set; }
         public bool HasNearbyTi => TiCount != 0;
         public int TiCount { get; set; }
         public List<GraphNode> Neighbours { get; set; } = new List<GraphNode>();

         public GraphNode(Location3 location, SurfaceType surface, int speed, Direction direction)
         {
             Location = location;
             SurfaceType = surface;
             Speed = speed;
             Direction = direction;
         }

         public override string ToString()
         {
             return $"{Location} {Direction} {Speed} {SurfaceType}";
         }
    }

    public class MapCell
    {
        public Location3 Location { get; set; }
        public SurfaceType SurfaceType { get; set; }
    }

    public static class Speeds
    {
        public const int Zero = 0;
        public const int Hills = 29;
        public const int Jumper = 40;
        public const int Pits = 70;

        public const int Angle1Drift = 90;
        public const int Angle2Drift = 60;
        public const int Angle3Drift = 30;
        
        
    }
    
    public class Graph
    {
        private static Dictionary<int, int> Drifts = new Dictionary<int, int>
        {
            [0] = 1000,
            [1] = 90,
            [2] = 60,
            [3] = 30
        };

        private const int MaxAccel = 30;
        private Dictionary<Location3, List<GraphNode>> _nodes = new Dictionary<Location3, List<GraphNode>>();
        
        public Location3 FinishLocation { get; }
        public Graph(IEnumerable<MapCell> cells, Location3 start, Location3 finish)
        {
            FinishLocation = finish;
            // Populate cells
            var allDirections = Enum.GetValues(typeof(Direction)).Cast<Direction>().ToList();


            foreach (var cell in cells)
            {
                var lst = new List<GraphNode>();
                void AddAllDirections(Location3 loc, SurfaceType surface, int speed)
                {
                    foreach (var d in allDirections) lst.Add(new GraphNode(loc, surface, speed, d));
                }

                if (cell.SurfaceType == SurfaceType.Rock)
                    AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Zero);
                if (cell.SurfaceType == SurfaceType.DangerousArea)
                    AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Hills);
                if (cell.SurfaceType == SurfaceType.Empty)
                {
                    AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Jumper);
                    AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Hills);
                    // We start at zero speed
                    if (cell.Location == start)
                        AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Zero);                   
                }

                if (cell.SurfaceType == SurfaceType.Pit)
                {
                    AddAllDirections(cell.Location, cell.SurfaceType, Speeds.Pits);
                }

                _nodes[cell.Location] = lst;
            }
            foreach(var lst in _nodes.Values)
            foreach (var n in lst)
                BuildNodeNeighbours(n);
        }

        private void BuildNodeNeighbours(GraphNode t)
        {
            if (t.SurfaceType == SurfaceType.Rock)
                return;
            foreach (var nloc in t.Location.GetNeighbours())
            {
                var nodes = GetNodes(nloc).ToList();
                if (nodes.Count == 0)
                    t.TiCount++;
                
                var dir = t.Location.GetDirection(nloc);
                var angle = DirectionUtils.Distance(t.Direction, dir);
                var maxTurnSpeed = Drifts[angle];
                foreach (var ne in GetNodes(nloc))
                {
                    if (ne.SurfaceType == SurfaceType.Rock)
                        continue;
                    // Filter out nodes facing in different directions
                    if (ne.Direction != dir)
                        continue;
                    // Check if we can turn there
                    if (ne.Speed > maxTurnSpeed)
                    {
                        continue;
                    }

                    // Check if we can accelerate here
                    if (Math.Abs(ne.Speed - t.Speed) > MaxAccel)
                        continue;
                    t.Neighbours.Add(ne);
                }
            }
            
        }

        public IEnumerable<GraphNode> GetNodes(Location3 location)
        {
            if (!_nodes.TryGetValue(location, out var lst))
                yield break;
            foreach (var n in lst)
                yield return n;
        }

        public IEnumerable<GraphNode> AllNodes => _nodes.Values.SelectMany(n => n);
    }
}
