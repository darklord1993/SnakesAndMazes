using SnakesAndMazes.TilingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SnakesAndMazes
{
    public class TilingGrader
    {
        public float averageLoopSize;
        public int loopCount; //note, this ignores "super loops"
        public int loopCountRange;

        public float Grade(Tiling tiling, int height, int width)
        {
            List<List<Tile>> loops = new List<List<Tile>>(); 

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = (Tile)tiling.GetTile(y * width + x);
                    List<Tile> smallestIncidentLoop = getSmallestLoop(tiling, tile, y, x, width);
                    if (smallestIncidentLoop == null) continue;
                    smallestIncidentLoop.OrderBy(l => l.transform.position.x + l.transform.position.z * 10);
                    if (!loops.Any(p => p.Equals(smallestIncidentLoop))) loops.Add(smallestIncidentLoop);
                }
            }

            int localCount = loops.Count;
            float localAverage = (float)loops.Sum(x => x.Count) / (float)localCount;

            localCount = Math.Abs(localCount - loopCount);
            if (localCount < loopCountRange) localCount = 0;
            else localCount -= loopCountRange;

            localAverage = Math.Abs(localAverage - averageLoopSize);

            return localCount + averageLoopSize;
        }

        private List<Tile> getSmallestLoop(Tiling tiling, Tile origin, int y, int x, int width)
        {
            int originPathCount = 1;

            List<TilePath> paths = new List<TilePath>();
            paths.Add(new TilePath(new List<Tile> {origin}, OhioState.Tiling.Direction.NE, y, x)); //NE as indication of first tile

            while (true)
            {
                TilePath path = paths[0];
                paths.RemoveAt(0);
                originPathCount -= 1;

                Tile endTile = path.path.Last();
                if(path.dir != OhioState.Tiling.Direction.E && endTile.GetEdgeColor(OhioState.Tiling.Direction.W) == 1)
                {
                    Tile left = (Tile)tiling.GetTile(path.i * width + path.j - 1);
                    if (left.Position() == origin.Position())
                        return path.path;
                    int leftPosition = left.Position();
                    if (left.EdgeColorCount(1) != 1 && !path.path.Any(k => k.Position() == leftPosition))
                    {
                        List<Tile> pathCopy = new List<Tile>();
                        for (int a = 0; a < path.path.Count; a++)
                            pathCopy.Add(path.path[a]);
                        pathCopy.Add(left);
                        TilePath newPath = new TilePath(pathCopy, OhioState.Tiling.Direction.W, path.i, path.j - 1);

                        paths.Add(newPath);

                    }
                }
                if (path.dir != OhioState.Tiling.Direction.W && endTile.GetEdgeColor(OhioState.Tiling.Direction.E) == 1)
                {
                    Tile right = (Tile)tiling.GetTile(path.i * width + path.j + 1);
                    if (right.Position() == origin.Position())
                        return path.path;
                    int rightPosition = right.Position();
                    if (right.EdgeColorCount(1) > 1 && !path.path.Any(k => k.Position() == rightPosition))
                    {
                        List<Tile> pathCopy = new List<Tile>();
                        for(int a = 0; a < path.path.Count; a++)
                            pathCopy.Add(path.path[a]);
                        pathCopy.Add(right);
                        TilePath newPath = new TilePath(pathCopy, OhioState.Tiling.Direction.E, path.i, path.j + 1);
                       
                        paths.Add(newPath);
                        originPathCount += 1;
                    }
                }
                if (path.dir != OhioState.Tiling.Direction.S && endTile.GetEdgeColor(OhioState.Tiling.Direction.N) == 1)
                {
                    Tile up = (Tile)tiling.GetTile((path.i + 1)* width + path.j);
                    if (up.Position() == origin.Position())
                        return path.path;
                    int upPosition = up.Position();
                    if (up.EdgeColorCount(1) != 1 && !path.path.Any(k => k.Position() == upPosition))
                    {
                        List<Tile> pathCopy = new List<Tile>();
                        for (int a = 0; a < path.path.Count; a++)
                            pathCopy.Add(path.path[a]);
                        pathCopy.Add(up);
                        TilePath newPath = new TilePath(pathCopy, OhioState.Tiling.Direction.S, path.i + 1, path.j);

                        paths.Add(newPath);
                        originPathCount += 1;
                    }
                }
                if (path.dir != OhioState.Tiling.Direction.N && endTile.GetEdgeColor(OhioState.Tiling.Direction.S) == 1)
                {
                    Tile down = (Tile)tiling.GetTile((path.i - 1) * width + path.j);
                    if (down.Position() == origin.Position())
                        return path.path;
                    int downPosition = down.Position();
                    if (down.EdgeColorCount(1) != 1 && !path.path.Any(k => k.Position() == downPosition))
                    {
                        List<Tile> pathCopy = new List<Tile>();
                        for (int a = 0; a < path.path.Count; a++)
                            pathCopy.Add(path.path[a]);
                        pathCopy.Add(down);
                        TilePath newPath = new TilePath(pathCopy, OhioState.Tiling.Direction.S, path.i - 1, path.j);

                        paths.Add(newPath);
                        originPathCount += 1;
                    }
                }

                if (originPathCount == 0 || originPathCount == 1) return null;
            }
        }

        private class TilePath
        {
            public TilePath(List<Tile> path, OhioState.Tiling.Direction dir, int i, int j)
            {
                this.path = path;
                this.dir = dir;
                this.i = i;
                this.j = j;
            }

            public int i;
            public int j;
            public List<Tile> path;
            public OhioState.Tiling.Direction dir;
        }
    }
}
