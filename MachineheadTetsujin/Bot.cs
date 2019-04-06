using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathfinder;

namespace MachineheadTetsujin
{
    public class Bot
    {
        private readonly ApiClient _client;
        public Direction Direction { get; private set; }
        public int Speed { get; private set; }
        public Dictionary<Location3, SurfaceType> MapData { get; private set; } = new Dictionary<Location3, SurfaceType>();
        public Location3 Finish { get; private set; }
        public Location3 Start { get; private set; }
        public Location3 Position { get; private set; }
        public PlayerStatusDto Status { get; private set; }
        public bool CanContinue => Status == PlayerStatusDto.NotBad || Status == PlayerStatusDto.Drifted;

        public Bot(ApiClient client, string login, string password, string map)
        {
            _client = client;
            _client.Login(login, password);
            _client.Start(map);
            var state = _client.GetRaceState();
            Start = state.CurrentLocation;
            Finish = state.Finish;
            foreach (var rock in default(Location3).Ring(state.Radius - 1))
                MapData[rock] = SurfaceType.Rock;
            Update();
        }

        void Update()
        {
            var state = _client.GetRaceState();
            Direction = state.CurrentDirection;
            Speed = state.CurrentSpeed;
            Position = state.CurrentLocation;
            Status = state.PlayerStatus;
            
            int discovered = 0;
            foreach (var cell in state.NeighbourCells)
            {
                if (MapData.TryAdd(cell.Item1, cell.Item2))
                    discovered++;
            }

            SurfaceType? terrain = null;
            if (MapData.TryGetValue(Position, out var xt))
                terrain = xt;
            Console.WriteLine($"Current state: {Position} ({terrain}) {Direction} {Speed} {Status}");
            
            if (discovered != 0)
                Console.WriteLine($"Discovered {discovered} new cells");
        }

        public void Turn(Direction direction, int accel)
        {
            Console.WriteLine($"Turn: {direction} {accel}");
            _client.Turn(direction, accel);
            Update();
        }

        public void MakeTurn()
        {
            var graph = new Graph(MapData.Select(x => new MapCell
            {
                Location = x.Key, SurfaceType = x.Value
            }), Start, Finish);
            var node = graph.GetNodes(Position).FirstOrDefault(n =>
                n.Direction == Direction
                && n.Speed == Speed);
            if(node == null)
                throw new Exception("Unable to find ourselves in the map graph");

            var paths = new PathMatrix(graph, node);
            var reachable = paths.GetReachableNodes();
            var target = reachable.Where(n => n.node.Location == Finish).Select(n => n.node).FirstOrDefault()
                         ?? reachable.Where(n => n.node.HasNearbyTi)
                             .OrderBy(n => n.node.SurfaceType)
                             .ThenBy(n => n.node.Speed)
                             .ThenBy(n => n.node.TiCount)
                             .ThenBy(n => n.distance)
                             .ThenBy(n => n.node.Location.Distance(Finish))
                             .Select(n => n.node).FirstOrDefault();
            if (target == null)
                throw new Exception("Nowhere to go");
            Console.WriteLine($"Chosen {target} as final target");
            var path = paths.GetPath(target);
            if (path == null)
                Console.WriteLine("Bug in pathfinder");
            var nextCell = path.First(x => x.Location != Position);
            Navigate(nextCell);
        }

        void Navigate(GraphNode target)
        {
            Console.WriteLine($"Trying to move to {target}");
            var direction = Position.GetDirection(target.Location);
            
            // Controlled drift
            if (target.SurfaceType == SurfaceType.Pit && DirectionUtils.Inverse(target.Direction) == Direction)
                direction = target.Direction;

            var accel = target.Speed - Speed;
            Turn(direction, accel);
            if (Position != target.Location || Speed != target.Speed ||
                target.Direction != Direction)
            {
                Console.WriteLine("Unexpected position change, investigate");
            }
        }
    }
}