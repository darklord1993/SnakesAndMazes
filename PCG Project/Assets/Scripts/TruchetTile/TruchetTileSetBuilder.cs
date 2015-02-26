using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;
using UnityEngine;

namespace OhioState.Tiling
{
    public class TruchetTileSetBuilder : ITileSetBuilder<GameObject>
    {
        private List<TruchetTile> tiles;

        public TruchetTileSetBuilder(List<TruchetTile> tiles)
        {
            this.tiles = tiles;
        }

        public ITileSet<GameObject> Build()
        {
            TruchetTileSet tileSet = new TruchetTileSet(tiles);
            return tileSet;
        }
    }
}
