using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace IslesOfWar
{
    public class Constants
    {
        public int[] version; //Compatibility GSP Version, Effeciency Version, Client Version 0,0,0
        public bool isInMaintenanceMode;
        public string recieveAddress;

        public decimal resourcePackCost;
        public float[] resourcePackAmount;
        public float[] marketFeePrecent;
        public float[] minMarketFee;

        public float[] islandSearchCost;
        public float islandModifierExponent;
        public float attackCostPercent;
        public float undiscoveredPercent;
        public float islandSearchReplenishTime; //Estimated time in blocks it should take to get enough resources to search again.
        public string[] islandSearchOptions;

        public float squadHealthLimit;
        public float[,] unitCosts;
        public float[,] blockerCosts;
        public float[,] bunkerCosts;
        public float[,] collectorCosts;

        public float[] unitDamages;
        public float[] unitHealths;
        public float[] unitOrderProbabilities;
        public float[,] unitCombatModifiers;

        public int[,] minMaxResources;
        public float[] extractRates;
        public float[] freeResourceRates;
        public int extractPeriod;
        public int freeResourcePeriod;
        public int assumedDailyBlocks;

        public float[] tileProbabilities;
        public float[] resourceProbabilities;
        public float[,] purchaseToPoolPercents;
        public int poolRewardBlocks;
        public int warbucksRewardBlocks;

        public Constants()
        {
            version = new int[] { 0, 0, 0};
            isInMaintenanceMode = false;
            recieveAddress = "CWjRsUeXNA2c4jBS6TsABMGVqKL3fBNpHL";

            resourcePackCost = 50.0M;
            resourcePackAmount = new float[] {10000, 20000, 20000, 20000};
            marketFeePrecent = new float[] { 0.15f, 0.07f, 0.07f, 0.07f };
            minMarketFee = new float[] { 50, 200, 600, 100};

            islandSearchCost = new float[] { 100, 0, 0, 0 };
            islandModifierExponent = 0.1f;
            attackCostPercent = 0.1f;
            undiscoveredPercent = 0.5f;
            islandSearchReplenishTime = 1.0f; //Full Day
            islandSearchOptions = new string[] { "norm" };

            squadHealthLimit = 1000000;
            //Warbucks, Oil, Metal, No concrete because no unit cost concrete (maybe).
            unitCosts = new float[,]
            {
                {10,    0,      10,     0},
                {50,    0,      20,     0},
                {100,   10,     20,     0},
                {100,   25,     100,    0},
                {200,   50,     200,    0},
                {500,   100,    500,    0},
                {250,   100,    50,     0},
                {500,   250,    75,     0},
                {1000,  500,    200,    0}
            };

            blockerCosts = new float[,]
            {
                {1500, 100, 1000, 100 },
                {1500, 1000, 500, 500 },
                {1500, 500, 1000, 1000}
            };

            bunkerCosts = new float[,]
            {
                {1500, 100, 1000, 100 },
                {1500, 1000, 500, 500 },
                {1500, 500, 1000, 1000}
            };

            collectorCosts = new float[,]
            {
                {1500, 500, 1000, 1000 },
                {1500, 1000, 500, 1000 },
                {1500, 1000, 1000, 500 }
            };

            unitDamages = new float[]
            {
                2.0f, 3.0f, 4.0f,
                2.5f, 5.0f, 10.0f,
                8.0f, 12.0f, 14.0f,
                12.0f, 30.0f, 16.0f
            };

            unitHealths = new float[]
            {
                100.0f, 100.0f, 100.0f,
                125.0f, 250.0f, 500.0f,
                200.0f, 300.0f, 200.0f,
                300.0f, 750.0f, 400.0f
            };

            unitOrderProbabilities = new float[]
            {
                0.5f, 0.4f, 0.3f,
                0.1f, 0.085f, 0.065f,
                0.05f, 0.05f, 0.05f,
                0.01f, 0.01f, 0.01f
            };

            //12x12 grid - troop, machine, zook, lTank, mTank, hTank, lPlane, mPlane, bomber, troopBunk, tankBunk, airBunk
            unitCombatModifiers = new float[,]
            {
                {1.0f, 1.0f, 1.0f,     0.1f, 0.1f, 0.1f,    0.1f, 0.1f, 0.1f,     0.1f, 0.1f, 0.1f},
                {1.5f, 1.5f, 1.5f,     0.25f, 0.1f, 0.1f,   0.25f, 0.25f, 0.25f,  0.1f, 0.1f, 0.1f},
                {0.1f, 0.1f, 0.1f,     1.5f, 1.25f, 1.0f,   1.5f, 1.5f, 1.5f,     1.0f, 1.0f, 1.0f},
                {1.0f, 1.0f, 1.0f,     1.0f, 0.5f, 0.25f,   0.3f, 0.15f, 0.1f,    1.5f, 0.5f, 1.5f},
                {0.75f, 0.75f, 0.75f,  0.6f, 0.3f, 0.1f,    1.5f, 1.0f, 0.5f,     1.5f, 0.5f, 1.5f},
                {0.5f, 0.5f, 0.5f,     2.0f, 1.5f, 1.0f,    0.75f, 0.5f, 0.2f,    1.5f, 0.5f, 1.5f},
                {2.0f, 2.0f, 2.0f,     2.0f, 1.5f, 1.0f,    1.0f, 1.5f, 0.75f,    0.5f, 0.5f, 0.5f},
                {1.5f, 1.5f, 1.5f,     2.0f, 1.5f, 1.0f,    0.5f, 1.0f, 1.5f,     1.0f, 1.0f, 1.0f},
                {1.0f, 1.0f, 1.0f,     1.5f, 1.5f, 1.5f,    0.1f, 0.1f, 0.1f,     2.0f, 2.0f, 2.0f},
                {2.0f, 2.0f, 2.0f,     1.5f, 0.25f, 0.25f,  2.25f, 1.5f, 1.5f,    0.0f, 0.0f, 0.0f},
                {1.5f, 1.5f, 1.5f,     4.0f, 3.0f, 2.0f,    0.3f, 0.2f, 0.1f,     0.0f, 0.0f, 0.0f},
                {1.0f, 1.0f, 1.0f,     1.5f, 0.25f, 0.25f,  4.0f, 3.5f, 2.0f,     0.0f, 0.0f, 0.0f}
            };

            minMaxResources = new int[,]
            {
                {320, 448},
                {320, 448},
                {320, 448}
            };

            extractRates = new float[] { 10, 20, 5 };
            freeResourceRates = new float[] { 1, 1, 1, 1 };
            extractPeriod = 150;
            freeResourcePeriod = 480;
            assumedDailyBlocks = 9600;

            tileProbabilities = new float[] { 0.65f, 0.25f, 0.1f };
            resourceProbabilities = new float[] { 0.15f, 0.2f, 0.1f };

            //X = Warbucks Oil Metal Concrete 
            //Y = Units Collectors Defenses Search
            purchaseToPoolPercents = new float[,]
            {
                {0.15f, 0.05f, 0.05f, 0.05f},
                {0.15f, 0.05f, 0.05f, 0.05f},
                {0.15f, 0.05f, 0.05f, 0.05f},
                {0.15f, 0.05f, 0.05f, 0.05f}
            };

            poolRewardBlocks = 9600;
            warbucksRewardBlocks = 9600;
        }
    }
}
