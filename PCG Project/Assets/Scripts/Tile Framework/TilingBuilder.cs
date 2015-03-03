using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;

using UnityEngine;

namespace SnakesAndMazes.TilingFramework
{
    public class TilingBuilder : ITilingBuilder<GameObject>
    {
        public int Height
        {
            get{ return height;}
        }
        public int Width
        {
            get{return Width;}
        }
        public TileSet TileSet
        {
            get { return tileSet; }
        }
        public TilingBuilder(int width, int height, TileSet tileSet)
        {
            this.width = width;
            this.height = height;
            this.tileSet = tileSet;
        }

        public Tile FullTile;

        //Based off of provided Tiling Example
        public ITiling<GameObject> BuildTiling()
        {
            return BuildTiling(null);
        }

        public ITiling<GameObject> BuildTiling(Portal[] prevPortals)
        {
            Tile[] layout = new Tile[width * height];
            List<Tuple<Direction, int>> edgeConstraints = new List<Tuple<Direction, int>>();

            #region maze generation

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    edgeConstraints.Clear();
                    if (i == 0)
                    {
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.S, 0);
                        edgeConstraints.Add(tp);
                    }
                    if (j == 0)
                    {
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.W, 0);
                        edgeConstraints.Add(tp);
                    }
                    if (i == height - 1)
                    {
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.N, 0);
                        edgeConstraints.Add(tp);
                    }
                    if (j == width - 1)
                    {
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.E, 0);
                        edgeConstraints.Add(tp);
                    }
                    if (i != 0)
                    {
                        Tile bottom = (Tile)layout[(i - 1) * width + j];
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.S, bottom.GetEdgeColor(Direction.N));
                        edgeConstraints.Add(tp);
                    }
                    if (j != 0)
                    {
                        Tile left = (Tile)layout[i * width + j - 1];
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.W, left.GetEdgeColor(Direction.E));
                        edgeConstraints.Add(tp);
                    }
                  
                    TileSet truchetTileSet = (TileSet) tileSet;
                    IEnumerable<PriorityTile> matched = truchetTileSet.MatchingTiles(edgeConstraints) as IEnumerable<PriorityTile>;
                    int sum = matched.Sum(x => x.priority);
                    int idx = UnityEngine.Random.Range(0, sum);
                    Tile selectedTile = null;
                    foreach (PriorityTile priorityTile in matched)
                    {
                        idx -= priorityTile.priority;
                        if (idx <= 0)
                        {
                            selectedTile = priorityTile.tile;
                            break;
                        }
                    }

                    if (selectedTile != null)
                    {
                        layout[i * width + j] = selectedTile;
                    }
                    else throw new Exception("No suitable tile found");
                }
            }

            #endregion

            #region Up Portals

            int[] upIndex = new int[3];
            int entry = -1;
            if (prevPortals != null && prevPortals[0] != null)
            {
                int portalCount = prevPortals.Count(p => p != null);
                for (int i = 0; i < portalCount; i++)
                {
                    int portalY = UnityEngine.Random.Range(0, height);
                    int portalX = UnityEngine.Random.Range(0, width);

                    while (upIndex.Contains(portalY * width + portalX + 1) || !layout[portalY * width + portalX].hasEdgeColor(1))
                    {
                        portalY = UnityEngine.Random.Range(0, height);
                        portalX = UnityEngine.Random.Range(0, width);
                    }
                    upIndex[i] = portalY * width + portalX + 1;
                }
            }
            else
            {
                int portalY = UnityEngine.Random.Range(0, height);
                int portalX = UnityEngine.Random.Range(0, width);

                while (!layout[portalY * width + portalX].hasEdgeColor(1))
                {
                    portalY = UnityEngine.Random.Range(0, height);
                    portalX = UnityEngine.Random.Range(0, width);
                }
                entry = portalY * width + portalX + 1;
            }

            List<PriorityTile> tiling = new List<PriorityTile>();
            for(int i=0;i<width*height;i++)
            {
                if (upIndex.Contains(i + 1)) tiling.Add(new PriorityTile(layout[i], PortalType.Up));
                else if (entry != -1 && entry - 1 == i) tiling.Add(new PriorityTile(layout[i], PortalType.Enter));
                else tiling.Add(new PriorityTile(layout[i], PortalType.None));
            }

            #endregion

            #region Eliminate disconnects

            List<int> connectedTiles = new List<int>();

            for (int i = 0; i < layout.Count(); i++)
            {
                if (!connectedTiles.Contains(i))
                {
                    exploredTiles = new List<int>();
                    DetermineIfConnected(tiling, i, null, ref connectedTiles);
                }
            }

            #endregion

            #region Down portals

            int[] downIndex = new int[3];

            int newPortalCount;

            if (tiling.Count(t => t.tile != FullTile) < 3)
                newPortalCount = 1;
            else
                newPortalCount = UnityEngine.Random.Range(1, 3);

            for (int i = 0; i < newPortalCount; i++)
            {
                List<int> indexes = tiling.Where(t => t.tile != FullTile).Select(x => tiling.IndexOf(x)).ToList();

                int index = indexes[UnityEngine.Random.Range(0, indexes.Count)];

                while (upIndex.Contains(index + 1) || downIndex.Contains(index + 1) || tiling[index].portalType == PortalType.Enter || !tiling[index].tile.hasEdgeColor(1))
                {
                    index = indexes[UnityEngine.Random.Range(0, indexes.Count)];
                }

                downIndex[i] = index + 1;
            }

            for (int i = 0; i < 3; i++)
                if (downIndex[i] != 0)
                    tiling[downIndex[i] - 1].portalType = PortalType.Down;

            #endregion

            Tiling truchetFinalTiling = new Tiling(tiling);
            return truchetFinalTiling;
        }

        private List<int> exploredTiles;
        private bool DetermineIfConnected(List<PriorityTile> tiling, int i, Direction? direction, ref List<int> connectedTiles)
        {
            if (connectedTiles.Contains(i))
            {
                return true; 
            }
            if (tiling[i].portalType != PortalType.None)
            {
                connectedTiles.Add(i);
                return true;
            }
            else
            {
                exploredTiles.Add(i);
                int up = i + width;
                int down = i - width;
                int left = i - 1;
                int right = i + 1;

                if  ((direction != Direction.S && !exploredTiles.Contains(up) && tiling[i].GetEdgeColor(Direction.N) == 1 && DetermineIfConnected(tiling, up, Direction.N, ref connectedTiles)) ||
                    (direction != Direction.N && !exploredTiles.Contains(down) && tiling[i].GetEdgeColor(Direction.S) == 1 && DetermineIfConnected(tiling, down, Direction.S, ref connectedTiles)) ||
                    (direction != Direction.E && !exploredTiles.Contains(left) && tiling[i].GetEdgeColor(Direction.W) == 1 && DetermineIfConnected(tiling, left, Direction.W, ref connectedTiles)) ||
                    (direction != Direction.W && !exploredTiles.Contains(right) && tiling[i].GetEdgeColor(Direction.E) == 1 && DetermineIfConnected(tiling, right, Direction.E, ref connectedTiles)))
                {
                    connectedTiles.Add(i);
                    return true;
                }
                else
                {
                    if(direction == null)
                        tiling[i].tile = FullTile;
                    return false;
                }
            }
        }

        private int width;
        private int height;
        private TileSet tileSet;
    }
}
