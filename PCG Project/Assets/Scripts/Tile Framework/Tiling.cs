using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using OhioState.Tiling;

namespace SnakesAndMazes.TilingFramework
{
    public class Tiling : ITiling<GameObject>
    {
        private int width;
        private int height;
        private List<Tile> tiling;

        public Tiling(int width, int height, List<Tile> tiling)
        {
            // TODO: Complete member initialization
            this.width = width;
            this.height = height;
            this.tiling = tiling;
            NumberOfTiles = tiling.Count;
        }

        public int NumberOfTiles { get; set; }

        internal Tile GetTile(int p)
        {
            return tiling[p];
        }
    }
}
