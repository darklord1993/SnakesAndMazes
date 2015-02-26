using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OhioState.Tiling;
using UnityEngine;

namespace OhioState.Tiling
{
    public class TruchetTileSet : ITileSet<GameObject>
    {
        List<TruchetTile> tiles;
        public int NumberOfTiles {get;set;}

        public TruchetTileSet(List<TruchetTile> tiles)
        {
            this.tiles = tiles;
            NumberOfTiles = tiles.Count;
        }

        public bool MatchEdges(int c1, int c2)
        {
            if (c1 != c2)
                return false;
            return true;
        }

        public IEnumerable<TruchetTile> MatchingTiles(IEnumerable<Tuple<Direction, int>> edgeColorConstraints)
        {
            foreach (TruchetTile tile in tiles)
            {
                bool allSatisfied = true;
                foreach (Tuple<Direction, int> constraint in edgeColorConstraints)
                {
                    Direction dir = constraint.Item1;
                    int result = tile.GetEdgeColor(dir);

                    if (MatchEdges(constraint.Item2, result) == false)
                    {
                        allSatisfied = false;
                        break;
                    }
                }
                if (allSatisfied == true)
                    yield return tile;
            }
        }

        public ITile<GameObject> GetTile(int p)
        {
            return tiles[p];
        }
    }
}
