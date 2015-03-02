﻿using SnakesAndMazes.TilingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Vexe.Runtime.Types;

namespace SnakesAndMazes
{
    [Serializable]
    public class TilingScene : BetterBehaviour
    {
        public List<Tile> tiles;
        public int seed;
        public int width;
        public int height;
        public float tileWidth;
        public float tileHeight;

        public float averageLoopSize;
        public int loopCount;
        public int loopCountRange;
        public float threshold;

        private int count = 0;

        private List<PriorityTile>[] tileSets;

        private TilingGrader grader;
        private bool found = false;

        void Start()
        {
            seed = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second;
            System.Random rng = new System.Random(seed);

            tileSets = new List<PriorityTile>[4];
            for (int i = 0; i < 4; i++)
            {
                tileSets[i] = tiles.Select(t => new PriorityTile(t, rng.Next(0, 100))).ToList();
            }

            grader = new TilingGrader();
            grader.averageLoopSize = averageLoopSize;
            grader.loopCount = loopCount;
            grader.loopCountRange = loopCountRange;
        }

        void Update()
        {
            if (tileSets[1] == tileSets[0] || tileSets[2] == tileSets[0] || tileSets[3] == tileSets[0])
                Debug.Log("Same");

            if (found) return;
            Tiling[] tilings = new Tiling[4];

            for (int i = 0; i < 4; i++)
            {
                List<PriorityTile> tileSet = tileSets[i];

                Tiling tiling = BuildTiling(tileSet);
                GradeTiling(tiling);

                tilings[i] = tiling;
            }

            List<PriorityTile> optimalTileSet = null;
            if (EvaluateGeneration(tilings, ref optimalTileSet))
            {
                found = true;
                Debug.Log("Optimal Parameters:");
                foreach (PriorityTile tile in optimalTileSet)
                {
                    Debug.Log(tile.tile + ":" + tile.priority);
                }
            }
        }

        private bool EvaluateGeneration(Tiling[] tilings, ref List<PriorityTile> optimalTileSet)
        {
            Debug.Log(tilings.Min(t => t.grade));
            if (tilings.Any(t => t.grade < threshold))
            {
                if (count > 10)
                {
                    Tiling optimalTiling = tilings.First(t => t.grade == tilings.Min(x => x.grade));
                    for(int i = 0; i < 4; i++)
                        if (tilings[i] == optimalTiling)
                        {
                            InstantiateTiling(optimalTiling);
                            optimalTileSet = tileSets[i];
                            return true;
                        }
                }
                else
                {
                    count++;
                }
            }
            else
                count = 0;

            //Reverse grades to work better with selection algorithm
            float max = tilings.Max(t => t.grade);
            foreach (Tiling tiling in tilings)
                tiling.grade = max - tiling.grade + 2;

            //Genetic Algorithm
            List<PriorityTile>[] nextGeneration = new List<PriorityTile>[4];

            float sum = tilings.Sum(t => t.grade);
            for(int k = 0; k < 2; k++)
            {
                float selection = UnityEngine.Random.Range(0f, sum);

                for (int i = 0; i < 4; i++)
                {
                    selection -= tilings[i].grade;
                    if (selection <= 0)
                    {
                        nextGeneration[k * 2] = Copy(tileSets[i]);
                        float innerSum = 0;
                        for (int j = 0; j < 4; j++)
                            if (j != i) innerSum += tilings[j].grade;

                        float innerSelection = UnityEngine.Random.Range(0f, innerSum);
                        for (int j = 0; j < 4; j++)
                            if (j != i)
                            {
                                innerSelection -= tilings[j].grade;
                                if (innerSelection <= 0)
                                {
                                    nextGeneration[k * 2 + 1] = Copy(tileSets[j]);
                                    break;
                                }
                            }
                        break;
                    }
                }
            }
            for (int l = 0; l < 2; l++)
            {
                int pos = UnityEngine.Random.Range(0, tileSets[l].Count);
                for (int m = 0; m < pos; m++)
                {
                    int temp = nextGeneration[2 * l][m].priority;
                    nextGeneration[2 * l][m].priority = nextGeneration[2 * l + 1][m].priority;
                    nextGeneration[2 * l + 1][m].priority = temp;
                }
                int mutatePos = UnityEngine.Random.Range(0, tileSets[l].Count);
                int mutatePriority = UnityEngine.Random.Range(0, 100);

                nextGeneration[2 * l + 1][mutatePos].priority = mutatePriority;

                int oldPos = mutatePos;
                while (mutatePos == oldPos)
                {
                    mutatePos = UnityEngine.Random.Range(0, tileSets[l].Count);
                    mutatePriority = UnityEngine.Random.Range(0, 100);
                }

                nextGeneration[2 * l + 1][mutatePos].priority = mutatePriority;
            }

            tileSets = nextGeneration;
            return false;
        }

        private TileSet tileSet;

        public void InstantiateTiling(Tiling tiling)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = (Tile)tiling.GetTile(y * width + x);
                    Instantiate(tile.tilePrefab, new Vector3(tileWidth * x, 0, tileHeight * y), Quaternion.identity);
                }
                
            }
        }

        public Tiling BuildTiling(List<PriorityTile> tileSet)
        {
            TileSetBuilder builder = new TileSetBuilder(tileSet);
            var set = (TileSet)builder.Build();

            TilingBuilder tilingBuilder = new TilingBuilder(width, height, set);
            tilingBuilder.seed = seed;

            return (Tiling)tilingBuilder.BuildTiling();
        }

        public void GradeTiling(Tiling tiling)
        {
            tiling.grade = grader.Grade(tiling, height, width);
        }

        public List<PriorityTile> Copy(List<PriorityTile> list)
        {
            var newList = new List<PriorityTile>();
            for (int i = 0; i < list.Count; i++)
                newList.Add(new PriorityTile(list[i].tile, list[i].priority));
            return newList;
        }
    }
}
