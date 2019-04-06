using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    public class PathMatrix
    {
        private const int NoPath = 0xffffff;
        private readonly Dictionary<GraphNode, int> _nodesToNumbers = new Dictionary<GraphNode, int>();
        private readonly int[] _paths;
        private readonly int[] _distances;
        private readonly int _start;
        private readonly List<GraphNode> _numbersToNodes = new List<GraphNode>();

        public PathMatrix(Graph graph, GraphNode startNode)
        {
            int cnt = 0;
            foreach (var n in graph.AllNodes)
            {
                _numbersToNodes.Add(n);
                _nodesToNumbers[n] = cnt;
                cnt++;
            }

            var distanceMatrix = new int[cnt, cnt];
            for(var i=0;i<cnt; i++)
            for (var j = 0; j < cnt; j++)
                distanceMatrix[i, j] = NoPath;
            for(var c=0; c<cnt; c++)
                foreach (var n in _numbersToNodes[c].Neighbours)
                {
                    var ni = _nodesToNumbers[n];
                    distanceMatrix[c, ni] = 100 - _numbersToNodes[c].Speed;
                }
            var start = _nodesToNumbers[startNode];
            _start = start;
            var A = distanceMatrix;
            var N = cnt;
            var d = _distances = new int[cnt];
            var C = _paths = new int[cnt];
            var used = new bool[cnt];
            for (var i = 0; i < cnt; i++)
            {
                d[i] = NoPath;
                C[i] = -1;
            }

            d[start] = 0;

            while (true)
            {
                var curr = -1;
                var minDI = NoPath;
                // Find unused node with min d[i]
                for(var i=0; i<N; i++)
                    if (!used[i])
                    {
                        if (d[i] < minDI)
                        {
                            curr = i;
                            minDI = d[i];
                        }
                            
                    }

                if (curr == -1)
                    break;
                used[curr] = true;
                for(var i=0; i<N;i++)
                    if (!used[i])
                    {
                        var sum = d[curr] + A[curr, i];
                        if (d[i] > sum)
                        {
                            d[i] = sum;
                            C[i] = curr;
                        }
                    }
            }
        }

        public List<GraphNode> GetPath(GraphNode to)
        {
            var fromI = _start;
            var toI = _nodesToNumbers[to];
            if (_paths[toI] == -1)
                return null;

            var path = new List<int>();
            var j = fromI;
            var k = toI;
            while (k != j)
            {
                path.Add(k);
                k = _paths[k];
                //k = _pathMatrix[j, k].Value;
            }
            path.Reverse();

            return path.Select(x => _numbersToNodes[x]).ToList();
        }

        public List<(GraphNode node, int distance)> GetReachableNodes()
        {
            var rv = new List<(GraphNode node, int distance)>();
            for (var i = 0; i < _distances.Length; i++)
            {
                var d = _distances[i];
                if (d != NoPath)
                    rv.Add((_numbersToNodes[i], d));
            }

            return rv;
        }
    }
}