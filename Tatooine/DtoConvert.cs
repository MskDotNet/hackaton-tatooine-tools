using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Tatooine
{
    public class DtoConvert
    {
        public static string Serialize(GameField field)
        {
            // Clone the map and place rocks at the perimeter
            field = field.Clone(field.Radius + 1, field.SightRadius, GameField.Terrain.Rocks);
            
            var cells = new List<MapCellDto>();

            MapCellDto GetCell((int cell, int row) pt)
            {
                var (x, y, z) = GameField.HexToCube(pt.cell, pt.row);

                var rv = new MapCellDto()
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Type = (SurfaceTypeDto) ((int) field[pt.cell, pt.row] - GameField.TerrainUtil.MinValidValue),
                    VisibleCells = new List<LocationDto>()
                };
                for (var c = -field.Radius; c <= field.Radius; c++)
                for (var r = -field.Radius; r <= field.Radius; r++)
                {
                    if (field.IsVisibleHex(pt, (c, r)))
                        rv.VisibleCells.Add(GetLocation((c, r)));
                }

                return rv;
            }
            
            LocationDto GetLocation((int cell, int row) pt)
            {
                var (x, y, z) = GameField.HexToCube(pt.cell, pt.row);

                return new LocationDto()
                {
                    X = x,
                    Y = y,
                    Z = z,
                    
                };
            }

            for (var c = -field.Radius; c <= field.Radius; c++)
            for (var r = -field.Radius; r <= field.Radius; r++)
                if (field[c, r] != GameField.Terrain.Invalid)
                    cells.Add(GetCell((c, r)));

            var dto = new MapDataDto()
            {
                Radius = field.Radius + 1,
                Cells = cells,
                Start = GetLocation(field.Start),
                Finish = GetLocation(field.Finish),
                SightRadius = field.SightRadius,
            };
            

            return JsonConvert.SerializeObject(dto);
        }

        public static GameField Deserialize(string data)
        {
            var dto = JsonConvert.DeserializeObject<MapDataDto>(data);
            var field = new GameField(dto.Radius - 1);
            foreach (var cell in dto.Cells)
            {
                var (c, r) = GameField.CubeToHex(cell.X, cell.Y, cell.Z);
                field[c, r] = (int) cell.Type + GameField.TerrainUtil.MinValidValue;
            }

            field.Start = GameField.CubeToHex(dto.Start.X, dto.Start.Y, dto.Start.Z);
            field.Finish = GameField.CubeToHex(dto.Finish.X, dto.Finish.Y, dto.Finish.Z);
            field.SightRadius = Math.Clamp(dto.SightRadius, 1, 4);
            
            // Cut the mountains on the border
            field = field.Clone(field.Radius - 1, field.SightRadius);
            return field;
        }

        class MapDataDto
        {
            public LocationDto Start { get; set; }

            public LocationDto Finish { get; set; }

            public int Radius { get; set; }
            public int SightRadius { get; set; }

            public IReadOnlyCollection<MapCellDto> Cells { get; set; }


        }

        public class LocationDto
        {

            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

        }

        enum SurfaceTypeDto
        {
            Empty = 0,
            Rock = 1,
            DangerousArea = 2,
            Pit = 3
        }


        class MapCellDto
        {
            public int X { get; set; }

            public int Y { get; set; }

            public int Z { get; set; }

            public SurfaceTypeDto Type { get; set; }
            
            public List<LocationDto> VisibleCells { get; set; }
        }


    }
}