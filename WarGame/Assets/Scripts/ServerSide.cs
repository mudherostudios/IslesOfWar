using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServerSide
{
    public class TileFeatureLookUp
    {
        public char[,] lookUpTable;

        public TileFeatureLookUp()
        {
            lookUpTable = new char[,]
            {
                { '0', '1', '2', '3', '4', '5', '6', '7' },
                { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' },
                { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' },
            };
        }

        public char GetFeatureCode(int tileType, int resourceType)
        {
            if (tileType == -1 || resourceType == -1)
                return '!';

            return lookUpTable[tileType, resourceType];
        }
    }

    public class IslandGenerator
    {
        public float[] tileProbabilities = new float[3];
        public float[] resourceProbabilities = new float[3];

        public IslandGenerator()
        {
            tileProbabilities[0] = 0.75f; //Flat Normal
            tileProbabilities[1] = 0.15f; //Lake
            tileProbabilities[2] = 0.10f; //Mountain
            resourceProbabilities[0] = 0.1f; //Oil
            resourceProbabilities[1] = 0.2f; //Limestone
            resourceProbabilities[2] = 0.15f; //Metal   
        }

        public IslandGenerator(float[] probabilities)
        {
            tileProbabilities[0] = probabilities[0];
            tileProbabilities[1] = probabilities[1];
            tileProbabilities[2] = probabilities[2];
            resourceProbabilities[0] = probabilities[3];
            resourceProbabilities[1] = probabilities[4];
            resourceProbabilities[2] = probabilities[5];
        }

        public string Generate()
        {
            string island = "";
            TileFeatureLookUp lut = new TileFeatureLookUp();

            for (int t = 0; t < 12; t++)
            {
                island += lut.GetFeatureCode(GetTileType(), GetResourceType()).ToString(); 
            }

            return island;
        }

        //Only allow two resources per tile
        //Make a matrix of unique resource combinations to understand this math.
        int GetResourceType()
        {
            int resource = 0;
            int count = 0;

            for (int p = 1; p < resourceProbabilities.Length+1 && count < 2; p++)
            {
                if (Random.value < resourceProbabilities[p-1])
                {
                    resource += p;
                    count++;
                }
            }
            
            if (count >= 2)
                resource += 1;

            return resource;
        }

        int GetTileType()
        {
            int type = -1;

            float feature = Random.value;
            float last = 0.0f;

            for (int p = 0; p < tileProbabilities.Length; p++)
            {
                if (feature <= tileProbabilities[p] + last)
                {
                    type = p;
                    p = tileProbabilities.Length;
                }
                else
                {
                    last += tileProbabilities[p];
                }
            }

            if (type == -1)
                Debug.Log(last);

            return type;
        }
    }
}
