using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;
using UnityEngine;

namespace SnakesAndMazes.TilingFramework
{
    public class TileSetBuilder : ITileSetBuilder<GameObject>
    {
        private List<PriorityTile> tiles;

        public TileSetBuilder(List<PriorityTile> tiles)
        {
            this.tiles = tiles;
        }

        public ITileSet<GameObject> Build()
        {
            TileSet tileSet = new TileSet(tiles);
            return tileSet;
        }
    }
}
