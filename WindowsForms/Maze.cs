using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WindowsForms
{
    public class Maze
    {
        private readonly Cell[,] _maze;
        private readonly Random _random;
        private Bitmap _mazeBitmap;
        private Cell _exitCell;
        private Cell _startCell;
        public Maze(int x, int y, Random r)
        {
            _maze = new Cell[x, y];
            _random = r;
        }

        public async Task<Bitmap> GetBitmapAsync()
        {
            if (_mazeBitmap != null)
            {
                return _mazeBitmap;
            }

            var tcs = new TaskCompletionSource<Bitmap>();
            await Task.Run(() =>
            {
                GenerateMaze();
                _mazeBitmap = GenerateBitmap();
                tcs.SetResult(_mazeBitmap);
            });
            return await tcs.Task;
        }

        private void GenerateMaze()
        {
            Prepare();
            var stack = new Stack<Cell>();
            var current = _maze[1, 1];
            current.Visited = true;
            do
            {
                var neighbors = GetNeighbors(current, 2);
                if (neighbors.Count > 0)
                {
                    stack.Push(current);
                    var r = _random.Next() % neighbors.Count;
                    var next = neighbors[r];
                    RemoveWall(current, next);
                    next.Visited = true;
                    current = next;
                }
                else if (stack.Count > 0)
                {
                    current = stack.Pop();
                }
                else
                {
                    var unvisited = GetUnvisitedCells().ToArray();
                    if (unvisited.Length <= 0)
                    {
                        break;
                    }

                    current = unvisited[_random.Next(0, unvisited.Length - 1)];
                    current.Visited = true;
                }
            } while (true);

            _maze[0, 1].CellType = CellType.Cell;
            _maze[_maze.GetLength(0) - 2, _maze.GetLength(1) - 1].CellType = CellType.Cell;
        }

        private void Prepare()
        {
            for (var i = 0; i < _maze.GetLength(0); i++)
            {
                for (var j = 0; j < _maze.GetLength(1); j++)
                {
                    _maze[i, j] = new Cell(i, j);
                    if (i % 2 != 0 && j % 2 != 0 && i < _maze.GetLength(0) - 1 && j < _maze.GetLength(1) - 1)
                    {
                        _maze[i, j].CellType = CellType.Cell;
                    }
                    else
                    {
                        _maze[i, j].CellType = CellType.Wall;
                    }
                }
            }
            _exitCell = _maze[_maze.GetLength(0) - 2, _maze.GetLength(1) - 1];
            _startCell = _maze[0, 1];
        }

        private List<Cell> GetNeighbors(Cell cell, int delta)
        {
            var neighbors = new List<Cell>();
            var a = new[]
            {
                (cell.X, cell.Y + delta), (cell.X, cell.Y - delta),
                (cell.X + delta, cell.Y), (cell.X - delta, cell.Y)
            };
            foreach (var tuple in a)
            {
                if (tuple.Item1 > 0 && tuple.Item2 > 0 && tuple.Item1 < _maze.GetLength(0) &&
                    tuple.Item2 < _maze.GetLength(1))
                {
                    var cur = _maze[tuple.Item1, tuple.Item2];
                    if (!cur.Visited && cur.CellType != CellType.Wall)
                    {
                        neighbors.Add(cur);
                    }
                }
            }

            return neighbors;
        }

        private void RemoveWall(Cell cell1, Cell cell2)
        {
            var xDiff = cell2.X - cell1.X;
            var yDiff = cell2.Y - cell1.Y;

            var addX = xDiff != 0 ? xDiff / Math.Abs(xDiff) : 0;
            var addY = yDiff != 0 ? yDiff / Math.Abs(yDiff) : 0;

            _maze[cell1.X + addX, cell1.Y + addY].CellType = CellType.Cell;
            _maze[cell1.X + addX, cell1.Y + addY].Visited = true;
        }

        private IEnumerable<Cell> GetUnvisitedCells()
        {
            for (var i = 0; i < _maze.GetLength(0); i++)
            {
                for (var j = 0; j < _maze.GetLength(1); j++)
                {
                    if (!_maze[i, j].Visited && _maze[i, j].CellType == CellType.Cell)
                    {
                        yield return _maze[i, j];
                    }
                }
            }
        }

        private Bitmap GenerateSolvedBitmap(Cell cell)
        {
            var bitmap = new Bitmap(_maze.GetLength(0), _maze.GetLength(1));

            for (var i = 0; i < _maze.GetLength(0); i++)
            {
                for (var j = 0; j < _maze.GetLength(1); j++)
                {
                    Color color;
                    switch (_maze[i, j].CellType)
                    {
                        case CellType.Wall:
                            color = Color.Black;
                            break;
                        case CellType.Cell:
                            color = Color.White;
                            break;
                        case CellType.Way:
                            color = Color.Green;
                            break;
                        case CellType.Seek:
                            color = Color.Blue;
                            break;
                        default:
                            color = Color.Aqua;
                            break;
                    }

                    bitmap.SetPixel(i, j, color);
                }
            }

            if (cell != null)
            {
                bitmap.SetPixel(cell.X, cell.Y, Color.Fuchsia);
            }

            return bitmap;
        }


        private Bitmap GenerateBitmap()
        {
            var bitmap = new Bitmap(_maze.GetLength(0), _maze.GetLength(1));

            for (var i = 0; i < _maze.GetLength(0); i++)
            {
                for (var j = 0; j < _maze.GetLength(1); j++)
                {
                    Color color;
                    switch (_maze[i, j].CellType)
                    {
                        case CellType.Wall:
                            color = Color.Black;
                            break;
                        default:
                            color = Color.White;
                            break;
                    }

                    bitmap.SetPixel(i, j, color);
                }

                bitmap.SetPixel(0, 1, Color.Fuchsia);
            }

            return bitmap;
        }

        public IEnumerable<Bitmap> GetSolveTrack()
        {
            PrepareSolve();
            var stack = new Stack<Cell>();
            var current = _startCell;
            current.Visited = true;
            do
            {
                current = SolveStep(stack, current);
                yield return GenerateSolvedBitmap(current);
            } while (current != _exitCell);
        }

        private Cell SolveStep(Stack<Cell> stack, Cell current)
        {
            var neighbors = GetNeighbors(current, 1);
            if (neighbors.Count > 0)
            {
                current.CellType = CellType.Way;
                stack.Push(current);
                var r = _random.Next() % neighbors.Count;
                var next = neighbors[r];
                next.Visited = true;
                return next;
            }
            else if (stack.Count > 0)
            {
                current.CellType = CellType.Seek;
                return stack.Pop();
            }

            throw new ArgumentException("Maze is unsolvable");
        }
        private Bitmap GetSolve()
        {
            PrepareSolve();
            var stack = new Stack<Cell>();
            var current = _startCell;
            current.Visited = true;
            do
            {
                current = SolveStep(stack, current);
            } while (current != _exitCell);

            return GenerateSolvedBitmap(current);
        }

        private void PrepareSolve()
        {
            for (var i = 0; i < _maze.GetLength(0); i++)
            {
                for (var j = 0; j < _maze.GetLength(1); j++)
                {
                    _maze[i, j].Visited = false;
                    if (_maze[i, j].CellType != CellType.Wall)
                    {
                        _maze[i, j].CellType = CellType.Cell;
                    }
                }
            }
        }

        public async Task<Bitmap> GetSolveBitmapAsync()
        {
            var tcs = new TaskCompletionSource<Bitmap>();
#pragma warning disable 4014
            Task.Run(() =>
#pragma warning restore 4014
            {
                var bitmap = GetSolve();
                tcs.SetResult(bitmap);
            });
            return await tcs.Task;
        }
    }
}