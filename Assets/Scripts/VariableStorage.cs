using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner, GoToForge }
        public enum GaugesLabel { /*Specialisation, Tiredness, */ ThirstSatisfaction, Workdesire, Pickaxe }
        
        // new dwarves start with those activities :
        public WeightedList startingActivity = new WeightedList(
            new List<_WeightedObject>{
                new _WeightedObject(ActivitiesLabel.Explorer, 1),
                new _WeightedObject(ActivitiesLabel.Miner, 0) // currently zero chance to be a miner :(
            }
        );

        #region Objects in the game environment
        public List<GameObject> NoticeableObjects; // objects dwarves can see. used to test dwarves line of sight
        public List<GameObject> Dwarves; // list of all living dwarves in the game
        public List<GameObject> Mines; // list of the mines in the World
        #endregion

        #region global informations (positions, sizes)
        public float MapXMax = 500;
        public float MapZMax = 500;

        public Vector3 beerPosition = new Vector3((float)203.27, (float)1.13, (float)246.37);
        public Vector3 forgePosition = new Vector3((float)194.75, (float)0.9, (float)275.2);
        public Vector3 dwarvesSpawn = new Vector3(212, 1.2f, 250); // center of the village

        public int maxValueGauge = 100;
        public int minValueGauge = 0;
        #endregion

        # region general stats
        public int TimeSinceStart = 0;
        public int TotalGoldMined = 0;
        public int TotalBeerDrank = 0;
        #endregion

        #region UPDATE CYCLE ( mining rate / ore spawn rate / rethink rate / gauge update rate etc )
        public int oreSpawnRate = 1;
        /* This simulation takes place in a world where gold "appears" continuously in mines. 
         * We neglect the economical impact of that property in the simulation's universe. 
         * Even though it's worthless, dwarves love gold. */

        public int dwarfOreMiningRate = 10;

        public int activityRethinkChangeRate = 10; // in UnityEngine.Time, used in gameEnvironment.Update()
        public int gaugeUpdateRate = 2; // in UnityEngine.Time, used in gameEnvironment.Update()
        #endregion

        public Heuristics H = new Heuristics();

        #region HEURISTICS

        public class Heuristics // In this section, we store heuristics.
        {
            // distance the dwarves can see
            public int SightDistance = 50;


            public double attenuateTimeImpact = 1; /* this value (0 <= x <= 1) affects a dwarf's chance to rethink his activity */
            /* proba = 0.25 + (attenuateTimeImpact * nbsec / 100 ) */

            // From how many thirsty dwarves should we worry ? ( number of dwarves from which "ThirstEvaluation" results with a true)
            public int thirstyDwarvesGaugeLimit = 60;

            // When do you think a pickaxe is deteriored enough to consider going to the forge ?
            public int pickaxeLimit = 20;

            #region Explorer
            // When is a destination considered "too close from me" to be chosen ?
            public int expl_positionTooClose = 40;
            // When is a destination considered "too close from a mine I know" chosen ?
            public int expl_positionTooKnown = 40;
            // When do an explorer consider that he has enough information to stop beeing an explorer ?
            public int expl_iknwoenough = 3;
            #endregion

            #region Deviant
            // Weight of the "go to beer" decision (versus the ten 1-weighted random decisions) ?

            // considering that the list contains 10* 10-weighted random destinations... weight of beer-destination ?
            public int dev_goToBeer = 50;

            // When is a destination considered "too close from me" to be chosen ?
            public int dev_positionTooClose = 80;
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

        }
        
        #endregion
        
    }
}
