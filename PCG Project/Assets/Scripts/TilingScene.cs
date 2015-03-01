using SnakesAndMazes.TilingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vexe.Runtime.Types;

namespace SnakesAndMazes
{
    [Serializable]
    public class TilingScene : BetterBehaviour
    {
        public List<PriorityTile> tiles;
        public int seed;
        public int width;
        public int height;
        public float tileWidth;
        public float tileHeight;

        void Start()
        {
            TileSetBuilder builder = new TileSetBuilder(tiles);
            tileSet = (TileSet) builder.Build();

            TilingBuilder tilingBuilder = new TilingBuilder(width, height, tileSet);
            tilingBuilder.seed = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
            //tilingBuilder.seed = seed;
            Tiling tiling = (Tiling)tilingBuilder.BuildTiling();

            TilingGrader grader = new TilingGrader();
            grader.averageLoopSize = 6;
            grader.loopCount = 10;
            grader.loopCountRange = 5;

            float grade = grader.Grade(tiling, height, width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = (Tile)tiling.GetTile(y * width + x);
                    Instantiate(tile.tilePrefab, new Vector3(tileWidth * x, 0, tileHeight * y), Quaternion.identity);
                }
            }
        }
        
        private TileSet tileSet;
    }
}
