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
        public bool detected;

        public PriorityTile(Tile tile, PortalType portalType)
        {
            this.tile = tile;
            this.portalType = portalType;
        }

        public PriorityTile(Tile tile, int priority)
        {
            this.tile = tile;
            this.priority = priority;
        }

        public PriorityTile(Tile tile, int priority, PortalType type)
        {
            this.tile = tile;
            this.priority = priority;
            this.portalType = type;
        }

        public Tile tile;
        public int priority;
        [Hide]
        public PortalType portalType;
        
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

    public enum PortalType
    {
        None,
        Enter,
        Up,
        Down
    }
}
