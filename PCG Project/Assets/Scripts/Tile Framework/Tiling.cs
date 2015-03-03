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
        private List<PriorityTile> tiling;
        public float grade;

        public Tiling(List<PriorityTile> tiling)
        {
            this.tiling = tiling;
            NumberOfTiles = tiling.Count;
        }

        public int NumberOfTiles { get; set; }

        internal PriorityTile GetTile(int p)
        {
            return tiling[p];
        }
    }
}
