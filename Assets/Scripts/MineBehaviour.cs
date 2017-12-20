using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class MineBehaviour : MonoBehaviour
    {
        public GameObject GE;

        public int OreExtracted;
        public int TimesInteracted;

        private GameEnvironment _gameEnvironment;
        public int Ore;
        private int _lastSecond;
        private int _emptySince;
        public List<GameObject> DwarvesInside;

        void Start()
        {
            _gameEnvironment = GE.GetComponent<GameEnvironment>();
            Ore = _gameEnvironment.Variables.dwarfOreMiningRate * 120; // the amount of gold a dwarf alone can mine in 2 minutes 
            _lastSecond = 0;
            _emptySince = -1;
            DwarvesInside = new List<GameObject>();
            OreExtracted = 0;
            TimesInteracted = 0;
        }

        void Update()
        {
            if (_emptySince != -1 && Time.time - _emptySince >= 120) // 2 minutes after the mine was emptied
            {
                Ore = _gameEnvironment.Variables.dwarfOreMiningRate * 120; // the amount of gold a dwarf alone can mine in 2 minutes
                _emptySince = -1;
                /* This simulation takes place in a world where gold "appears" spontaneously in mines. 
                * We neglect the economical impact of that property in the simulation's universe. 
                * Even though it's worthless, dwarves love gold. */
            }

            if (Time.time - _lastSecond >= 1)
            {
                _lastSecond = (int)Mathf.Floor(Time.time);

                foreach (var d in DwarvesInside)
                {
                    d.GetComponent<DwarfMemory>().TimeAsMiner++;
                }

                var miningRate = _gameEnvironment.Variables.dwarfOreMiningRate;
                foreach (var Dwarf in DwarvesInside)
                {
                    if (Ore >= miningRate)
                    {
                        Dwarf.GetComponent<DwarfMemory>().GoldOreMined += miningRate;
                        _gameEnvironment.Variables.TotalGoldMined += miningRate;
                        OreExtracted += miningRate;
                        Ore -= miningRate;
                    }
                    else
                    {
                        Dwarf.GetComponent<DwarfMemory>().GoldOreMined += Ore;
                        _gameEnvironment.Variables.TotalGoldMined += Ore;
                        OreExtracted += Ore;
                        Ore = 0;
                        if (_emptySince == -1) _emptySince = (int)Mathf.Floor(Time.time);
                    }
                }
            }
        }

        public void AddDwarfInside(GameObject dwarf)
        {
            DwarvesInside.Add(dwarf);
        }

        public void RemoveDwarfInside(GameObject dwarf)
        {
            DwarvesInside.Remove(dwarf);
        }
    }
}
