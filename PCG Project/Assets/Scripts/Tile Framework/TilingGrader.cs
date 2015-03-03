using SnakesAndMazes.TilingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SnakesAndMazes
{
    public class TilingGrader
    {
        public float averageLoopSize;
        public int loopCount; //note, this ignores "super loops"
        public int loopCountRange;
        public float percentMazeFilled;

        public float Grade(Tiling tiling, int height, int width)
        {
            List<List<int>> loops = new List<List<int>>(); 
            int localFullCount = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    PriorityTile tile = tiling.GetTile(y * width + x);
                    if (!tile.tile.hasEdgeColor(1)) localFullCount++;
                    List<TileNode> smallestIncidentLoop = getSmallestLoop(tiling, tile, y, x, width);
                    if (smallestIncidentLoop == null) continue;
                    List<int> loop = smallestIncidentLoop.Select(l => l.i * 100 + l.j).ToList();
                    loop.Sort();
                    if (!loops.Any(p => p.SequenceEqual(loop))) loops.Add(loop);
                }
            }

            int localCount = loops.Count;
            if (localCount == 0) localCount = 1;
            float localAverage = (float)loops.Sum(x => x.Count) / (float)localCount;

            localCount = Math.Abs(localCount - loopCount);
            if (localCount < loopCountRange) localCount = 0;
            else localCount -= loopCountRange;

            localAverage = Math.Abs(localAverage - averageLoopSize);

            float localFilledDifference = Math.Abs(percentMazeFilled - ((float)localFullCount / (float)(width * height)));

            return localCount + localAverage * 2 + localFilledDifference * 2;
        }

        private List<TileNode> getSmallestLoop(Tiling tiling, PriorityTile origin, int y, int x, int width)
        {
            int originPathCount = 1;

            List<List<TileNode>> paths = new List<List<TileNode>>();
            paths.Add(new List<TileNode> { new TileNode (origin, OhioState.Tiling.Direction.NE, y, x) }); //NE as indication of first tile

            while (true)
            {
                List<TileNode> path = paths[0];
                paths.RemoveAt(0);
                originPathCount -= 1;

                TileNode lastNode = path.Last();
                if(lastNode.dir != OhioState.Tiling.Direction.E && lastNode.tile.GetEdgeColor(OhioState.Tiling.Direction.W) == 1)
                {
                    PriorityTile left = tiling.GetTile(lastNode.i * width + lastNode.j - 1);
                    if (lastNode.i == y && lastNode.j - 1 == x)
                        return path;
                    if (left.tile.EdgeColorCount(1) > 1 && !path.Any(n => n.i == lastNode.i && n.j == lastNode.j - 1))
                    {
                        List<TileNode> newPath = new List<TileNode>();
                        for (int a = 0; a < path.Count; a++)
                            newPath.Add(new TileNode(path[a].tile, path[a].dir, path[a].i, path[a].j));
                        newPath.Add(new TileNode(left, OhioState.Tiling.Direction.W, lastNode.i, lastNode.j - 1));
                        paths.Add(newPath);
                        originPathCount++;
                    }
                }
                if(lastNode.dir != OhioState.Tiling.Direction.W && lastNode.tile.GetEdgeColor(OhioState.Tiling.Direction.E) == 1)
                {
                    PriorityTile right = tiling.GetTile(lastNode.i * width + lastNode.j + 1);
                    if (lastNode.i == y && lastNode.j + 1 == x)
                        return path;
                    if (right.tile.EdgeColorCount(1) > 1 && !path.Any(n => n.i == lastNode.i && n.j == lastNode.j + 1))
                    {
                        List<TileNode> newPath = new List<TileNode>();
                        for (int a = 0; a < path.Count; a++)
                            newPath.Add(new TileNode(path[a].tile, path[a].dir, path[a].i, path[a].j));
                        newPath.Add(new TileNode(right, OhioState.Tiling.Direction.E, lastNode.i, lastNode.j + 1));
                        paths.Add(newPath);
                        originPathCount++;
                    }
                }
                if(lastNode.dir != OhioState.Tiling.Direction.S && lastNode.tile.GetEdgeColor(OhioState.Tiling.Direction.N) == 1)
                {
                    PriorityTile up = tiling.GetTile((lastNode.i + 1) * width + lastNode.j);
                    if (lastNode.i + 1 == y && lastNode.j == x)
                        return path;
                    if (up.tile.EdgeColorCount(1) > 1 && !path.Any(n => n.i == lastNode.i + 1 && n.j == lastNode.j))
                    {
                        List<TileNode> newPath = new List<TileNode>();
                        for (int a = 0; a < path.Count; a++)
                            newPath.Add(new TileNode(path[a].tile, path[a].dir, path[a].i, path[a].j));
                        newPath.Add(new TileNode(up, OhioState.Tiling.Direction.N, lastNode.i + 1, lastNode.j));
                        paths.Add(newPath);
                        originPathCount++;
                    }
                }
                if(lastNode.dir != OhioState.Tiling.Direction.N && lastNode.tile.GetEdgeColor(OhioState.Tiling.Direction.S) == 1)
                {
                    PriorityTile up = tiling.GetTile((lastNode.i - 1) * width + lastNode.j);
                    if (lastNode.i - 1 == y && lastNode.j == x)
                        return path;
                    if (up.tile.EdgeColorCount(1) > 1 && !path.Any(n => n.i == lastNode.i - 1 && n.j == lastNode.j))
                    {
                        List<TileNode> newPath = new List<TileNode>();
                        for (int a = 0; a < path.Count; a++)
                            newPath.Add(new TileNode(path[a].tile, path[a].dir, path[a].i, path[a].j));
                        newPath.Add(new TileNode(up, OhioState.Tiling.Direction.S, lastNode.i - 1, lastNode.j));
                        paths.Add(newPath);
                        originPathCount++;
                    }
                }

                if (originPathCount < 2 || path.Count > 20)
                {
                    return null;
                }
            }
        }

        private class TileNode
        {
            public TileNode(PriorityTile tile, OhioState.Tiling.Direction dir, int i, int j)
            {
                this.tile = tile;
                this.dir = dir;
                this.i = i;
                this.j = j;
            }

            public int i;
            public int j;
            public PriorityTile tile;
            public OhioState.Tiling.Direction dir;
        }
    }
}
