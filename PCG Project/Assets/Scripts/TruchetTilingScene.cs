using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;
using UnityEngine;

namespace OhioState.Tiling
{
    public class TruchetTilingScene : MonoBehaviour
    {
        public List<TruchetTile> tiles;
        public int width;
        public int height;
        public float tileWidth;
        public float tileHeight;

        void Start()
        {
            TruchetTileSetBuilder builder = new TruchetTileSetBuilder(tiles);
            tileSet = (TruchetTileSet) builder.Build();

            TruchetTilingBuilder tilingBuilder = new TruchetTilingBuilder(width, height, tileSet);
            TruchetTiling tiling = (TruchetTiling)tilingBuilder.BuildTiling();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TruchetTile tile = (TruchetTile)tiling.GetTile(y * width + x);
                    Instantiate(tile.tilePrefab, new Vector3(tileWidth * x, 0, tileHeight * y), Quaternion.identity);
                }
            }
        }
        
        private TruchetTileSet tileSet;
    }
}
