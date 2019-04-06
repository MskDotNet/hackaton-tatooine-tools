namespace Tatooine
{
    public struct Location2D
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public static implicit operator Location2D((int cell, int row) c)
        {
            return new Location2D {Col = c.cell, Row = c.row};
        }

        public static implicit operator (int cell, int row)(Location2D l) => (Cell: l.Col, l.Row);

        public override string ToString()
        {
            var cube = GameField.HexToCube(Col, Row);
            var axial = GameField.CubeToLos(cube);
            var center = LosCalculator.Center(this);
            return
                $"2D: (cell: {Col}, row: {Row}), Cube: (x: {cube.x}, y: {cube.y}, z: {cube.z}) Axial: (q: {axial.q}, r: {axial.r}) LosCenter: ({center.x}, {center.y})";
        }
    }
}