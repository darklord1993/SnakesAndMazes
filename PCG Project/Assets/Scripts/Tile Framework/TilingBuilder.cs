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

        //Based off of provided Tiling Example
        public ITiling<GameObject> BuildTiling()
        {
            Tile[] layout = new Tile[width * height];
            List<Tuple<Direction, int>> edgeConstraints = new List<Tuple<Direction, int>>();

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
            List<Tile> tiling = new List<Tile>();
            for(int i=0;i<width*height;i++)
            {
                tiling.Add((Tile) layout[i]);
            }
            Tiling truchetFinalTiling = new Tiling(width, height, tiling);
            return truchetFinalTiling;
        }

        private int width;
        private int height;
        private TileSet tileSet;
    }
}
