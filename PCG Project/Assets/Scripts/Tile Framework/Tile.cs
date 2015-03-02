using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;
using UnityEngine;
using Vexe.Runtime.Types;

namespace SnakesAndMazes.TilingFramework
{
    [Serializable]
    public class Tile : BetterBehaviour, ITile<GameObject>
    {
        public int topEdgeColor;
        public int bottomEdgeColor;
        public int leftEdgeColor;
        public int rightEdgeColor;
        public GameObject tilePrefab;

        public int EdgeCount { get; set; }

        internal int GetEdgeColor(Direction dir)
        {
            switch (dir)
            {
                case Direction.N:
                    return topEdgeColor;
                case Direction.S:
                    return bottomEdgeColor;
                case Direction.E:
                    return rightEdgeColor;
                case Direction.W:
                    return leftEdgeColor;
            }

            throw new Exception("This edge direction is not supported");
        }

        internal int EdgeColorCount(int color)
        {
            int count = 0;
            if (GetEdgeColor(OhioState.Tiling.Direction.N) == color) count++;
            if (GetEdgeColor(OhioState.Tiling.Direction.W) == color) count++;
            if (GetEdgeColor(OhioState.Tiling.Direction.E) == color) count++;
            if (GetEdgeColor(OhioState.Tiling.Direction.S) == color) count++;
            return count;
        }

        internal bool hasEdgeColor(int color)
        {
            return GetEdgeColor(OhioState.Tiling.Direction.N) == color ||
                   GetEdgeColor(OhioState.Tiling.Direction.W) == color ||
                   GetEdgeColor(OhioState.Tiling.Direction.E) == color ||
                   GetEdgeColor(OhioState.Tiling.Direction.S) == color;
        }
    }
}
