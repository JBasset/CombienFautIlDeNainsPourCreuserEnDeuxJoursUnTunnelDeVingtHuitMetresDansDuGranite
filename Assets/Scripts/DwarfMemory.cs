using System;
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

        public GameObject OccupiedMine;

        private ActivitiesLabel _currentActivity;
        public ActivitiesLabel CurrentActivity { get { return _currentActivity; } }
        
        private DateTime _lastActivityChange;
        public DateTime LastActivityChange {  get { return _lastActivityChange; } }

        private _Gauges Gauges;
        public int Thirst { get { return Gauges.Thirst; } }
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

            this.Gauges = new _Gauges(GameEnvironment);

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
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(Thirst,10)
            if (byValue <= 0) return;
            switch (theGauge)
            {
                case GaugesLabel.Thirst:
                    Gauges.Thirst += byValue;
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
            // exemple of use : one dwarf drinks, his thirst lowerBy(Thirst,10)
            if (byValue <= 0) return;
            switch (theGauge)
            {
                case GaugesLabel.Thirst:
                    Gauges.Thirst -= byValue;
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

        public bool UpdateMine(Vector3 thePosition, int newThirstyDwarves, int newDwarvesInTheMine, bool empty)
        {            
            // maybe this mine is already in the list
            var thisMine = _knownMines.Where(o => (o.MinePosition == thePosition)).ToList();
            if (thisMine.Any())
            {
                thisMine[0].LastInteraction = DateTime.Now;
                thisMine[0].Empty = empty;
                thisMine[0].DwarvesInTheMine = newDwarvesInTheMine;
                thisMine[0].ThirstyDwarves = (newDwarvesInTheMine < newThirstyDwarves) ? 0 : newThirstyDwarves;
            }

            // if the mine isnt already known, let's add it
            else _knownMines.Add(new _KnownMine(thePosition, newDwarvesInTheMine, newThirstyDwarves, empty, GameEnvironment));

            return true;

        }

        #endregion

        // The dwarf rethinks his activity : he may 1- change 2- keep on doing what he's doing
        public bool RethinkActivity() {
<<<<<<< HEAD

=======
>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
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

<<<<<<< HEAD
            var chanceToChangeMyActivity = 25 + (s * GameEnvironment.Variables.attenuateTimeImpact);

            switch (_currentActivity)
            {
                case ActivitiesLabel.Deviant:
                {
                    // I'm (most likely) not a deviant anymore !
                    if (Thirst > 75 || WorkDesire > 75) { chanceToChangeMyActivity += (0.5 * this.Thirst); }
                    break;
                }
                case ActivitiesLabel.Explorer:
                {
                    // NOTE THAT EXPLORATION IS REALLY IMPORTANT : AN EXPLORER'S WATCH WON'T END unless he needs beer
                    if (!KnownMines.Any()) { chanceToChangeMyActivity -= (0.5 * this.Thirst); }
                    break;
                }
                case ActivitiesLabel.Vigile: //TODO : completer
                    break;
                case ActivitiesLabel.Supply:  //TODO : completer
                    break;
                case ActivitiesLabel.Miner:  //TODO : completer
                    break;
                case ActivitiesLabel.GoToForge: //TODO : completer
                    break;
                default: break;
            }
=======
            if (rnd.Next(0, (int)chances) > 50) {
                Debug.Log("||Reth  " + this.name +
                    " 5 more minutes please.. ");
                return false; /* no changes */ }
>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7

            #endregion

            if (rnd.Next(1, 101) > chanceToChangeMyActivity) // then we don't change
            { return false; /* no changes */ }

            #region STEP THREE : I DECIDED TO CHANGE. OK. LET'S CONSIDER POSSIBILITIES

            var list = new List<_WeightedObject>();

            var w = this.WorkDesire;
            var p = this.Pickaxe;
            var t = this.Thirst;

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
                && KnownMines.Any(m => !m.Empty) && this.Pickaxe >= 10)
            {
                var w0 = (int)((w + p) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Miner, w0));
            }

            if (_currentActivity != ActivitiesLabel.Supply
                && KnownDwarves.Any(d => d.HighThirst) && this.Thirst >= 10)
            {
                var w0 = (int)((w + (100 - t)) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Supply, w0));
            }

            if (_currentActivity != ActivitiesLabel.Vigile
                && KnownDwarves.Any(d => d.Deviant) && this.Thirst >= 10)
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
<<<<<<< HEAD
            var rnd = new System.Random();
