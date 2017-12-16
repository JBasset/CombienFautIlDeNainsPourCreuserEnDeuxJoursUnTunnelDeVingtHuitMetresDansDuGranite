using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    public class DwarfMemory:MonoBehaviour
    {
        private GameEnvironment gameEnvironment;
        
        private ActivitiesLabel _currentActivity;
        public ActivitiesLabel CurrentActivity { get { return _currentActivity; } }

        private ActivitiesLabel _previousActivity;
        public ActivitiesLabel PreviousActivity { get { return _previousActivity; } }

        private DateTime _lastActivityChange;
        public DateTime LastActivityChange {  get { return _lastActivityChange; } }

        private _Gauges Gauges;
        public int Specialisation { get { return Gauges.Specialisation; } }
        public int Tirednesss { get { return Gauges.Tiredness; } }
        public int Thirst { get { return Gauges.Thirst; } }
        public int WorkDesire { get { return Gauges.WorkDesire; } }
        public int Pickaxe { get { return Gauges.Pickaxe; } }

        int? targetDwarf; // identité de la cible si c'est un vigile

        int targetPosition; // position cible ?

        private List<_KnownDwarf> _knownDwarves = new List<_KnownDwarf>(); // nainConnus
        public List<_KnownDwarf> KnownDwarves { get { return _knownDwarves; } }

        private List<_KnownMine> _knownMines = new List<_KnownMine>();
        public List<_KnownMine> KnownMines { get { return _knownMines; } }
        /* when a dwarf meets another, or walk through a mine, 
         * he may update his knowledge of the mines 
         * using updateMine(Vector3 thePosition, bool newHighThirst) */
         
        private DwarfBehaviour dwarfBehaviour;

        

        private Transform dwarfTransf;

        void Start()
        {
            
            //_currentActivity = (ActivitiesLabel)variables.startingActivity.selectRandomItem();
            
            dwarfBehaviour = GetComponent<DwarfBehaviour>();
            dwarfTransf = GetComponent<Transform>();
            gameEnvironment = dwarfTransf.parent.parent.parent.GetComponent<GameEnvironment>();
            

            //int max = variables.maxValueGauge;
            //this.Gauges = new Gauges(max, max, max, max, max);
        }

        #region increase and lower functions ( param : VariableStorage.GaugesLabel theGauge, int byValue )
        public void IncreaseBy(GaugesLabel theGauge, int byValue) {
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Specialisation) Gauges.Specialisation += byValue;
                else if (theGauge == GaugesLabel.Tiredness) Gauges.Tiredness += byValue;
                else if (theGauge == GaugesLabel.Thirst) Gauges.Thirst += byValue;
                else if (theGauge == GaugesLabel.Workdesire) Gauges.WorkDesire += byValue;
                else if (theGauge == GaugesLabel.Pickaxe) Gauges.Pickaxe += byValue;
            }
        }

        public void LowerBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf drinks, his thirst lowerBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Specialisation) Gauges.Specialisation -= byValue;
                else if (theGauge == GaugesLabel.Tiredness) Gauges.Tiredness -= byValue;
                else if (theGauge == GaugesLabel.Thirst) Gauges.Thirst -= byValue;
                else if (theGauge == GaugesLabel.Workdesire) Gauges.WorkDesire -= byValue;
                else if (theGauge == GaugesLabel.Pickaxe) Gauges.Pickaxe -= byValue;
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
            else _knownMines.Add(new _KnownMine(thePosition, newDwarvesInTheMine, newThirstyDwarves, empty, gameEnvironment));

            return true;





        }

        #endregion

        

        public Vector3 GetNewDestination()
        {
            Vector3 destination = new Vector3();
            if (this._currentActivity == ActivitiesLabel.Deviant)
            {
                /* TODO: remplir */
            }
            else if (this._currentActivity == ActivitiesLabel.Explorer)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Miner)
            {
                List<_WeightedObject> theDest = new List<_WeightedObject>();
                int w;
                foreach (_KnownMine mine in KnownMines)
                {
                    w = 20;
                    w -= mine.DwarvesInTheMine; // the more dwarves are ALREADY in the mine, the less he wants to go
                    if (mine.Empty) { w += 20; } // at least, looks like this mine ain't empty
                    // if ( Vector3.Distance(dwarfTransf) )
                    //(mine.MinePosition)
                }
                WeightedList destinations = new WeightedList(theDest);
                destination = (Vector3)destinations.selectRandomItem();
            }
            else if (this._currentActivity == ActivitiesLabel.Supply)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Vigile)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.GoToForge)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.GoToSleep)
            { /* TODO: remplir */ }

            return destination;
        }


        public class _KnownDwarf
        {
            public Vector3 dwarfPosition;
            public int id; public bool highThirst; public DateTime lastInteraction;

            public _KnownDwarf()
            { // TODO: mettre un nain en paramètre
              // this.id = nain.id
              // this.highThirst = nain.memory. 
              // TODO: associer une memoire à chaque nain
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
            private int[] _gauges = new int[5];
            GameEnvironment ge;

            #region get/set (specialisation, tiredness, thirst, workdesire, pickaxe)
            public int Specialisation
            {
                get { return _gauges[0]; }
                set { _gauges[0] = StockGauge(value); }
            }
            public int Tiredness
            {
                get { return _gauges[1]; }
                set { _gauges[1] = StockGauge(value); }
            }
            public int Thirst
            {
                get { return _gauges[2]; }
                set { _gauges[2] = StockGauge(value); }
            }
            public int WorkDesire
            {
                get { return _gauges[3]; }
                set { _gauges[3] = StockGauge(value); }
            }
            public int Pickaxe
            {
                get { return _gauges[4]; }
                set { _gauges[4] = StockGauge(value); }
            }
            #endregion

            public _Gauges(int specialisation, int tiredness, int thirst, int workDesire, int pickaxe, GameEnvironment gameEnv)
            {
                ge = gameEnv;
                _gauges[0] = StockGauge(specialisation);
                _gauges[1] = StockGauge(tiredness);
                _gauges[2] = StockGauge(thirst);
                _gauges[3] = StockGauge(workDesire);
                _gauges[4] = StockGauge(pickaxe);
            }

            private int StockGauge(int value)
            {
                int max = ge.Variables.minValueGauge;
                int min = ge.Variables.minValueGauge;
                return (value > min) ? ((value < max) ? value : max) : min;
            }
        }
    }

    
}
