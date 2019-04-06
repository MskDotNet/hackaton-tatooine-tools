using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PropertyChanged;

namespace Tatooine
{
    public enum GameFieldHighlightMode
    {
        None,
        Vision,
        Neighbours,
        Path
    }
    
    [DoNotNotify]
    public class GameFieldView : Control
    {
        public event Action<Location2D> PaintEvent;
        
        private int _hexagonSize = 48;
        private Dictionary<GameField.Terrain, IBrush> _brushes = new Dictionary<GameField.Terrain, IBrush>
        {
            [GameField.Terrain.Plains] = new SolidColorBrush(0xfffffdbf),
            [GameField.Terrain.Hills] = new SolidColorBrush(0xffaeac6f),
            [GameField.Terrain.Pits] = new SolidColorBrush(0xffb09124),
            [GameField.Terrain.Rocks] = new SolidColorBrush(0xff5e4900),
            [GameField.Terrain.TerraIncognita] = Brushes.Black
        };

        private static SolidColorBrush Highlight = new SolidColorBrush(0x7090ff90);
        private static SolidColorBrush FogOfWar = new SolidColorBrush(0x90404040);

        public static DirectProperty<GameFieldView, bool> DarkFowProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, bool>("DardFow", x => x.DarkFow,
                (x, v) => x.DarkFow = v, false);
        
        public static DirectProperty<GameFieldView, int> HexagonSizeProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, int>("HexagonSize", x => x.HexagonSize,
                (x, v) => x.HexagonSize = v, 48);

        private GameField _field;

        public static DirectProperty<GameFieldView, GameField> FieldProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, GameField>("Field", x => x.Field,
                (x, v) => x.Field = v);

        private Location2D _highlightLocation;

        public static DirectProperty<GameFieldView, Location2D> HighlightLocationProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, Location2D>("HighlightLocation", x => x.HighlightLocation,
                (x, v) => x.HighlightLocation = v);

        public int HexagonSize
        {
            get => _hexagonSize;
            set => SetAndRaise(HexagonSizeProperty, ref _hexagonSize, value);
        }

        public GameField Field
        {
            get => _field;
            set => SetAndRaise(FieldProperty, ref _field, value);
        }

        private bool _darkFow;
        

        public Location2D HighlightLocation
        {
            get { return _highlightLocation; }
            set { SetAndRaise(HighlightLocationProperty, ref _highlightLocation, value); }
        }

        public bool DarkFow
        {
            get { return _darkFow; }
            set { SetAndRaise(DarkFowProperty, ref _darkFow, value); }
        }

        public GameFieldHighlightMode HighlightMode
        {
            get { return _highlightMode; }
            set { SetAndRaise(HighlightModeProperty, ref _highlightMode, value); }
        }

        private GameFieldHighlightMode _highlightMode;

        public static DirectProperty<GameFieldView, GameFieldHighlightMode> HighlightModeProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, GameFieldHighlightMode>("HighlightMode",
                x => x.HighlightMode,
                (x, v) => x.HighlightMode = v);


        private List<Location2D> _highlightCells;

        public static readonly DirectProperty<GameFieldView, List<Location2D>> HighlightCellsProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, List<Location2D>>("HighlightCells", o => o.HighlightCells, (o, v) => o.HighlightCells = v);

        public List<Location2D> HighlightCells
        {
            get => _highlightCells;
            set => SetAndRaise(HighlightCellsProperty, ref _highlightCells, value);
        }


        private bool _highlightCursor = true;

        public static readonly DirectProperty<GameFieldView, bool> HighlightCursorProperty =
            AvaloniaProperty.RegisterDirect<GameFieldView, bool>("HighlightCursor", o => o.HighlightCursor, (o, v) => o.HighlightCursor = v);

        public bool HighlightCursor
        {
            get => _highlightCursor;
            set => SetAndRaise(HighlightCursorProperty, ref _highlightCursor, value);
        }
 

        static GameFieldView()
        {
            AffectsMeasure<GameFieldView>(HexagonSizeProperty, FieldProperty);
            AffectsRender<GameFieldView>(DarkFowProperty, HighlightModeProperty);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_field == null)
                return new Size();

            return new Size((_field.Radius * 2 + 1.5) * _hexagonSize,
                (_field.Radius * 2 + 1) * _hexagonSize * 0.75 + (_hexagonSize * 0.25));
        }
        
