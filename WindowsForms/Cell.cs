namespace WindowsForms
{
    internal class Cell
    {
        public CellType CellType;
        public bool Visited;
        public int X;
        public int Y;
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
