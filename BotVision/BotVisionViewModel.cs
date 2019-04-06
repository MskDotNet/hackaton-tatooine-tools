using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder;
using PropertyChanged;
using Tatooine;

namespace BotVision
{
    [AddINotifyPropertyChangedInterface]
    public class BotVisionViewModel
    {
        private Dictionary<Location3, GameField.Terrain> _mapData = new Dictionary<Location3, GameField.Terrain>();
        public GameField Field { get; set; } = new GameField(1, GameField.Terrain.TerraIncognita);
        public string Status { get; set; }
        public List<Location2D> Highlight { get; set; } = new List<Location2D>();

        public void WeatherUpdate(ApiRaceStateDto race, bool newRace)
        {
            if (newRace)
                _mapData = new Dictionary<Location3, GameField.Terrain>();
            Highlight.Clear();
            Location2D Convert(Location3 l) => GameField.CubeToHex(l.X, l.Y, l.Z);
            foreach (var visible in race.NeighbourCells)
            {
                Highlight.Add(Convert(visible.Item1));
                var terrain = GameField.TerrainUtil.MinValidValue + (int) visible.Item2;
                _mapData[visible.Item1] = terrain;
            }

            var radius = race.Radius - 1;
            var field = new GameField(radius, GameField.Terrain.TerraIncognita);
            foreach (var ringCell in new Location3().Ring(radius))
            {
                var l = Convert(ringCell);
                field[l.Col, l.Row] = GameField.Terrain.Rocks;
            }
            
            foreach (var cell in _mapData)
            {
                var l = Convert(cell.Key);
                field[l.Col, l.Row] = cell.Value;
            }

            field.Start = Convert(race.CurrentLocation);
            field.Finish = Convert(race.Finish);
            Field = field;
            Status = $"Speed: {race.CurrentSpeed} Direction: {race.CurrentDirection} Fuel: {race.Fuel} {race.PlayerStatus}";
        }
        
    }
}