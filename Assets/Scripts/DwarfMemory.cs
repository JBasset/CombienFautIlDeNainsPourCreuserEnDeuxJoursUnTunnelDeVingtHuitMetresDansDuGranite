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

        public readonly int minValueGauge;
        public readonly int maxValueGauge;

        public GameObject OccupiedMine;

        public Vector3? savedDestination;

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
        

        public void UpdateMine(Vector3 thePosition, int newThirstyDwarves, int newDwarvesInTheMine, int ore, DateTime newDateTime, string newName)
        {            
            // maybe this mine is already in the list
            var thisMine = _knownMines.Where(m => m.Name == newName).ToList();

            if (!thisMine.Any()) _knownMines.Add(new _KnownMine(thePosition, newDwarvesInTheMine, newThirstyDwarves, ore, newDateTime, newName));
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
            var thisMine = _knownMines.Where(m => m.Name == newMine.Name).ToList();

            if (!thisMine.Any()) _knownMines.Add(new _KnownMine(newMine.MinePosition, newMine.DwarvesInTheMine, newMine.ThirstyDwarves, newMine.Ore, newMine.informatonTakenDateTime, newMine.Name));
            // if the mine isnt already known, let's add it

            else if (thisMine[0].informatonTakenDateTime > newMine.informatonTakenDateTime)
            {
                var index = _knownMines.FindIndex(m => (m.Name == newMine.Name));
                _knownMines[index].informatonTakenDateTime = newMine.informatonTakenDateTime;
                _knownMines[index].Ore = newMine.Ore;
                _knownMines[index].DwarvesInTheMine = newMine.DwarvesInTheMine;
                _knownMines[index].ThirstyDwarves = (newMine.DwarvesInTheMine < newMine.ThirstyDwarves) ? 0 : newMine.ThirstyDwarves;
            }

            // else our information is more recent

        }

        public void UpdateMemory(List<_KnownDwarf> newKnownDwarves, List<_KnownMine> newKnownMines)
        {
            foreach (var newDwarf in newKnownDwarves)
            {
                UpdateDwarf(newDwarf);
            }
            foreach (var newMine in newKnownMines)
            {
                UpdateMine(newMine);   
            }
        }

        public void UpdateDwarf(DwarfMemory metPerson, DateTime newInformatonTakenDateTime, Vector3 position)
        {
            // maybe he is already in the list
            var sameD = _knownDwarves.Where(d => d.Name == metPerson.name).ToList();

            if (!sameD.Any()) _knownDwarves.Add(new _KnownDwarf(metPerson));
            // if the mine isnt already known, let's add it

            else if (sameD[0].informatonTakenDateTime > newInformatonTakenDateTime)
            {
                var index = _knownDwarves.FindIndex(d => (d.Name == metPerson.name));
                _knownDwarves[index].informatonTakenDateTime = newInformatonTakenDateTime;
                _knownDwarves[index].HighThirst = metPerson.ThirstSatisfaction < 50;
                _knownDwarves[index].Deviant = (metPerson._currentActivity == ActivitiesLabel.Deviant);
                _knownDwarves[index].DwarfPosition = position;
            }

            // else our information is more recent
        }

        public void UpdateDwarf(_KnownDwarf newDwarf)
        {
            // maybe he is already in the list
            var sameD = _knownDwarves.Where(d => d.Name == newDwarf.Name).ToList();

            if (!sameD.Any()) _knownDwarves.Add(newDwarf);
            // if the mine isnt already known, let's add it

            else if (sameD[0].informatonTakenDateTime > newDwarf.informatonTakenDateTime)
            {
                var index = _knownDwarves.FindIndex(d => (d.Name == newDwarf.Name));
                _knownDwarves[index].informatonTakenDateTime = newDwarf.informatonTakenDateTime;
                _knownDwarves[index].HighThirst = newDwarf.HighThirst;
                _knownDwarves[index].Deviant = newDwarf.Deviant;
                _knownDwarves[index].DwarfPosition = newDwarf.DwarfPosition;
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
                    if (ThirstSatisfaction > 75 || WorkDesire > 75)
                        { chanceToChangeMyActivity += (0.5 * this.WorkDesire); }
                    
                    // I may stay a deviant for a while
                    if (ThirstSatisfaction < 25 || WorkDesire < 25)
                        { chanceToChangeMyActivity -= (0.5 * ( 100 - this.WorkDesire) ); }
                    break;
                    }
                case ActivitiesLabel.Explorer:
                {
                    // Exploration means a lot for an explorer : AN EXPLORER'S WATCH WON'T END (unless he needs beer) (or find something)
                    if (!KnownMines.Any())
                        { chanceToChangeMyActivity -= this.WorkDesire; }

                    // Since he knows a few full mines, it's ok to stop
                    if (KnownMines.Count(m => m.Ore > 70) > GameEnvironment.Variables.expl_iknwoenough)
                        { chanceToChangeMyActivity += 0.5*this.WorkDesire; }
                    break;
                }
                case ActivitiesLabel.Vigile: //TODO : completer
                    break;
                case ActivitiesLabel.Supply:  //TODO : completer
                    break;
                case ActivitiesLabel.Miner:  //TODO : completer : je suis actuellement un mineur, je compte le rester un moment ! C'est mon objectif dans la vie quand même.
                    if (OccupiedMine && OccupiedMine.GetComponent<MineBehaviour>().Ore > 50)
                    {
                        chanceToChangeMyActivity -= 0.5 * this.WorkDesire;
                    }
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

            if (!startingActivity.List.Any())
            {
                _currentActivity = ActivitiesLabel.Deviant;
                return true;
            }

            var newActivity = (ActivitiesLabel)startingActivity.SelectRandomItem();
            // Debug.Log("Hey " + this.name +" just changed his activity from " +_currentActivity +" to " +newActivity);
            _currentActivity = newActivity;
            //_currentActivity = ActivitiesLabel.Explorer;
            
            #endregion
            
            return true;
        }

        public Vector3 GetRandomDestination()
        {
            var rnd = new System.Random();
            return FixDestination(new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500)));
        }

        public bool DistantEnough(Vector3 element, Vector3 element2, int value)
        {
            return Vector3.Distance(element, element2) >= value;
        }


        public Vector3 GetNewDestination()
        {
            var currentPosition = _dwarfTransf.position;

            #region : the dwarf may have encapsuled a destination
            if (savedDestination != null)
            {
                var d = (Vector3)savedDestination;
                savedDestination = null;
                return d;
            }
            #endregion

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
                        do
                        {
                            destination = GetRandomDestination();
                        }
                        while 
                        (
                            DistantEnough(
                                currentPosition, destination, GameEnvironment.Variables.expl_positionTooClose
                            ) 
                            && KnownMines.All(
                                mine => (
                                DistantEnough(mine.MinePosition, destination, GameEnvironment.Variables.expl_positionTooKnown)
                                ))
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
                            destination = GetNewDestination();
                        }
                        while
                        (
                            DistantEnough(
                                currentPosition, destination, GameEnvironment.Variables.expl_positionTooClose
                            )
                            && KnownMines.All(
                                mine => (
                                    DistantEnough(mine.MinePosition, destination, GameEnvironment.Variables.expl_positionTooKnown)
                                ))
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

                        if (
                            DistantEnough(currentPosition, mine.MinePosition, GameEnvironment.Variables.min_closeMinefLimit)
                            )
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

                        if (
                            DistantEnough(currentPosition, mine.MinePosition, GameEnvironment.Variables.sup_closeMinefLimit)
                            )
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
                        w = DistantEnough(currentPosition, dPosition, GameEnvironment.Variables.sup_closeDwarfLimit) 
                            ? 50 
                            : 10;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                    break;
                    #endregion
                case ActivitiesLabel.Vigile:
                    #region Adds deviant known dwarves (50 or 10 depending on distance)
                    foreach (var dwarf in KnownDwarves.FindAll(d => (d.Deviant)).ToList())
                    {
                        var dPosition = dwarf.DwarfPosition;
                        w = DistantEnough(currentPosition, dPosition, GameEnvironment.Variables.vig_closeDwarfLimit) 
                            ? 50 
                            : 10;
                        destList.Add(new _WeightedObject(dPosition, w));
                    }
                    #endregion
                    if (destList.Any()) { break; }
                    #region (IF no target) Adds all thirsy dwarves (10 or 50 depending on distance)
                    foreach (var dwarf in KnownDwarves.FindAll(d => (d.HighThirst)).ToList())
                        {
                            var dPosition = dwarf.DwarfPosition;
                        w = DistantEnough(currentPosition, dPosition, GameEnvironment.Variables.vig_closeDwarfLimit) 
                            ? 50 
                            : 10;
                        destList.Add(new _WeightedObject(dPosition, w));
                        }
                    break;
                    #endregion
                case ActivitiesLabel.GoToForge:
                    #region Adds forge (not questionable)
                    if (this.Pickaxe <= GameEnvironment.Variables.pickaxeLimit)
                    {
                        var d = (Vector3)GameEnvironment.Variables.forgePosition;
                        savedDestination = d;
                        return d;
                    }
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

            savedDestination = (Vector3) destinations.SelectRandomItem();
            #endregion
            
            return (Vector3)savedDestination;
        }

        private Vector3 FixDestination(Vector3 destination) /* make sure that the destination is accessible */
        {
            NavMeshHit hit;
            Debug.Log(NavMesh.SamplePosition(destination, out hit, 2, 1));
            return hit.position;
        }

        public class _KnownDwarf
        {
            public Vector3 DwarfPosition;
            public string Name;
            public bool HighThirst; public DateTime informatonTakenDateTime;
            public bool Deviant;

            public _KnownDwarf(DwarfMemory anotherPerson)
            {
                this.Name = anotherPerson.name;
                this.HighThirst = anotherPerson.ThirstSatisfaction < 50;
                this.Deviant = (anotherPerson._currentActivity == ActivitiesLabel.Deviant);
                this.informatonTakenDateTime = DateTime.Now;
            }
        }

        public class _KnownMine
        {

            public string Name;
            public Vector3 MinePosition;
            public DateTime informatonTakenDateTime;

            public int Ore;

            private int _dwarvesInTheMine;
            public int DwarvesInTheMine { get { return _dwarvesInTheMine; } set { _dwarvesInTheMine = (value > 0) ? value : 0; } }

            private int _thirstyDwarves;
            public int ThirstyDwarves { get { return _thirstyDwarves; } set { _thirstyDwarves = (value > 0) ? value : 0; } }
            // number of dwarves under thirstyDwarvesGaugeLimit

            public _KnownMine(Vector3 minePosition, int dwarvesInTheMine, int thirstyDwarves, int ore, DateTime newDateTime, string name)
            {
                this.Ore = ore;
                this.Name = name;
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
