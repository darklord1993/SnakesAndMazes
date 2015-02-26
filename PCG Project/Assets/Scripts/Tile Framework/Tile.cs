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
    }
}
