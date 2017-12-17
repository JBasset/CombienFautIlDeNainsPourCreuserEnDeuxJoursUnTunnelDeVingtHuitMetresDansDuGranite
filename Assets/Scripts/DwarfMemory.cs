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
        public GameEnvironment gameEnvironment;
        
        private ActivitiesLabel _currentActivity;
        public ActivitiesLabel CurrentActivity { get { return _currentActivity; } }

        private DateTime _lastActivityChange;
        public DateTime LastActivityChange {  get { return _lastActivityChange; } }

        private _Gauges Gauges;
        public int Thirst { get { return Gauges.Thirst; } }
        public int WorkDesire { get { return Gauges.WorkDesire; } }
        public int Pickaxe { get { return Gauges.Pickaxe; } }

        int? targetDwarf; // identité de la cible si c'est un vigile

        Vector3 targetPosition; // position cible ?

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

            //gameEnvironment = dwarfTransf.parent.parent.parent.GetComponent<GameEnvironment>();
            

            //int max = variables.maxValueGauge;
            //this.Gauges = new Gauges(max, max, max, max, max);
        }

        #region increase and lower functions ( param : VariableStorage.GaugesLabel theGauge, int byValue )
        public void IncreaseBy(GaugesLabel theGauge, int byValue) {
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Thirst) Gauges.Thirst += byValue;
                else if (theGauge == GaugesLabel.Workdesire) Gauges.WorkDesire += byValue;
                else if (theGauge == GaugesLabel.Pickaxe) Gauges.Pickaxe += byValue;
            }
        }

        public void LowerBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf drinks, his thirst lowerBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Thirst) Gauges.Thirst -= byValue;
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

        // The dwarf rethinks his activity : he may 1- change 2- keep on doing what he's doing
        public void RethinkActivity() {
            System.Random rnd = new System.Random();
            /*  TODO : check qu'il n'y a pas un problème, genre que les nains ne prennent pas tous le 2nd choix quand rnd = 2 */
            
            var s = (int)((DateTime.Now - _lastActivityChange).TotalSeconds); // time since last change (in sec)

            if (s < 5) { return; /* no changes */ }

            /* we consider that the dwarf has a one chance out of four of changing activity,
             * this probability is increased as time passes.
             * thus, after 60 sec, the dwarf has a 0,3571 proba to change
             * after 120 sec, he has a 0,625 proba to change
             * 
             * Fact is : the more he "rethinks", the more the risk that he changes his activity increases
             */
            var chances = 200 - ( s * gameEnvironment.Variables.attenuateTimeImpact );

            if (rnd.Next(0, (int)chances) > 50) { return; /* no changes */ }

            #region let's change his activity
            
            List<_WeightedObject> list = new List<_WeightedObject>();

            var w = this.WorkDesire;
            var p = this.Pickaxe;
            var t = this.Thirst;
            
            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.GoToForge)
            { list.Add(new _WeightedObject(ActivitiesLabel.GoToForge,
                (int)((w + 2*(100 - p)) / 3)));
            }

            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.Deviant)
            { list.Add(new _WeightedObject(ActivitiesLabel.Deviant, 
                (int)(((100 - w) + t) / 2)));
            }

            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.Explorer)
            { list.Add(new _WeightedObject(ActivitiesLabel.Explorer,
                (int)((w + (100 - p)) / 2)));
            }

            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.Miner)
            { list.Add(new _WeightedObject(ActivitiesLabel.Miner,
                (int)((w + p) / 2)));
            }

            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.Supply)
            { list.Add(new _WeightedObject(ActivitiesLabel.Supply,
                (int)((w + (100 - t)) / 2)));
            }

            if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit && _currentActivity != ActivitiesLabel.Vigile)
            {
                list.Add(new _WeightedObject(ActivitiesLabel.Vigile,
                  (int)(w)));
            }

            #endregion

            WeightedList startingActivity = new WeightedList(list);
            _currentActivity = (ActivitiesLabel)startingActivity.SelectRandomItem();
        }

        public Vector3 GetNewDestination()
        {
            Vector3 destination = new Vector3();

            List<_WeightedObject> destList = new List<_WeightedObject>();

            System.Random rnd = new System.Random();
            /*  TODO : check qu'il n'y a pas un problème, genre que les nains ne prennent pas tous la même destination */

            int w;

            // Deviant : generation of random decisions, plus a "clever" decision : the beer.
            if (this._currentActivity == ActivitiesLabel.Deviant)                
            {
                for (int i = 0; i < 10; i++)
                {
                    do
                    {
                        destination = new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500));
                    } while (
                        (Vector3.Distance(dwarfTransf.position, destination) < gameEnvironment.Variables.expl_positionTooClose)
                        && KnownMines.All(
                            mine => (Vector3.Distance(mine.MinePosition, destination) < gameEnvironment.Variables.expl_positionTooKnown)
                        )
                    );
                    destList.Add(new _WeightedObject(destination, 1));
                }
                destList.Add(new _WeightedObject(gameEnvironment.Variables.beerPosition, gameEnvironment.Variables.dev_goToBeer));
            }

            // Explorer : generation of 10 "random" decisions, just not too close from the dwarf nor known mines
            else if (this._currentActivity == ActivitiesLabel.Explorer) 
            {
                for (int i = 0; i < 10; i++)
                {
                    do {
                        destination = new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500));
                    } while (
                        (Vector3.Distance(dwarfTransf.position, destination) < gameEnvironment.Variables.expl_positionTooClose)
                        && KnownMines.All(
                            mine => (Vector3.Distance(mine.MinePosition, destination) < gameEnvironment.Variables.expl_positionTooKnown)
                        )
                    );
                    destList.Add(new _WeightedObject(destination, 1));
                }
            }

            // Miner : wich known mine will be targeted ?
            else if (this._currentActivity == ActivitiesLabel.Miner)
            {
                foreach (_KnownMine mine in KnownMines.FindAll(m => (!m.Empty)).ToList())
                {
                    w = 3; // TODO: réfléchir/tester
                    w -= mine.DwarvesInTheMine; // the more dwarves are ALREADY in the mine, the less he wants to go
                    
                    if (Vector3.Distance(dwarfTransf.position, mine.MinePosition) < gameEnvironment.Variables.min_closeMinefLimit)
                    { w++; } // this mine is close enough

                    if (w > 0) destList.Add(new _WeightedObject(mine.MinePosition, w));
                }
            }

            // Supply : choose either a mine or a thirsty dwarf
            else if (this._currentActivity == ActivitiesLabel.Supply)
            {
                foreach (_KnownMine mine in KnownMines.FindAll(m => (!m.Empty)).ToList()) // we add a mine if not empty
                {
                    var mPosition = mine.MinePosition;

                    w = 1;
                    w += mine.DwarvesInTheMine; // the more dwarves in the mine, the more he wants to go

                    if (Vector3.Distance(dwarfTransf.position, mPosition) < gameEnvironment.Variables.sup_closeMinefLimit)
                    { w++; } // this mine is close enough

                    if (mine.ThirstEvaluationResult()) { w += 2; } // I know they want to drink in this mine

                    destList.Add(new _WeightedObject(mPosition,  w));
                }
                foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.highThirst)).ToList()) // we add thirsty dwarves (weight depending on how close he is)
                {
                    var dPosition = dwarf.dwarfPosition;
                    w = (Vector3.Distance(dwarfTransf.position, dPosition) < gameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                    destList.Add(new _WeightedObject(dPosition, w));
                }
            }

            // Vigile : go to a deviant dwarf, or a thirsty one if there's no deviant one
            else if (this._currentActivity == ActivitiesLabel.Vigile)
            {
                foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.deviant)).ToList())
                // we add deviant dwarves (weight depending on how close he is)
                {
                    var dPosition = dwarf.dwarfPosition;
                    w = (Vector3.Distance(dwarfTransf.position, dPosition) < gameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                    destList.Add(new _WeightedObject(dPosition, w));
                }
                
                // if the vigile has no possible target, he goes to a (close?) thirsy dwarf
                if (!destList.Any()) {
                    foreach (_KnownDwarf dwarf in KnownDwarves.FindAll(d => (d.highThirst)).ToList())
                    {
                        var dPosition = dwarf.dwarfPosition;
                        w = (Vector3.Distance(dwarfTransf.position, dPosition) < gameEnvironment.Variables.sup_closeDwarfLimit) ? 5 : 1;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                };
            }

            // forge : go to the forge
            else if (this._currentActivity == ActivitiesLabel.GoToForge)
            {
                if (this.Pickaxe <= gameEnvironment.Variables.pickaxeLimit)
                {
                    return (Vector3)gameEnvironment.Variables.forgePosition;
                }
            }

            else /* change to another activity and rester destination */
            { RethinkActivity(); return GetNewDestination(); }


            if (!destList.Any()) /* change to another activity and reset destination */
            { RethinkActivity(); return GetNewDestination(); }

            WeightedList destinations = new WeightedList(destList);
            return (Vector3)destinations.SelectRandomItem();
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
            private int[] _gauges = new int[3];
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
                get { return _gauges[2]; }
                set { _gauges[2] = StockGauge(value); }
            }
            #endregion

            public _Gauges(int thirst, int workDesire, int pickaxe, GameEnvironment gameEnv)
            {
                ge = gameEnv;
                _gauges[0] = StockGauge(thirst);
                _gauges[1] = StockGauge(workDesire);
                _gauges[2] = StockGauge(pickaxe);
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