        Point GetCoordinate(int cell, int row)
        {
            var rv = (row % 2 == 0)
                ? new Point(cell * _hexagonSize, row * _hexagonSize * 0.75)
                : new Point(cell * _hexagonSize + _hexagonSize / 2d, row * _hexagonSize * 0.75);
            var center = _field.Radius * _hexagonSize + _hexagonSize/2d;
            return new Point(rv.X + center, rv.Y + center * 0.75 - _hexagonSize / 2d);

        }

        (int cell, int row) GetClosest(Point pt)
        {
            (int cc, int cr, double distance) = (0, 0, double.MaxValue);
            for (var c = -_field.Radius; c <= _field.Radius; c++)
            for (var r = -_field.Radius; r <= _field.Radius; r++)
            {
                var center = GetCoordinate(c, r) + new Vector(_hexagonSize / 2, _hexagonSize / 2);
                var dist = Math.Sqrt(Math.Pow(pt.X - center.X, 2) + Math.Pow(pt.Y - center.Y, 2));
                if (dist < distance)
                {
                    cc = c;
                    cr = r;
                    distance = dist;
                }
            }

            return (cc, cr);
        }

        void DoMapPaint(Point pt)
        {
            PaintEvent?.Invoke(GetClosest(pt));
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            var pointer = e.GetPosition(this);
            HighlightLocation = GetClosest(pointer);
            if(e.InputModifiers.HasFlag(InputModifiers.LeftMouseButton))
                DoMapPaint(pointer);
            base.OnPointerMoved(e);
            InvalidateVisual();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            DoMapPaint(e.GetPosition(this));               
            base.OnPointerPressed(e);
        }
        
        

        
        
        public override void Render(DrawingContext context)
        {

            var path = new StreamGeometry();

            var center = new Point(_hexagonSize / 2, _hexagonSize / 2);

            Point HexCorner(int corner)
            {
                var angleDeg = 60 * corner - 30;
                var angleRad = Math.PI / 180 * angleDeg;
                var radius = _hexagonSize / 2d;
                var ox = center.X + radius * Math.Sin(angleRad);
                return new Point(corner < 2 ? _hexagonSize : (corner == 2 || corner == 5) ? radius : 0,
                    center.Y + radius * Math.Sin(angleRad));
            }

            using (var p = path.Open())
            {
                p.BeginFigure(HexCorner(0), true);
                for (var c = 1; c < 6; c++)
                    p.LineTo(HexCorner(c));
                p.EndFigure(true);
            }


            Matrix GetMatrix((int c, int r) pt) => Matrix.CreateTranslation(GetCoordinate(pt.c, pt.r));

            var nearest = HighlightLocation;
            if (!_field.IsValid(nearest))
                nearest = _field.Start;

            Func<Location2D, bool> highlighter = _ => true;

            if (_highlightCells != null)
                highlighter = l => _highlightCells.Contains(l);
            
            if (_highlightMode == GameFieldHighlightMode.Vision)
                highlighter = l => _field.IsVisibleHex(nearest, l);
            #if !PUBLIC
            if (_highlightMode == GameFieldHighlightMode.Neighbours)
                highlighter = PathfinderHighlighters.CreateNeighbourHighlighter(_field, nearest);
            if (_highlightMode == GameFieldHighlightMode.Path)
                highlighter = PathfinderHighlighters.CreatePathHighlighter(_field, nearest);
            #endif
            for (var c = -_field.Radius; c <= _field.Radius; c++)
            for (var r = -_field.Radius; r <= _field.Radius; r++)
            {
                if (_brushes.TryGetValue(_field[c, r], out var brush))
                {
                    using (context.PushPreTransform(GetMatrix((c, r))))
                    {
                        context.DrawGeometry(brush, new Pen(Brushes.Black, 1), path);
                        if (!highlighter((c, r)))
                            context.DrawGeometry(_darkFow ? Brushes.Black : FogOfWar, null, path);
                    }
                }
            }

            if (_highlightCursor
                && Math.Abs(nearest.Col) <= _field.Radius
                && Math.Abs(nearest.Row) <= _field.Radius
                && _field[nearest.Col, nearest.Row] != GameField.Terrain.Invalid)
                using (context.PushPreTransform(GetMatrix((nearest.Col, nearest.Row))))
                    context.DrawGeometry(Highlight, null, path);

            using (context.PushPreTransform(GetMatrix(_field.Start)))
                context.DrawGeometry(null, new Pen(Brushes.Red, 4), path);
            
            using (context.PushPreTransform(GetMatrix(_field.Finish)))
                context.DrawGeometry(null, new Pen(Brushes.Blue, 2), path);


            base.Render(context);
        }
    }
}