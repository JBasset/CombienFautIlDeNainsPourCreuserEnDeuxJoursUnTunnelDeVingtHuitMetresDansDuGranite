﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    public class DwarfMemory:MonoBehaviour
    {
        public GameEnvironment GameEnvironment;

        public readonly int minValueGauge;
        public readonly int maxValueGauge;

        public GameObject OccupiedMine;

        private ActivitiesLabel _currentActivity;
        public ActivitiesLabel CurrentActivity { get { return _currentActivity; } }
        
        private DateTime _lastActivityChange;
        public DateTime LastActivityChange {  get { return _lastActivityChange; } }

        private _Gauges Gauges;
        public int ThirstSatisfaction { get { return Gauges.ThirstSatisfaction; } }
        public int WorkDesire { get { return Gauges.WorkDesire; } }
        public int Pickaxe { get {
            return Gauges.Pickaxe; } }

        // stats
        public int GoldOreMined;
        public int BeerDrank;
        public int BeerGiven;
        public int DeviantsStopped;
        public int TimeAsMiner;
        public int TimeAsSupply;
        public int TimeAsExplorer;
        public int TimeAsVigile;
        public int TimeAsDeviant;

        // int? targetDwarf; // identité de la cible si c'est un vigile

        // Vector3 targetPosition; // position cible ?

        private List<_KnownDwarf> _knownDwarves = new List<_KnownDwarf>(); // nainConnus
        public List<_KnownDwarf> KnownDwarves { get { return _knownDwarves; } }

        private List<_KnownMine> _knownMines = new List<_KnownMine>();
        public List<_KnownMine> KnownMines { get { return _knownMines; } }
        /* when a dwarf meets another, or walk through a mine, 
         * he may update his knowledge of the mines 
         * using updateMine(Vector3 thePosition, bool newHighThirst) */
         
        private DwarfBehaviour _dwarfBehaviour;
        
        private Transform _dwarfTransf;

        public void Start()
        {
            
            _dwarfBehaviour = GetComponent<DwarfBehaviour>();
            _dwarfTransf = GetComponent<Transform>();

            //GameEnvironment = dwarfTransf.parent.parent.parent.GetComponent<GameEnvironment>();
            
        }


        public void DwarfMemoryInitialization()
        {
            _currentActivity = (ActivitiesLabel)GameEnvironment.Variables.startingActivity.SelectRandomItem();
            _lastActivityChange = DateTime.Now;

            this.Gauges = new _Gauges(GameEnvironment.Variables.maxValueGauge, GameEnvironment.Variables.minValueGauge);

            var minValueGauge = GameEnvironment.Variables.minValueGauge;
            var maxValueGauge = GameEnvironment.Variables.maxValueGauge;

            OccupiedMine = null;

            GoldOreMined = 0;
            BeerDrank = 0;
            BeerGiven = 0;
            DeviantsStopped = 0;
            TimeAsMiner = 0;
            TimeAsSupply = 0;
            TimeAsExplorer = 0;
            TimeAsVigile = 0;
            TimeAsDeviant = 0;
    }


        #region increase and lower functions ( param : VariableStorage.GaugesLabel theGauge, int byValue )
        public void IncreaseBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(ThirstSatisfaction,10)
            if (byValue <= 0) return;
            switch (theGauge)
            {
                case GaugesLabel.ThirstSatisfaction:
                    Gauges.ThirstSatisfaction += byValue;
                    break;
                case GaugesLabel.Workdesire:
                    Gauges.WorkDesire += byValue;
                    break;
                case GaugesLabel.Pickaxe:
                    Gauges.Pickaxe += byValue;
                    break;
                default:
                    return;
            }
        }

        public void LowerBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf drinks, his thirst lowerBy(ThirstSatisfaction,10)
            if (byValue <= 0) return;
            switch (theGauge)
            {
                case GaugesLabel.ThirstSatisfaction:
                    Gauges.ThirstSatisfaction -= byValue;
                    break;
                case GaugesLabel.Workdesire:
                    Gauges.WorkDesire -= byValue;
                    break;
                case GaugesLabel.Pickaxe:
                    Gauges.Pickaxe -= byValue;
                    break;
                default:
                    return;
            }
        }
        

        public void UpdateMine(Vector3 thePosition, int newThirstyDwarves, int newDwarvesInTheMine, int ore, DateTime newDateTime)
        {            
            // maybe this mine is already in the list
            var thisMine = _knownMines.Where(
                o => (Vector3.Distance(thePosition, o.MinePosition) < 0.1f /*o.MinePosition == thePosition*/)).ToList();

            if (!thisMine.Any()) _knownMines.Add(new _KnownMine(thePosition, newDwarvesInTheMine, newThirstyDwarves, ore, newDateTime));
            // if the mine isnt already known, let's add it

            else if (thisMine[0].informatonTakenDateTime > newDateTime)
            {
                thisMine[0].informatonTakenDateTime = newDateTime;
                thisMine[0].Ore = ore;
                thisMine[0].DwarvesInTheMine = newDwarvesInTheMine;
                thisMine[0].ThirstyDwarves = (newDwarvesInTheMine < newThirstyDwarves) ? 0 : newThirstyDwarves;
            }
        
            // else our information is more recent

        }

        public void UpdateMine(_KnownMine newMine)
        {
            // maybe this mine is already in the list
            var thisMine = _knownMines.Where(
                o => (Vector3.Distance(newMine.MinePosition, o.MinePosition) < 0.1f /*o.MinePosition == mine.MinePosition*/)).ToList();

            if (!thisMine.Any()) _knownMines.Add(new _KnownMine(newMine.MinePosition, newMine.DwarvesInTheMine, newMine.ThirstyDwarves, newMine.Ore, newMine.informatonTakenDateTime));
            // if the mine isnt already known, let's add it

            else if (thisMine[0].informatonTakenDateTime > newMine.informatonTakenDateTime)
            {
                thisMine[0].informatonTakenDateTime = newMine.informatonTakenDateTime;
                thisMine[0].Ore = newMine.Ore;
                thisMine[0].DwarvesInTheMine = newMine.DwarvesInTheMine;
                thisMine[0].ThirstyDwarves = (newMine.DwarvesInTheMine < newMine.ThirstyDwarves) ? 0 : newMine.ThirstyDwarves;
            }

            // else our information is more recent

        }

        #endregion

        // The dwarf rethinks his activity : he may 1- change 2- keep on doing what he's doing
        public bool RethinkActivity() {
            var rnd = new System.Random();

            #region STEP ONE : CHECK TIME

            var s = (int)((DateTime.Now - _lastActivityChange).TotalSeconds); // time since last change (in sec)
            if (s < GameEnvironment.Variables.lowerBoundBeforeRethink) { return false; /* no changes */ }

            #endregion

            #region STEP TWO : DO I CHANGE MY ACTIVITY ? (probability to change calculation)

            /* we consider that the dwarf has a 25 chances out of 100 of changing his activity,
             * this probability is increased as time passes.
             * 
             * Fact is : the more he "rethinks", the more the risk that he changes his activity increases
             */
            var chanceToChangeMyActivity = 25 + (s * GameEnvironment.Variables.attenuateTimeImpact);

            if (_currentActivity != ActivitiesLabel.Deviant)
            { chanceToChangeMyActivity += (0.5 * (100 - this.ThirstSatisfaction)); }

            switch (_currentActivity)
            {
                case ActivitiesLabel.Deviant:
                {
                    // I'm (most likely) not a deviant anymore !
                    if (ThirstSatisfaction > 75 || WorkDesire > 75) { chanceToChangeMyActivity += (0.5 * this.ThirstSatisfaction); }
                    
                    // I may stay a deviant for a while
                    if (ThirstSatisfaction < 25 || WorkDesire < 25) { chanceToChangeMyActivity -= (0.5 * ( 100 - this.ThirstSatisfaction) ); }
                    break;
                    }
                case ActivitiesLabel.Explorer:
                {
                    // NOTE THAT EXPLORATION IS REALLY IMPORTANT : AN EXPLORER'S WATCH WON'T END unless he needs beer
                    if (!KnownMines.Any()) { chanceToChangeMyActivity -= (0.5 * this.ThirstSatisfaction); }
                    break;
                }
                case ActivitiesLabel.Vigile: //TODO : completer
                    break;
                case ActivitiesLabel.Supply:  //TODO : completer
                    break;
                case ActivitiesLabel.Miner:  //TODO : completer : je suis actuellement un mineur, je compte le rester un moment ! C'est mon objectif dans la vie quand même.
                    break;
                case ActivitiesLabel.GoToForge: //TODO : completer
                    break;
                default: break;
            }

            #endregion

            if (rnd.Next(1, 101) > chanceToChangeMyActivity) // then we don't change
            { return false; /* no changes */ }

            #region STEP THREE : I DECIDED TO CHANGE. OK. LET'S CONSIDER POSSIBILITIES

            var list = new List<_WeightedObject>();

            var w = this.WorkDesire;
            var p = this.Pickaxe;
            var t = this.ThirstSatisfaction;

            if (_currentActivity != ActivitiesLabel.GoToForge
                && this.Pickaxe <= GameEnvironment.Variables.pickaxeLimit)
            {
                var w0 = (int)((w + 2 * (100 - p)) / 3);
                list.Add(new _WeightedObject(ActivitiesLabel.GoToForge, w0));
            }

            if (_currentActivity != ActivitiesLabel.Deviant)
            {
                var w0 = (int)(((100 - w) + t) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Deviant, w0));
            }

            if (_currentActivity != ActivitiesLabel.Explorer)
            {
                var w0 = (KnownMines.Any()) ? (int)((w + (100 - p)) / 2) : (int)((w + 100) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Explorer, w0));
            }

            if (_currentActivity != ActivitiesLabel.Miner
                && KnownMines.Any(m => m.Ore >= 5) && this.Pickaxe >= 10)
            {
                var w0 = (int)((w + p) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Miner, w0));
            }

            if (_currentActivity != ActivitiesLabel.Supply
                && KnownDwarves.Any(d => d.HighThirst) && this.ThirstSatisfaction >= 10)
            {
                var w0 = (int)((w + (100 - t)) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Supply, w0));
            }

            if (_currentActivity != ActivitiesLabel.Vigile
                && KnownDwarves.Any(d => d.Deviant) && this.ThirstSatisfaction >= 10)
            {
                var w0 = w;
                list.Add(new _WeightedObject(ActivitiesLabel.Vigile, w0));
            }

            #endregion

            #region STEP FOUR : TIME TO MAKE A DECISION

            var startingActivity = new WeightedList(list);
            var newActivity = (ActivitiesLabel)startingActivity.SelectRandomItem();
            // Debug.Log("Hey " + this.name +" just changed his activity from " +_currentActivity +" to " +newActivity);
            _currentActivity = newActivity;

            #endregion
            
            return true;
        }

        public Vector3 GetNewDestination()
        {

            var rnd = new System.Random();

            Vector3 destination;
            var destList = new List<_WeightedObject>();

            int w; // a 0~100 weight
            
            #region STEP ONE : EXPLORING MY OPTIONS (depending on my activity)

            switch (this._currentActivity)
            {
                case ActivitiesLabel.Deviant:
                    #region Adds 10 random destination (10 each), plus the Beer position (Variables.dev_goToBeer)
                    for (var i = 0; i < 10; i++)
                    {
                        do{destination = new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500));}
                        while (
                            (Vector3.Distance(_dwarfTransf.position, destination) < GameEnvironment.Variables.expl_positionTooClose)
                            && KnownMines.All(
                                mine => (Vector3.Distance(mine.MinePosition, destination) < GameEnvironment.Variables.expl_positionTooKnown)
                            )
                        );
                        destList.Add(new _WeightedObject(destination, 10));
                    }
                    destList.Add(new _WeightedObject(GameEnvironment.Variables.beerPosition, GameEnvironment.Variables.dev_goToBeer));
                    break;
                    #endregion
                case ActivitiesLabel.Explorer:
                    #region Adds a random destination (100) [ not to close for me nor too close from a known mine ]

                    for (var i = 0; i < 10; i++)
                    {
                        do
                        {

                            var x = rnd.Next(0, 500);var y = rnd.Next(0, 500);
                            destination = new Vector3(x, 0, y);
                            Debug.Log(" destination set to : " + destination);
                            
                        } while (
                            (Vector3.Distance(_dwarfTransf.position, destination) < GameEnvironment.Variables.expl_positionTooClose)
                            || KnownMines.Any(
                                mine => (Vector3.Distance(mine.MinePosition, destination) < GameEnvironment.Variables.expl_positionTooKnown)
                            )
                        );
                        destList.Add(new _WeightedObject(destination, 1));
                    }
                    break;
                    #endregion
                case ActivitiesLabel.Miner:
                    #region Adds every non-empty mines (0-100 depending on distance and dwarf number)
                    foreach (_KnownMine mine in KnownMines.FindAll(m => (m.Ore > 5)).ToList())
                    {
                        w = 80 - (int)(mine.DwarvesInTheMine * GameEnvironment.Variables.min_pplInTheMineImportance);
                        // the more dwarves are ALREADY in the mine, the less he wants to go

                        if (Vector3.Distance(_dwarfTransf.position, mine.MinePosition) < GameEnvironment.Variables.min_closeMinefLimit)
                        { w += 20; } // this mine is close enough

                        if (w > 0) destList.Add(new _WeightedObject(mine.MinePosition, w));
                    }
                    break;
                    #endregion
                case ActivitiesLabel.Supply:
                    #region Adds every occuped mines (dwarf number + 0-20 depending on distance and thirst evaluation)
                    foreach (var mine in KnownMines.FindAll(m => m.DwarvesInTheMine > 0).ToList()) // we add a mine if not empty
                    {
                        var mPosition = mine.MinePosition;
                        w = (int)(mine.DwarvesInTheMine); 
                        // the more dwarves in the mine, the more he wants to go

                        if (Vector3.Distance(_dwarfTransf.position, mPosition) < GameEnvironment.Variables.sup_closeMinefLimit)
                        { w += 10; } 
                        // this mine is close enough

                        w += 2 * mine.ThirstyDwarves;
                        // I know they want to drink in this mine

                        destList.Add(new _WeightedObject(mPosition, w));
                    }
                    #endregion
                    #region Adds known thirsty dwarves (50 or 10 depending on distance)
                    foreach (var dwarf in KnownDwarves.FindAll(d => (d.HighThirst)).ToList()) 
                        // we add thirsty dwarves (weight depending on how close he is)
                    {
                        var dPosition = dwarf.DwarfPosition;
                        w = (Vector3.Distance(_dwarfTransf.position, dPosition) < GameEnvironment.Variables.sup_closeDwarfLimit) ? 50 : 10;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                    break;
                    #endregion
                case ActivitiesLabel.Vigile:
                    #region Adds deviant known dwarves (50 or 10 depending on distance)
                    foreach (var dwarf in KnownDwarves.FindAll(d => (d.Deviant)).ToList())
                    {
                        var dPosition = dwarf.DwarfPosition;
                        w = (Vector3.Distance(_dwarfTransf.position, dPosition) < GameEnvironment.Variables.sup_closeDwarfLimit) ? 50 : 10;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                    #endregion
                    if (destList.Any()) { break; }
                    #region (IF no target) Adds all thirsy dwarves (10 or 50 depending on distance)
                    foreach (var dwarf in KnownDwarves.FindAll(d => (d.HighThirst)).ToList())
                        {
                            var dPosition = dwarf.DwarfPosition;
                            w = (Vector3.Distance(_dwarfTransf.position, dPosition) <
                                 GameEnvironment.Variables.sup_closeDwarfLimit)
                                ? 50
                                : 10;
                            destList.Add(new _WeightedObject(dPosition, w));
                        }
                    break;
                    #endregion
                case ActivitiesLabel.GoToForge:
                    #region Adds forge (not questionable)
                    if (this.Pickaxe <= GameEnvironment.Variables.pickaxeLimit)
                    { return (Vector3)GameEnvironment.Variables.forgePosition; }
                    break;
                    #endregion
                default:
                    { RethinkActivity(); return GetNewDestination(); /*iteration until a destination is chosen*/ }
            }


            #endregion
     
            if (!destList.Any()) /* change to another activity and reset destination */
            { RethinkActivity(); return GetNewDestination(); }

            #region STEP TWO : SELECT AN OPTION
            var destinations = new WeightedList(destList);

            var theDest = (Vector3) destinations.SelectRandomItem();
            #endregion
            
            return theDest;
        }

        public class _KnownDwarf
        {
            public Vector3 DwarfPosition;
            public int id; public bool HighThirst; public DateTime LastInteraction;
            public bool Deviant;

            public _KnownDwarf()
            { // TODO: mettre un nain en paramètre
              // this.id = nain.id
              // this.highThirst = dwarf.highThirst
              // this.deviant = dwarf.currentActivity == deviant
            this.LastInteraction = DateTime.Now;
            }
        }

        public class _KnownMine
        {
            public Vector3 MinePosition;
            public DateTime informatonTakenDateTime;

            public int Ore;

            private int _dwarvesInTheMine;
            public int DwarvesInTheMine { get { return _dwarvesInTheMine; } set { _dwarvesInTheMine = (value > 0) ? value : 0; } }

            private int _thirstyDwarves;
            public int ThirstyDwarves { get { return _thirstyDwarves; } set { _thirstyDwarves = (value > 0) ? value : 0; } }
            // number of dwarves under thirstyDwarvesGaugeLimit

            public _KnownMine(Vector3 minePosition, int dwarvesInTheMine, int thirstyDwarves, int ore, DateTime newDateTime)
            {
                this.Ore = ore;
                this.MinePosition = minePosition;
                this.informatonTakenDateTime = newDateTime;
                this._dwarvesInTheMine = (dwarvesInTheMine > 0) ? dwarvesInTheMine : 0;
                this._thirstyDwarves = (this._dwarvesInTheMine < thirstyDwarves) ? 0 : thirstyDwarves;
            }
            
        }

        public class _Gauges
        {
            private readonly int[] _gauges = new int[3];
            GameEnvironment ge;
            private int max;
            private int min;

            #region get/set (thirst, workdesire, pickaxe)
            public int ThirstSatisfaction
            {
                get { return _gauges[0]; }
                set { _gauges[0] = StockGauge(value); }
            }
            public int WorkDesire
            {
                get { return _gauges[1]; }
                set { _gauges[1] = StockGauge(value); }
            }
            public int Pickaxe
            {
                get {
                    return _gauges[2]; }
                set { _gauges[2] = StockGauge(value); }
            }
            #endregion

            public _Gauges(int maxValueGauge, int minValueGauge, int thirst = 100, int workDesire = 100, int pickaxe = 100)
            {
                max = maxValueGauge;
                min = minValueGauge;
                _gauges[0] = StockGauge(thirst);
                _gauges[1] = StockGauge(workDesire);
                _gauges[2] = StockGauge(pickaxe);
            }

            private int StockGauge(int value)
            {
                return (value >= max) ? max : ((value <= min) ? min : value);
            }
        }
    }

    
}
