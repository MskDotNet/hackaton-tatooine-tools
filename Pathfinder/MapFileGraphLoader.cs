using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Pathfinder
{
    public class MapFileGraphLoader
    {
        public static Graph LoadGraphFromMapFile(string path)
        {
            var s = File.ReadAllText(path);
            var map = JsonConvert.DeserializeObject<MapDataDto>(s);
            return new Graph(map.Cells.Select(x => new MapCell
            {
                Location = new Location3(x.X, x.Y, x.Z),
                SurfaceType = x.Type
            }), map.Start, map.Finish);
        }

        class MapCellDto
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public SurfaceType Type { get; set; }
        }
        
        class MapDataDto
        {
            public Location3 Start { get; set; }
            public Location3 Finish { get; set; }
            public List<MapCellDto> Cells { get; set; }
        }
    }
}