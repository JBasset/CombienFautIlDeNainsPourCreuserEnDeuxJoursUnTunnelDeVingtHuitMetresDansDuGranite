using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        // TODO : attention, on a enlevé gotosleep donc faut pas en parler dans le rapport

        public List<GameObject> NoticeableObjects; // objects dwarves can see. used to test dwarves line of sight
        public List<GameObject> Dwarves; // list of all living dwarves in the game
        public List<GameObject> Mines; // list of the mines in the World

        public float MapXMax = 500;
        public float MapZMax = 500;

        // TODO : VRAIMENT RECUPERER LA POSITION ^^
        // TODO : JEAN HALP LA VRAIE POSITION ELLEESTOU ? :sob:

        public Vector3 beerPosition = new Vector3((float)5.778, (float)2.05, (float)-4.25630);
        public Vector3 forgePosition = new Vector3((float)55.25, (float)1.9, (float)25.20);

        public int maxValueGauge = 100;
        public int minValueGauge = 0;

        public int oreSpawnRate = 1; // This simulation takes place in a world where gold "appears" continuously in mines. We neglect the economical impact of that property in the simulation's universe. Even if worthless, dwarves love gold.
        public int dwarfOreMiningRate = 10;

        public double attenuateTimeImpact = 1; /* this value (0 <= x <= 1) affects a dwarf's chance to rethink his activity */
        /* proba = 0.25 + (attenuateTimeImpact * nbsec / 100 ) */

        public int activityRethinkChangeRate = 10; // in UnityEngine.Time, used in gameEnvironment.Update()
        public int lowerBoundBeforeRethink = 3; // in seconds, used in DwarfMemory.RethinkActivity()

        #region HEURISTICS

        // In this section, we store heuristics.

        // From how many thirsty dwarves should we worry ? ( number of dwarves from which "ThirstEvaluation" results with a true)
        public int thirstyDwarvesLimit = 3;

        // When do you think a pickaxe is deteriored enough to consider going to the forge ?
        public int pickaxeLimit = 20;

        #region Explorer
        // When is a destination considered "too close from me" to be chosen ?
        public int expl_positionTooClose = 70;
        // When is a destination considered "too close from a mine I know" chosen ?
        public int expl_positionTooKnown = 50;
        #endregion

        #region Deviant
        // Weight of the "go to beer" decision (versus the ten 1-weighted random decisions) ?
        public int dev_goToBeer = 50; // considering that the list contains 10* 10-weighted random destinations
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

        /* importance given of the number of dwarves already in the mine
         * this value affects a dwarf's chance to go in a densely populated mine 
         * 0 : no incidence || >0 : the dwarf may prefer a less populated mine
         */
        public double min_pplInTheMineImportance = 1.5; 
        #endregion




        #endregion

        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner, GoToForge }
        public enum GaugesLabel { /*Specialisation, Tiredness, */ Thirst, Workdesire, Pickaxe}

        

        // new dwarves start with those activities :
        public WeightedList startingActivity = new WeightedList(
            new List<_WeightedObject>{
                new _WeightedObject(ActivitiesLabel.Explorer, 1),
                new _WeightedObject(ActivitiesLabel.Miner, 0) // currently zero chance to be a miner :(
            } 
        );
    }
}
