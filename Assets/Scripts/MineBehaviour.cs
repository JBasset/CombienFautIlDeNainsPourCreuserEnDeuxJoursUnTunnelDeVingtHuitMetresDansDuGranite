using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class MineBehaviour : MonoBehaviour
    {
        public GameObject GE;

        private GameEnvironment gameEnvironment;
        public int ore;
        private int LastSecond;
        public List<GameObject> dwarvesInside;

        void Start()
        {
            gameEnvironment = GE.GetComponent<GameEnvironment>();
            ore = Random.Range(0, gameEnvironment.Variables.dwarfOreMiningRate * 60); // random value between 0 and the amount of gold ore one dwarf can mine in 1 min.
            LastSecond = 0;
            dwarvesInside = new List<GameObject>();
        }

        void Update()
        {
            if (Time.time - LastSecond >= 1)
            {
                LastSecond = (int)Mathf.Floor(Time.time);
                ore += gameEnvironment.Variables.oreSpawnRate;

                int miningRate = gameEnvironment.Variables.dwarfOreMiningRate;
                foreach (GameObject Dwarf in dwarvesInside)
                {
                    if (ore >= miningRate)
                    {
                        Dwarf.GetComponent<DwarfMemory>().GoldOreMined += miningRate;
                        gameEnvironment.Variables.TotalGoldMined += miningRate;
                        ore -= miningRate;
                    }
                    else
                    {
                        Dwarf.GetComponent<DwarfMemory>().GoldOreMined += ore;
                        gameEnvironment.Variables.TotalGoldMined += ore;
                        ore = 0;
                        EmptyMine();
                        break;
                    }
                }
            }
        }

        public void AddDwarfInside(GameObject dwarf)
        {
            dwarvesInside.Add(dwarf);
        }

        public void RemoveDwarfInside(GameObject dwarf)
        {
            dwarvesInside.Remove(dwarf);
        }

        private void EmptyMine()
        {
            foreach(GameObject Dwarf in dwarvesInside)
            {
                Dwarf.GetComponent<DwarfMemory>().GetNewDestination();
            }
        }
    }
}
