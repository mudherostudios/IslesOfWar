namespace IslesOfWar.Combat
{
    //Eventually remove all of the named variables and convert it to just a double array
    public class Squad
    {
        public double riflemen, machineGunners, bazookamen;
        public double lightTanks, mediumTanks, heavyTanks;
        public double lightFighters, mediumFighters, bombers;
        public double troopBunkers, tankBunkers, antiAircrafts;
        public double totalHealth, totalDamage;
        public double[] unitProbabilities;
        public int damagedUnit;
        public double remainingHealth;

        public Squad()
        {

        }

        public Squad(int[] squadCounts)
        {
            riflemen = squadCounts[0];
            machineGunners = squadCounts[1];
            bazookamen = squadCounts[2];

            lightTanks = squadCounts[3];
            mediumTanks = squadCounts[4];
            heavyTanks = squadCounts[5];

            lightFighters = squadCounts[6];
            mediumFighters = squadCounts[7];
            bombers = squadCounts[8];

            if (squadCounts.Length > 9)
            {
                troopBunkers = squadCounts[9];
                tankBunkers = squadCounts[10];
                antiAircrafts = squadCounts[11];
            }
            else
            {
                troopBunkers = 0;
                tankBunkers = 0;
                antiAircrafts = 0;
            }

            totalHealth = 0;
            totalDamage = 0;

            unitProbabilities = new double[12];

            damagedUnit = -1;
            remainingHealth = 0.0;
        }

        public Squad(long[] squadCounts)
        {
            riflemen = squadCounts[0];
            machineGunners = squadCounts[1];
            bazookamen = squadCounts[2];

            lightTanks = squadCounts[3];
            mediumTanks = squadCounts[4];
            heavyTanks = squadCounts[5];

            lightFighters = squadCounts[6];
            mediumFighters = squadCounts[7];
            bombers = squadCounts[8];

            if (squadCounts.Length > 9)
            {
                troopBunkers = squadCounts[9];
                tankBunkers = squadCounts[10];
                antiAircrafts = squadCounts[11];
            }
            else
            {
                troopBunkers = 0;
                tankBunkers = 0;
                antiAircrafts = 0;
            }

            totalHealth = 0;
            totalDamage = 0;

            unitProbabilities = new double[12];

            damagedUnit = -1;
            remainingHealth = 0.0;
        }

        public Squad(double[] squadCounts)
        {
            riflemen = squadCounts[0];
            machineGunners = squadCounts[1];
            bazookamen = squadCounts[2];

            lightTanks = squadCounts[3];
            mediumTanks = squadCounts[4];
            heavyTanks = squadCounts[5];

            lightFighters = squadCounts[6];
            mediumFighters = squadCounts[7];
            bombers = squadCounts[8];

            if (squadCounts.Length > 9)
            {
                troopBunkers = squadCounts[9];
                tankBunkers = squadCounts[10];
                antiAircrafts = squadCounts[11];
            }
            else
            {
                troopBunkers = 0;
                tankBunkers = 0;
                antiAircrafts = 0;
            }

            totalHealth = 0;
            totalDamage = 0;

            unitProbabilities = new double[12];

            damagedUnit = -1;
            remainingHealth = 0.0;
        }

        public double[] fullSquad
        {
            get
            {
                double[] squad = new double[]
                {
                    riflemen, machineGunners, bazookamen,
                    lightTanks, mediumTanks, heavyTanks,
                    lightFighters, mediumFighters, bombers,
                    troopBunkers, tankBunkers, antiAircrafts
                };

                return squad;
            }
        }

        public double[] onlyUnits
        {
            get
            {
                double[] squad = new double[]
                {
                    riflemen, machineGunners, bazookamen,
                    lightTanks, mediumTanks, heavyTanks,
                    lightFighters, mediumFighters, bombers
                };

                return squad;
            }
        }

        public int[] bunkers
        {
            get { return new int[] { (int)troopBunkers * 1, (int)tankBunkers * 2, (int)antiAircrafts * 3 }; }
        }

        public void AddBunkers(int[] bunkers)
        {
            if (bunkers[0] > 0)
                troopBunkers = 1;
            if (bunkers[1] > 0)
                tankBunkers = 1;
            if (bunkers[2] > 0)
                antiAircrafts = 1;
        }

        public void CalculateCasualties(Squad attacker, ref MudHeroRandom random, float[] unitHealths, float[] unitOrderProbabilities)
        {
            double[] units = fullSquad;
            double cumulativeDamage = 0;
            double unitCount = GetTotalUnitCount();

            if (totalHealth > attacker.totalDamage)
            {
                if (damagedUnit != -1)
                {
                    if (remainingHealth <= attacker.totalDamage)
                    {
                        cumulativeDamage += remainingHealth;
                        remainingHealth = 0;
                        units[damagedUnit]--;
                        unitCount--;
                        damagedUnit = -1;
                    }
                    else
                    {
                        remainingHealth -= attacker.totalDamage;
                        cumulativeDamage = attacker.totalDamage;
                    }

                    CalculateUnitProbabilities(units, unitOrderProbabilities);
                }

                while (cumulativeDamage < attacker.totalDamage && unitCount > 0)
                {
                    int deadUnit = GetUnitByProbability(ref random);

                    cumulativeDamage += unitHealths[deadUnit];

                    if (cumulativeDamage < attacker.totalDamage)
                    {
                        units[deadUnit]--;
                        unitCount--;
                        CalculateUnitProbabilities(units, unitOrderProbabilities);
                    }
                    else
                    {
                        remainingHealth = cumulativeDamage - attacker.totalDamage;
                        damagedUnit = deadUnit;
                        cumulativeDamage = attacker.totalDamage;
                    }
                }
            }
            else
            {
                remainingHealth = 0;
                damagedUnit = -1;
                units = new double[12];
            }

            SetUnits(units);
        }

        int GetUnitByProbability(ref MudHeroRandom random)
        {
            double threshold = random.Value();
            double currentRange = 0.0;

            for (int p = 0; p < unitProbabilities.Length; p++)
            {
                if (threshold <= currentRange + unitProbabilities[p])
                {
                    return p;
                }

                currentRange += unitProbabilities[p];
            }

            return -1;
        }

        public void CalculateUnitProbabilities(float[] unitOrderProbabilities)
        {
            CalculateUnitProbabilities(fullSquad, unitOrderProbabilities);
        }

        public void CalculateUnitProbabilities(double[] units, float[] unitOrderProbabilities)
        {
            double[] relativeProbabilities = new double[units.Length];
            double[] trueProbabilities = new double[units.Length];
            double totalRelatives = 0;

            for (int r = 0; r < relativeProbabilities.Length; r++)
            {
                relativeProbabilities[r] = (float)(units[r]) * unitOrderProbabilities[r];
                totalRelatives += relativeProbabilities[r];
            }

            for (int t = 0; t < trueProbabilities.Length; t++)
            {
                trueProbabilities[t] = relativeProbabilities[t] / totalRelatives;
            }

            unitProbabilities = trueProbabilities;
        }

        public double GetTotalUnitCount()
        {
            double total = 0;
            double[] units = fullSquad;

            for (int u = 0; u < units.Length; u++)
            {
                total += units[u];
            }

            return total;
        }

        public void SetUnits(double[] squad)
        {
            riflemen = squad[0];
            machineGunners = squad[1];
            bazookamen = squad[2];

            lightTanks = squad[3];
            mediumTanks = squad[4];
            heavyTanks = squad[5];

            lightFighters = squad[6];
            mediumFighters = squad[7];
            bombers = squad[8];

            troopBunkers = squad[9];
            tankBunkers = squad[10];
            antiAircrafts = squad[11];
        }
    }
}
