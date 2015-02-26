using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhioState.Tiling
{
    public class TruchetTiling : ITiling<GameObject>
    {
        private int width;
        private int height;
        private List<TruchetTile> tiling;

        public TruchetTiling(int width, int height, List<TruchetTile> tiling)
        {
            // TODO: Complete member initialization
            this.width = width;
            this.height = height;
            this.tiling = tiling;
            NumberOfTiles = tiling.Count;
        }

        public int NumberOfTiles { get; set; }

        internal TruchetTile GetTile(int p)
        {
            return tiling[p];
        }
    }
}