=======

>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
            Vector3 destination;
            var destList = new List<_WeightedObject>();
<<<<<<< HEAD
            int w; // a 0~100 weight

            #region STEP ONE : EXPLORING MY OPTIONS (depending on my activity)

=======

            var rnd = new System.Random();
            /*  TODO : check qu'il n'y a pas un problème, genre que les nains ne prennent pas tous la même destination */

            int w; // a 0~100 weight

>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
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
<<<<<<< HEAD
                    #region Adds a random destination (100) [ not to close for me nor too close from a known mine ]
=======
>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
                    for (var i = 0; i < 10; i++)
                    {
                        do
                        {
<<<<<<< HEAD
                            var x = rnd.Next(0, 500);var y = rnd.Next(0, 500);
                            destination = new Vector3(x, 0, y);
=======
                            var x = rnd.Next(0, 500);
                            var y = rnd.Next(0, 500);
                            destination = new Vector3(x, 0, y);
                            Debug.Log(" destination set to : " + destination);

>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
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
                    foreach (_KnownMine mine in KnownMines.FindAll(m => (!m.Empty)).ToList())
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

                        if (mine.ThirstEvaluationResult()) { w += 10; } 
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

<<<<<<< HEAD
            #endregion
            
=======
>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7
            if (!destList.Any()) /* change to another activity and reset destination */
            { RethinkActivity(); return GetNewDestination(); }

            #region STEP TWO : SELECT AN OPTION
            var destinations = new WeightedList(destList);
<<<<<<< HEAD
            var theDest = (Vector3) destinations.SelectRandomItem();
            #endregion
=======

            var theDest = (Vector3)destinations.SelectRandomItem();
>>>>>>> e678771d65faf15b473eaeb468a8fa95562b26d7

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
            public DateTime LastInteraction;
            GameEnvironment ge;

            public bool Empty;

            private int _dwarvesInTheMine;
            public int DwarvesInTheMine { get { return _dwarvesInTheMine; } set { _dwarvesInTheMine = (value > 0) ? value : 0; } }

            private int _thirstyDwarves;
            public int ThirstyDwarves { get { return _thirstyDwarves; } set { _thirstyDwarves = (value > 0) ? value : 0; } }
            
            public _KnownMine(Vector3 minePosition, int dwarvesInTheMine, int thirstyDwarves, bool empty, GameEnvironment gameEnv)
            {
                ge = gameEnv;
                this.Empty = empty;
                this.MinePosition = minePosition;
                this.LastInteraction = DateTime.Now;
                this._dwarvesInTheMine = (dwarvesInTheMine > 0) ? dwarvesInTheMine : 0;
                this._thirstyDwarves = (this._dwarvesInTheMine < thirstyDwarves) ? 0 : thirstyDwarves;
            }

            public bool ThirstEvaluationResult() { return (ThirstyDwarves >= ge.Variables.thirstyDwarvesLimit); }
        }

        public class _Gauges
        {
            private readonly int[] _gauges = new int[3];
            GameEnvironment ge;

            #region get/set (thirst, workdesire, pickaxe)
            public int Thirst
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

            public _Gauges(GameEnvironment gameEnv, int thirst = 100, int workDesire = 100, int pickaxe = 100)
            {
                ge = gameEnv;
                _gauges[0] = StockGauge(thirst);
                _gauges[1] = StockGauge(workDesire);
                _gauges[2] = StockGauge(pickaxe);
            }

            private int StockGauge(int value)
            {
                var min = ge.Variables.minValueGauge;
                var max = ge.Variables.maxValueGauge;
                
                return (value >= max)? max : ((value <= min)? min : value);
            }
        }
    }

    
}
