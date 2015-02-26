using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;

using UnityEngine;

namespace OhioState.Tiling
{
    public class TruchetTilingBuilder : ITilingBuilder<GameObject>
    {
        public int Height
        {
            get{ return height;}
        }
        public int Width
        {
            get{return Width;}
        }
        public TruchetTileSet TileSet
        {
            get { return tileSet; }
        }
        public TruchetTilingBuilder(int width, int height, TruchetTileSet tileSet)
        {
            this.width = width;
            this.height = height;
            this.tileSet = tileSet;
        }

        //Based off of provided Tiling Example
        public ITiling<GameObject> BuildTiling()
        {
            TruchetTile[] layout = new TruchetTile[width * height];
            List<Tuple<Direction, int>> edgeConstraints = new List<Tuple<Direction, int>>();

            System.Random rng = new System.Random(53423);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    edgeConstraints.Clear();
                    if (i == 0 && j == 0)
                    {
                        layout[0] = (TruchetTile)tileSet.GetTile(rng.Next(tileSet.NumberOfTiles));
                        continue;
                    }
                    if (i != 0)
                    {
                        TruchetTile bottom = (TruchetTile)layout[(i - 1) * width + j];
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.S, bottom.GetEdgeColor(Direction.N));
                        edgeConstraints.Add(tp);
                    }
                    if (j != 0)
                    {
                        TruchetTile left = (TruchetTile)layout[i * width + j - 1];
                        Tuple<Direction, int> tp = new Tuple<Direction, int>(Direction.W, left.GetEdgeColor(Direction.E));
                        edgeConstraints.Add(tp);
                    }
                  
                    TruchetTileSet truchetTileSet = (TruchetTileSet) tileSet;
                    IEnumerable<TruchetTile> matched = truchetTileSet.MatchingTiles(edgeConstraints) as IEnumerable<TruchetTile>;
                    int idx = rng.Next(matched.Count());
                    
                    
                    Debug.Log("matched " + matched.Count().ToString() + "choose " + idx.ToString());
                    TruchetTile truchetTile = matched.ElementAt(idx);
                    layout[i * width + j] = truchetTile;
                }
            }
            List<TruchetTile> tiling = new List<TruchetTile>();
            for(int i=0;i<width*height;i++)
            {
                tiling.Add((TruchetTile) layout[i]);
            }
            TruchetTiling truchetFinalTiling = new TruchetTiling(width, height, tiling);
            return truchetFinalTiling;
        }

        private int width;
        private int height;
        private TruchetTileSet tileSet;
    }
}
