using OhioState.Tiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vexe.Runtime.Types;

namespace SnakesAndMazes.TilingFramework
{
    [Serializable]
    public class PriorityTile : ITile<GameObject>
    {
        public PriorityTile(Tile tile, int priority)
        {
            this.tile = tile;
            this.priority = priority;
        }

        public Tile tile;
        public int priority;

        public int EdgeCount 
        { 
            get { return tile.EdgeCount; } 
            set { tile.EdgeCount = value; }
        }

        internal int GetEdgeColor(Direction dir)
        {
            return tile.GetEdgeColor(dir);
        }
    }
}
