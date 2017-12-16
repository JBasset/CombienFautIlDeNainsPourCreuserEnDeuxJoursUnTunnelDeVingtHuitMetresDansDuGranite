using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        public List<GameObject> NoticeableObjects; // objects dwarves can see. used to test dwarves line of sight
        public List<GameObject> Dwarves; // list of all living dwarves in the game
        public List<GameObject> Mines; // list of the mines in the World

        public float MapXMax = 500;
        public float MapZMax = 500;

        public int maxValueGauge = 100;
        public int minValueGauge = 0;

        #region HEURISTICS

        // In this section, we store heuristics.
        
        // When is a mine considered "close" ?
        public int closeMinefLimit = 50;
        // When is a dwarf considered "close" ?
        public int closeDwarfLimit = 50;

        // From how many thirsty dwarves should we worry ? ( number of dwarves from which "ThirstEvaluation" results with a true)
        public int thirstyDwarvesLimit = 3; 

        
        #endregion

        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner, GoToForge, GoToSleep }
        public enum GaugesLabel { Specialisation, Tiredness, Thirst, Workdesire, Pickaxe}

        

        // new dwarves start with those activities :
        public WeightedList startingActivity = new WeightedList(
            new List<_WeightedObject>{
                new _WeightedObject(ActivitiesLabel.Explorer, 1),
                new _WeightedObject(ActivitiesLabel.Miner, 0) // currently zero chance to be a miner :(
            } 
        );
    }
}
