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
            Debug.Log(
                "Hey, I'm in " + this.name + " DwarfMemoryInitialization ! ");
            _currentActivity = (ActivitiesLabel)GameEnvironment.Variables.startingActivity.SelectRandomItem();
            _lastActivityChange = DateTime.Now;

            this.Gauges = new _Gauges(GameEnvironment);
            Debug.Log( this.name + " initialization done, Pickaxe is : " + this.Pickaxe);

            OccupiedMine = null;
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
            Debug.Log("||Reth  " +
                this.name + " entering rethink activity");
            var rnd = new System.Random();
            /*  TODO : check qu'il n'y a pas un problème, genre que les nains ne prennent pas tous le 2nd choix quand rnd = 2 */
            
            var s = (int)((DateTime.Now - _lastActivityChange).TotalSeconds); // time since last change (in sec)

            if (s < GameEnvironment.Variables.lowerBoundBeforeRethink) { return false; /* no changes */ }

            /* we consider that the dwarf has a one chance out of four of changing activity,
             * this probability is increased as time passes.
             * thus, after 60 sec, the dwarf has a 0,3571 proba to change
             * after 120 sec, he has a 0,625 proba to change
             * 
             * Fact is : the more he "rethinks", the more the risk that he changes his activity increases
             */
            var chances = 200 - ( s * GameEnvironment.Variables.attenuateTimeImpact );

            Debug.Log("||Reth  " + this.name +
                " chances = " + chances);

            if (rnd.Next(0, (int)chances) > 50) {
                Debug.Log("||Reth  " + this.name +
                    " 5 more minutes please.. ");
                return false; /* no changes */ }


            Debug.Log("||Reth  " + this.name +
                " OK let's change ");

            #region let's change his activity

            var list = new List<_WeightedObject>();

            var w = this.WorkDesire;
            var p = this.Pickaxe;
            var t = this.Thirst;
            
            if (this.Pickaxe <= GameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.GoToForge)
            { list.Add(new _WeightedObject(ActivitiesLabel.GoToForge,
                (int)((w + 2*(100 - p)) / 3)));

                Debug.Log("||Reth  " + this.name +
                          "  added GoToForge, w = " + ((w + 2 * (100 - p)) / 3));

            }

            if (_currentActivity != ActivitiesLabel.Deviant)
            { list.Add(new _WeightedObject(ActivitiesLabel.Deviant, 
                (int)(((100 - w) + t) / 2)));

                Debug.Log("||Reth  " + this.name +
                          "  added Deviant, w = " + (((100 - w) + t) / 2));
            }

            if (_currentActivity != ActivitiesLabel.Explorer)
            { list.Add(new _WeightedObject(ActivitiesLabel.Explorer,
                (int)((w + (100 - p)) / 2)));

                Debug.Log("||Reth  " + this.name +
                          "  added Explorer, w = " + ((w + (100 - p)) / 2));
            }

            if (_currentActivity != ActivitiesLabel.Miner 
                && KnownMines.Any(m => !m.Empty))
            { list.Add(new _WeightedObject(ActivitiesLabel.Miner,
                (int)((w + p) / 2)));
            }

            if (_currentActivity != ActivitiesLabel.Supply
                && KnownDwarves.Any(d => d.highThirst))
            { list.Add(new _WeightedObject(ActivitiesLabel.Supply,
                (int)((w + (100 - t)) / 2)));
            }

            if (_currentActivity != ActivitiesLabel.Vigile
                && KnownDwarves.Any(d => d.deviant))
            {
                list.Add(new _WeightedObject(ActivitiesLabel.Vigile,
                  (int)(w)));
            }

            Debug.Log("||Reth  " + this.name +
                " list.Count = " + list.Count);

            #endregion

            var startingActivity = new WeightedList(list);
            var newActivity = (ActivitiesLabel)startingActivity.SelectRandomItem();
            Debug.Log("||Reth  " +
                "Hey " + this.name +" just changed his activity from " +_currentActivity +" to " +newActivity);
            
            _currentActivity = newActivity;


            return true;
        }

        public Vector3 GetNewDestination()
        {
            Debug.Log("###  " +
                      this.name + " entering GetNewDestination");

            Vector3 destination;

            var destList = new List<_WeightedObject>();

            Debug.Log(" ** creating some random shit");

            var rnd = new System.Random();
            /*  TODO : check qu'il n'y a pas un problème, genre que les nains ne prennent pas tous la même destination */

            int w; // a 0~100 weight

            Debug.Log("###  " +
                      this._currentActivity + " (_currentActivity)");

            switch (this._currentActivity)
            {
                case ActivitiesLabel.Deviant:
                    for (var i = 0; i < 10; i++)
                    {
                        do
                        {
                            destination = new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500));
                        } while (
                            (Vector3.Distance(_dwarfTransf.position, destination) < GameEnvironment.Variables.expl_positionTooClose)
                            && KnownMines.All(
                                mine => (Vector3.Distance(mine.MinePosition, destination) < GameEnvironment.Variables.expl_positionTooKnown)
                            )
                        );
                        destList.Add(new _WeightedObject(destination, 10));
                    }
                    destList.Add(new _WeightedObject(GameEnvironment.Variables.beerPosition, GameEnvironment.Variables.dev_goToBeer));
                    break;
                case ActivitiesLabel.Explorer:
                    Debug.Log(" YAY I'M AN EXPLOROROR");
                    for (var i = 0; i < 10; i++)
                    {
                        do
                        {
                            Debug.Log(" CAN DOOOO ");
                            var x = rnd.Next(0, 500);
                            var y = rnd.Next(0, 500);
                            destination = new Vector3(x, 0, y);
                            Debug.Log(" destination set to : " + destination);
                            Debug.Log(" quoi dans le fuck ? ");
                            Debug.Log(" _dwarfTransf.position " + _dwarfTransf.position);
                            Debug.Log(" destination " + destination);
                            Debug.Log(" Vector3.Distance(_dwarfTransf.position, destination) : " + Vector3.Distance(_dwarfTransf.position, destination));
                            Debug.Log(" GameEnvironment.Variables.expl_positionTooClose " + GameEnvironment.Variables.expl_positionTooClose);
                            Debug.Log(" KnownMines.Any( ... " + KnownMines.Any(
                                          mine => (Vector3.Distance(mine.MinePosition, destination) <
                                                   GameEnvironment.Variables.expl_positionTooKnown)));
                            Debug.Log(" quoi dans le fuck ? ");

                        } while (
                            (Vector3.Distance(_dwarfTransf.position, destination) < GameEnvironment.Variables.expl_positionTooClose)
                            || KnownMines.Any(
                                mine => (Vector3.Distance(mine.MinePosition, destination) < GameEnvironment.Variables.expl_positionTooKnown)
                            )
                        );
                        destList.Add(new _WeightedObject(destination, 1));
                    }
                    break;
                case ActivitiesLabel.Miner:
                    foreach (_KnownMine mine in KnownMines.FindAll(m => (!m.Empty)).ToList())
                    {
                        w = 80 - (int)(mine.DwarvesInTheMine * GameEnvironment.Variables.min_pplInTheMineImportance); 
                        // the more dwarves are ALREADY in the mine, the less he wants to go
                    
                        if (Vector3.Distance(_dwarfTransf.position, mine.MinePosition) < GameEnvironment.Variables.min_closeMinefLimit)
                        { w+=20; } // this mine is close enough

                        if (w > 0) destList.Add(new _WeightedObject(mine.MinePosition, w));
                    }
                    break;
                case ActivitiesLabel.Supply:
                    foreach (_KnownMine mine in KnownMines.FindAll(m => (!m.Empty)).ToList()) // we add a mine if not empty
                    {
                        var mPosition = mine.MinePosition;

                        w = 10 + (int)(mine.DwarvesInTheMine);
                        // the more dwarves in the mine, the more he wants to go

                        if (Vector3.Distance(_dwarfTransf.position, mPosition) < GameEnvironment.Variables.sup_closeMinefLimit)
                        { w+=10; } // this mine is close enough

                        if (mine.ThirstEvaluationResult()) { w += 10; } // I know they want to drink in this mine

                        destList.Add(new _WeightedObject(mPosition, w));
                    }
                    foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.highThirst)).ToList()) // we add thirsty dwarves (weight depending on how close he is)
                    {
                        var dPosition = dwarf.dwarfPosition;
                        w = (Vector3.Distance(_dwarfTransf.position, dPosition) < GameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                    break;
                case ActivitiesLabel.Vigile:
                    foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.deviant)).ToList())
                        // we add deviant dwarves (weight depending on how close he is)
                    {
                        var dPosition = dwarf.dwarfPosition;
                        w = (Vector3.Distance(_dwarfTransf.position, dPosition) < GameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                
                    // if the vigile has no possible target, he goes to a (close?) thirsy dwarf
                    if (!destList.Any()) {
                        foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.highThirst)).ToList())
                        {
                            var dPosition = dwarf.dwarfPosition;
                            w = (Vector3.Distance(_dwarfTransf.position, dPosition) < GameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                            destList.Add(new _WeightedObject(dPosition, w));
                        }
                    };
                    break;
                case ActivitiesLabel.GoToForge:
                    if (this.Pickaxe <= GameEnvironment.Variables.pickaxeLimit)
                    {
                        return (Vector3)GameEnvironment.Variables.forgePosition;
                    }
                    break;
                default:
                    RethinkActivity(); return GetNewDestination();
            }

            Debug.Log("###  exit switchcase");

            if (!destList.Any()) /* change to another activity and reset destination */
            { RethinkActivity(); return GetNewDestination(); }

            var destinations = new WeightedList(destList);

            Debug.Log(
                this.name + " destList count : " + destList.Count);

            var theDest = (Vector3)destinations.SelectRandomItem();

            Debug.Log(
                this.name + "dest " + theDest);

            return theDest;
        }

        public class _KnownDwarf
        {
            public Vector3 dwarfPosition;
            public int id; public bool highThirst; public DateTime lastInteraction;
            public bool deviant;

            public _KnownDwarf()
            { // TODO: mettre un nain en paramètre
              // this.id = nain.id
              // this.highThirst = dwarf.highThirst
              // this.deviant = dwarf.currentActivity == deviant
            this.lastInteraction = DateTime.Now;
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
                Debug.Log(
                    "Hey, I'm in _Gauges constructor trying to set pickaxe to" + pickaxe + " :) ");
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
