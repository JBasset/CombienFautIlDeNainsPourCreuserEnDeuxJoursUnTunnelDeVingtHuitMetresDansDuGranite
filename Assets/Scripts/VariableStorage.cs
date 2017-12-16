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

        // TODO : VRAIMENT RECUPERER LA POSITION ^^
        public Vector3 beerPosition = new Vector3((float)5.778, (float)2.05, (float)-4.25630);

        public int maxValueGauge = 100;
        public int minValueGauge = 0;

        #region HEURISTICS

        // In this section, we store heuristics.


        #region Explorer
        // When is a destination considered "too close from me" to be chosen ?
        public int expl_positionTooClose = 50;
        // When is a destination considered "too close from a mine I know" chosen ?
        public int expl_positionTooKnown = 50;
        #endregion

        #region Deviant
        // Weight of the "go to beer" decision (versus the ten 1-weighted random decisions) ?
        public int dev_goToBeer = 5;
        #endregion

        #region Vigile
        // When is a dwarf considered "close enough to be my target" ?
        public int vig_closeDwarfLimit = 50;
        #endregion

        #region Supply
        // When is a mine considered "close" ?
        public int sup_closeMinefLimit = 50;
        // When is a thirsty dwarf considered "close enough to be my target" ?
        public int sup_closeDwarfLimit = 50;
        #endregion

        #region Miner
        // When is a mine considered "pretty close" ?
        public int min_closeMinefLimit = 50;
        // When is a dwarf considered "pretty close" ?
        public int min_closeDwarfLimit = 50;
        #endregion
        

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
