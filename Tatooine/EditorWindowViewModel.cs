using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using PropertyChanged;

namespace Tatooine
{
    [AddINotifyPropertyChangedInterface]
    public class EditorWindowViewModel
    {
        private const int DefaultRadius = 7;
        public struct EditorBrush
        {
            public bool IsFinish { get; set; }
            public bool IsStart { get; set; }
            public GameField.Terrain Terrain { get; set; }
            public override string ToString()
            {
                return IsFinish ? "Finish" : IsStart ? "Start" : Terrain.ToString();
            }
        }

        public string Title => "Tatooine Editor - " +
                               (Path == null ? "Untitled" : System.IO.Path.GetFileNameWithoutExtension(Path));
        private string Path { get; set; }
        public GameField GameField { get; set; } = new GameField(DefaultRadius);
        public EditorBrush TerrainBrush { get; set; }

        public AvaloniaList<EditorBrush> AvailableTerrainBrushes =>
            new AvaloniaList<EditorBrush>(


                Enum.GetValues(typeof(GameField.Terrain)).Cast<GameField.Terrain>()
                    .Where(t => t >= Tatooine.GameField.TerrainUtil.MinValidValue &&
                                t <= Tatooine.GameField.TerrainUtil.MaxValidValue)
                    .Distinct().Select(t => new EditorBrush {Terrain = t})
                    .Concat(new[] {new EditorBrush {IsStart = true}, new EditorBrush() {IsFinish = true}}));


        public int Radius
        {
            get => GameField.Radius;
            set { GameField = GameField.Clone(value, SightRadius, GameField.Terrain.Rocks); }
        }
        
        public int SightRadius
        {
            get => GameField.SightRadius;
            set { GameField = GameField.Clone(Radius, value); }
        }

        public GameFieldHighlightMode HighlightMode { get; set; } = GameFieldHighlightMode.Vision;

        public List<GameFieldHighlightMode> HighlightModes { get; } = Enum.GetValues(typeof(GameFieldHighlightMode))
            .Cast<GameFieldHighlightMode>().ToList();
        
        public EditorWindowViewModel()
        {
            TerrainBrush = AvailableTerrainBrushes.First();
        }

        public void OnPaint((int c, int r) pt)
        {
            if (GameField[pt.c, pt.r] == GameField.Terrain.Invalid)
                return;
            if (TerrainBrush.IsStart)
                GameField.Start = pt;
            else if (TerrainBrush.IsFinish)
                GameField.Finish = pt;
            else 
                GameField[pt.c, pt.r] = TerrainBrush.Terrain;
        }

        private static List<FileDialogFilter> Filters = new List<FileDialogFilter>
            {new FileDialogFilter() {Name = "Map", Extensions = new List<string> {"json"}}};
        public async void SaveAs()
        {
            var path = await new SaveFileDialog()
            {
                Filters = Filters,
                InitialFileName = "map.json",
                Title = "Save as..."
            }.ShowAsync(Application.Current.Windows.First());
            if (path != null)
            {
                if (!path.EndsWith(".json"))
                    path += ".json";
                Path = path;
                Save();
            }
        }

        public void Save()
        {
            if(Path == null)
                SaveAs();
            else
            {
                File.WriteAllText(Path, DtoConvert.Serialize(GameField));
            }
        }

        public async void Open()
        {
            var path = (await new OpenFileDialog()
            {
                Filters = Filters,
                InitialFileName = "map.json",
                Title = "Open map"
            }.ShowAsync(Application.Current.Windows.First()))?.FirstOrDefault();
            if (path != null)
            {
                var data = File.ReadAllText(path);
                var field = DtoConvert.Deserialize(data);
                Path = path;
                GameField = field;
            }
        }

        public void New()
        {
            Path = null;
            GameField = new GameField(DefaultRadius);
        }

        public void Mirror()
        {
            GameField = GameField.Mirror();
        }
    }
}