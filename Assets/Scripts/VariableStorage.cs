using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        // VARIABLES NON STATIQUES
        public List<GameObject> NoticeableObjects; // objects dwarves can see. used to test dwarves line of sight
        public List<GameObject> Dwarves; // list of all living dwarves in the game
        public List<GameObject> Mines; // list of the mines in the World



        public static int maxValueGauge = 100;
        public static int minValueGauge = 0;

        #region HEURISTICS
        
        public int thirstyDwarvesLimit = 3; // if there is over (>=) than 3 thirsty dwarves in a mine, then the "ThirstEvaluation" results with a true
        
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
