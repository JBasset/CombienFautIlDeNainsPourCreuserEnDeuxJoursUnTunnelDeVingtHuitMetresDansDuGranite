using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    public class DwarfMemory:MonoBehaviour
    {
        private DwarfBehaviour _dwarfBehaviour;
        private Transform _dwarfTransf;

        public GameEnvironment GameEnvironment;
        public GameObject OccupiedMine;
        public Vector3? SavedDestination;

        public ActivitiesLabel CurrentActivity { get; private set; }

        public float LastActivityChange { get; private set; } // UnityEngine.Time

        #region Gauges
        private int StockGauge(int value)
        {
            var max = GameEnvironment.Variables.maxValueGauge;
            var min = GameEnvironment.Variables.minValueGauge;
            return value >= max ? max : (value <= min ? min : value);
        }

        private int _thirstSatisfaction;
        public int ThirstSatisfaction
        { get { return _thirstSatisfaction; } set { _thirstSatisfaction = StockGauge(value); } }

        private int _workDesire;
        public int WorkDesire
        { get { return _workDesire; } set { _workDesire = StockGauge(value); } }

        private int _pickaxe;
        public int Pickaxe
        { get { return _pickaxe; } set { _pickaxe = StockGauge(value); } }

        private int _beerCarried;
        public int BeerCarried
        { get { return _beerCarried; } set { _beerCarried = StockGauge(value); } }
        #endregion

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

        #region Knowledge about Mines and Dwarves
        private List<_KnownDwarf> _knownDwarves = new List<_KnownDwarf>(); // nainConnus
        public List<_KnownDwarf> KnownDwarves { get { return _knownDwarves; } }

        private List<_KnownMine> _knownMines = new List<_KnownMine>();
        public List<_KnownMine> KnownMines { get { return _knownMines; } }

        /* when a dwarf meets another, or walk through a mine, 
         * he may update his knowledge of the mines 
         * using this classe's Update functions*/
        #endregion
        
        public void Start()
        {
            _dwarfBehaviour = GetComponent<DwarfBehaviour>();
            _dwarfTransf = GetComponent<Transform>();
            //GameEnvironment = dwarfTransf.parent.parent.parent.GetComponent<GameEnvironment>();
        }

        public void DwarfMemoryInitialization()
        {
            CurrentActivity = (ActivitiesLabel)GameEnvironment.Variables.startingActivity.SelectRandomItem();
            LastActivityChange = UnityEngine.Time.time;

            IncreaseBy(GaugesLabel.ThirstSatisfaction, 100);
            IncreaseBy(GaugesLabel.Workdesire, 100);
            IncreaseBy(GaugesLabel.Pickaxe, 100);

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
        
        #region Gauges's Increase and Lower functions ( param : VariableStorage.GaugesLabel theGauge, int byValue )

        public void IncreaseBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(ThirstSatisfaction,10)
            if (byValue <= 0) return;
            switch (theGauge)
            {
                case GaugesLabel.ThirstSatisfaction:
                    ThirstSatisfaction += byValue;
                    break;
                case GaugesLabel.Workdesire:
                    WorkDesire += byValue;
                    break;
                case GaugesLabel.Pickaxe:
                    Pickaxe += byValue;
                    break;
                case GaugesLabel.BeerCarried:
                    BeerCarried += byValue;
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
                    ThirstSatisfaction -= byValue;
                    break;
                case GaugesLabel.Workdesire:
                {
                    WorkDesire -= byValue;
                    break;
                }
                case GaugesLabel.Pickaxe:
                    Pickaxe -= byValue;
                    break;
                case GaugesLabel.BeerCarried:
                    BeerCarried -= byValue;
                    break;
                default:
                    return;
            }
        }

        #endregion

        #region Update functions

        public void UpdateMine(Vector3 thePosition, int newThirstyDwarves, int newDwarvesInTheMine, int ore,
            float newDateTime, string newName)
        {
            // maybe this mine is already in the list
            var thisMine = _knownMines.Where(m => m.Name == newName).ToList();

            if (!thisMine.Any())
            {
                _knownMines.Add(
                    new _KnownMine(thePosition, newDwarvesInTheMine, newThirstyDwarves, ore, newDateTime, newName)
                    );
            }
            // if the mine isnt already known, let's add it

            else if (thisMine.First().InformatonTakenDateTime > newDateTime)
            {
                thisMine.First().InformatonTakenDateTime = newDateTime;
                thisMine.First().Ore = ore;
                thisMine.First().DwarvesInTheMine = newDwarvesInTheMine;
                thisMine.First().ThirstyDwarves = newDwarvesInTheMine < newThirstyDwarves ? 0 : newThirstyDwarves;
            }

            // else our information is more recent
        }

        public void UpdateMine(_KnownMine newMine)
        {
            // maybe this mine is already in the list
            var thisMine = _knownMines.FindAll(m => m.Name == newMine.Name);

            if (!thisMine.Any())
            {
                _knownMines.Add(
                    new _KnownMine(
                        newMine.MinePosition, newMine.DwarvesInTheMine, newMine.ThirstyDwarves, newMine.Ore,
                        newMine.InformatonTakenDateTime, newMine.Name)
                );
            }
            // if the mine isnt already known, let's add it

            else if (thisMine.First().InformatonTakenDateTime > newMine.InformatonTakenDateTime)
            {
                _knownMines.RemoveAll(m => m.Name == newMine.Name);
                _knownMines.Add(newMine);
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

        public void UpdateDwarf(DwarfMemory metPerson, float newInformatonTakenDateTime, Vector3 position)
        {
            if (metPerson.name == name) return;
            
            // maybe this mine is already in the list
            var sameD = _knownDwarves.FindAll(d => d.Name == metPerson.name);

            if (!sameD.Any())
            {
                _knownDwarves.Add(new _KnownDwarf(metPerson));
            }
            // if the mine isnt already known, let's add it

            else if (sameD.First().InformatonTakenDateTime > newInformatonTakenDateTime)
            {
                _knownDwarves.RemoveAll(d => d.Name == metPerson.name);
                _knownDwarves.Add(new _KnownDwarf(metPerson));
            }

            // else our information is more recent
        }

        public void UpdateDwarf(_KnownDwarf newDwarf)
        {
            if (newDwarf.Name == name) return;
            
            // maybe this dwarf is already in the list
            var sameD = _knownDwarves.FindAll(d => d.Name == newDwarf.Name);

            if (!sameD.Any())
            {
                _knownDwarves.Add(newDwarf);
            }
            // if the dwarf isnt already known, let's add it

            else if (sameD.First().InformatonTakenDateTime > newDwarf.InformatonTakenDateTime)
            {
                _knownDwarves.RemoveAll(d => d.Name == newDwarf.Name);
                _knownDwarves.Add(newDwarf);
            }

            // else our information is more recent
        }

        #endregion

        // The dwarf rethinks his activity : he may 1- change 2- keep on doing what he's doing
        public bool RethinkActivity() {
            var rnd = new System.Random();

            #region STEP ONE : CHECK TIME

            var time = (int)((UnityEngine.Time.time - LastActivityChange));// time since last change (in UnityEngine.Time)

            if (time < GameEnvironment.Variables.activityRethinkLimit)
                return false;

            #endregion

            #region STEP TWO : DO I CHANGE MY ACTIVITY ? (probability to change calculation)

            /* we consider that the dwarf has an initial 10% of changing his activity,
             * this probability is increased as time passes.
             * 
             * Fact is : the more he "rethinks", the more the risk that he changes his activity increases
             */
             
            double chanceToChangeMyActivity = 10;
            if (CurrentActivity != ActivitiesLabel.Miner) // A mining dwarves is having too much fun to see the time pass
                chanceToChangeMyActivity += (time * GameEnvironment.Variables.H.attenuateTimeImpact);

            if (CurrentActivity != ActivitiesLabel.Deviant)
            { chanceToChangeMyActivity += (0.5 * (100 - this.ThirstSatisfaction)); }

            switch (CurrentActivity)
            {
                case ActivitiesLabel.Deviant:
                {
                    // I'm (most likely) not a deviant anymore !
                    if (ThirstSatisfaction > 75)
                        chanceToChangeMyActivity += 0.5 * WorkDesire;

                    // OR I may stay a deviant for a while
                    if (ThirstSatisfaction < 25)
                        chanceToChangeMyActivity -= 0.5 * (100 - WorkDesire);
                    break;
                }
                case ActivitiesLabel.Explorer:
                {
                    // Exploration means a lot for an explorer : AN EXPLORER'S WATCH WON'T END (unless he needs beer) (or find something)
                    if (KnownMines.Count(m => m.Ore > 10 * GameEnvironment.Variables.dwarfOreMiningRate) < GameEnvironment.Variables.H.expl_iknwoenough)
                        chanceToChangeMyActivity -= 1.5 * WorkDesire;

                    // Since he knows a few full mines, it's ok to stop

                    if (KnownMines.Count(m => m.Ore > 10 * GameEnvironment.Variables.dwarfOreMiningRate) >
                        GameEnvironment.Variables.H.expl_iknwoenough)
                        chanceToChangeMyActivity += 0.5 * WorkDesire;
                    break;
                }
                case ActivitiesLabel.Vigile:
                    if (!KnownDwarves.Any(d => d.Deviant) && !KnownDwarves.Any(d => d.HighThirst))
                    {
                        chanceToChangeMyActivity = 100;
                        break;
                    }
                    if (KnownDwarves.Count(d => d.Deviant) < 2)
                        chanceToChangeMyActivity += 0.5 * WorkDesire;
                    break;

                case ActivitiesLabel.Supply:
                    if (!KnownMines.Any(m => m.DwarvesInTheMine > 0) && !KnownDwarves.Any(d => d.HighThirst))
                    {
                        chanceToChangeMyActivity = 100;
                        break;
                    }
                    if (KnownDwarves.Count(d => d.HighThirst) < 2)
                        chanceToChangeMyActivity += 0.5 * WorkDesire;
                    if (BeerCarried > 20)
                        chanceToChangeMyActivity -= 1.5 * WorkDesire;
                    else if (BeerCarried < 20 && !(GetComponent<NavMeshAgent>().destination == GameEnvironment.Variables.beerPosition))
                        chanceToChangeMyActivity += 3 * WorkDesire;
                    if (GetComponent<NavMeshAgent>().destination == GameEnvironment.Variables.beerPosition)
                    { chanceToChangeMyActivity -= 1.5 * WorkDesire; Debug.Log("reloading..."); }
                    break;

                case ActivitiesLabel.Miner:
                {   
                    if (!KnownMines.Any(m => m.Ore >= 0) || (OccupiedMine && OccupiedMine.GetComponent<MineBehaviour>().Ore == 0))
                    {
                            chanceToChangeMyActivity = 100;
                            break;
                    }

                    if (!OccupiedMine && KnownMines.Any(m => m.Ore > 50))
                        chanceToChangeMyActivity -= WorkDesire;

                    if (!OccupiedMine) break;

                    // I currently am in a mine

                    if (OccupiedMine.GetComponent<MineBehaviour>().Ore > 50)
                        chanceToChangeMyActivity -= 2 * WorkDesire;

                    if (OccupiedMine.GetComponent<MineBehaviour>().Ore < 10)
                        chanceToChangeMyActivity += 0.5 * WorkDesire;

                    break;
                }
                case ActivitiesLabel.GoToForge:
                    if (Pickaxe < 20)
                        chanceToChangeMyActivity -= 0.5 * WorkDesire;
                    else
                        chanceToChangeMyActivity += 0.5 * WorkDesire;
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

            if (CurrentActivity != ActivitiesLabel.GoToForge
                && this.Pickaxe <= GameEnvironment.Variables.H.pickaxeLimit)
            {
                var w0 = (int)((w + 2 * (100 - p)) / 3);
                list.Add(new _WeightedObject(ActivitiesLabel.GoToForge, w0));
            }

            if (CurrentActivity != ActivitiesLabel.Deviant)
            {
                var w0 = (int)(((100 - w) + t) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Deviant, w0));
            }

            if (CurrentActivity != ActivitiesLabel.Explorer)
            {
                var w0 = (KnownMines.Any()) ? (int)((w + (100 - p)) / 2) : (int)((w + 100) / 2);
                list.Add(new _WeightedObject(ActivitiesLabel.Explorer, w0));
            }

            if (CurrentActivity != ActivitiesLabel.Miner
                && KnownMines.Any(m => m.Ore >= 5) && this.Pickaxe >= 10)
            {
                var w0 = (int)((200 + w + p) / 4); // The simulation being about mining, the chance of dwarves being miners must be higher
                list.Add(new _WeightedObject(ActivitiesLabel.Miner, w0));
            }

            if (CurrentActivity != ActivitiesLabel.Supply)
            {
                var w0 = 0;
                if (KnownDwarves.Any(d => d.HighThirst) && this.ThirstSatisfaction >= 10)
                {
                    w0 = (int)((w + (100 - t)) / 2);
                }

                if (BeerCarried < 10 &&
                    Vector3.Distance(_dwarfTransf.position, GameEnvironment.Variables.beerPosition) <
                    GameEnvironment.Variables.H.sup_closeBeerStorage)
                {
                    w0 = 100;
                }
                list.Add(new _WeightedObject(ActivitiesLabel.Supply, w0));
            }
            
            if (CurrentActivity != ActivitiesLabel.Vigile
                && KnownDwarves.Any(d => d.Deviant) && this.ThirstSatisfaction >= 10)
            {
                var w0 = w;

                // if I know a close endangered dwarf, i may consider beeing a vigile
                if (KnownDwarves.Any(d => (d.Deviant || d.HighThirst) &&
                                          Vector3.Distance(_dwarfTransf.position, d.DwarfPosition) <
                                          GameEnvironment.Variables.H.vig_closeDwarfLimit))
                {
                    w0 += w;
                }
                list.Add(new _WeightedObject(ActivitiesLabel.Vigile, w0));
            }

            #endregion

            #region STEP FOUR : TIME TO MAKE A DECISION

            var startingActivity = new WeightedList(list);

            if (!startingActivity.List.Any())
            {
                CurrentActivity = ActivitiesLabel.Deviant;
                LastActivityChange = Time.time;
                return true;
            }

            var newActivity = (ActivitiesLabel)startingActivity.SelectRandomItem();
            CurrentActivity = newActivity;

            #endregion
            LastActivityChange = Time.time;
            return true;
        }

        public Vector3 GetRandomDestination()
        {
            var rnd = new System.Random();
            return FixDestination(new Vector3(rnd.Next(0, 500), 0, rnd.Next(0, 500)));
        }
        
        public Vector3 GetNewDestination() /* 
            Check if the dwarf had already a destination
             */
        {
            #region the dwarf may have encapsuled a destination
            if (SavedDestination != null)
            {
                return (Vector3)SavedDestination;
            }
            #endregion

            Vector3 destination;
            var destList = new List<_WeightedObject>();

            
            int w; // a 0~100 weight

            #region STEP ONE : EXPLORING MY OPTIONS (depending on my activity)

            switch (CurrentActivity)
            {
                // for each kind of activity, we add potential destinations

                case ActivitiesLabel.Deviant:

                    #region Adds a random (CLOSE enough) destination (10), plus the Beer position (Variables.dev_goToBeer)

                    // a deviant will stay close
                    do
                    {
                        destination = GetRandomDestination();
                    } while
                    (
                        Vector3.Distance(_dwarfTransf.position, destination) >= GameEnvironment.Variables.H.dev_positionCloseEnough
                        );
                    destList.Add(new _WeightedObject(destination, 10));
                    destList.Add(new _WeightedObject(GameEnvironment.Variables.beerPosition,
                        GameEnvironment.Variables.H.dev_goToBeer));
                    break;

                #endregion

                case ActivitiesLabel.Explorer:

                    #region Adds a random destination (10) [ not to close from me nor too close from a known mine ]
                    
                    do
                        {
                            destination = GetRandomDestination();
                        } while
                        (
                            Vector3.Distance(_dwarfTransf.position, destination) < GameEnvironment.Variables.H.expl_positionTooClose
                            
                            && KnownMines.All(
                                mine => Vector3.Distance(mine.MinePosition, destination) <
                                    GameEnvironment.Variables.H.expl_positionTooKnown)
                        );
                    

                    destList.Add(new _WeightedObject(destination, 10));
                    break;
                    
                #endregion

                case ActivitiesLabel.Miner:

                    #region Adds every non-empty mines (0-100 depending on distance and dwarf number)

                    foreach (var mine in KnownMines.FindAll(m => m.Ore > 10 * GameEnvironment.Variables.dwarfOreMiningRate).ToList())
                    {
                        w = 80 - (int) (mine.DwarvesInTheMine * GameEnvironment.Variables.H.min_pplInTheMineImportance);
                        // the more dwarves are ALREADY in the mine, the less he wants to go

                        // I like close mines better
                        if ( Vector3.Distance(_dwarfTransf.position, mine.MinePosition) < GameEnvironment.Variables.H.min_closeMinefLimit)
                            w += 20;

                        if (w > 0) destList.Add(new _WeightedObject(mine.MinePosition, w));
                    }

                    if (destList.Any())
                        break;

                    if (KnownMines.Any())
                        destList.Add(new _WeightedObject(KnownMines.First().MinePosition, 1));
                    else
                        RethinkActivity();

                    break;

                #endregion

                case ActivitiesLabel.Supply:

                    #region Adds every occuped mines (dwarf number + 0-20 depending on distance and thirst evaluation)

                    if (BeerCarried <= 20) // a supply who has nothing to supply ain't a good supply
                        destList.Add(new _WeightedObject(GameEnvironment.Variables.beerPosition, ( 100- BeerCarried )));
                    
                    foreach (var mine in KnownMines.FindAll(m => m.DwarvesInTheMine > 0).ToList()
                    ) // we add a mine if not empty
                    {
                        var mPosition = mine.MinePosition;
                        w = mine.DwarvesInTheMine;
                        // the more dwarves in the mine, the more he wants to go

                        // I like close mines better
                        if (Vector3.Distance(_dwarfTransf.position, mine.MinePosition) < GameEnvironment.Variables.H.sup_closeMinefLimit)
                            w += 20;

                        w += 2 * mine.ThirstyDwarves;
                        // I know they want to drink in this mine

                        destList.Add(new _WeightedObject(mPosition, w));
                    }
                    #endregion

                    #region Adds known thirsty dwarves (50 or 5 depending on distance)

                    foreach (var dwarf in KnownDwarves.FindAll(d => d.HighThirst).ToList())
                        // we add thirsty dwarves (weight depending on how close he is)
                    {
                        // I like close dwarves better
                        w = (Vector3.Distance(_dwarfTransf.position, dwarf.DwarfPosition) < GameEnvironment.Variables.H.sup_closeDwarfLimit)
                            ? 50
                            : 5;

                        destList.Add(new _WeightedObject(dwarf.DwarfPosition, w));
                    }
                    
                    #endregion

                    // Security
                    if (!destList.Any())
                        destList.Add(new _WeightedObject(GameEnvironment.Variables.beerPosition, 1));
                    break;

                case ActivitiesLabel.Vigile:

                    #region Adds deviant known dwarves (50 or 5 depending on distance)

                    foreach (var dwarf in KnownDwarves.FindAll(d => d.Deviant).ToList())
                    {
                        // I like close dwarves better
                        w = (Vector3.Distance(_dwarfTransf.position, dwarf.DwarfPosition) < GameEnvironment.Variables.H.vig_closeDwarfLimit)
                            ? 50
                            : 5;
                        if (w > 0) destList.Add(new _WeightedObject(dwarf.DwarfPosition, w));
                    }

                    #endregion

                    if (destList.Any()) break;

                    #region (IF no target) Adds all thirsy dwarves (5 or 50 depending on distance)

                    foreach (var dwarf in KnownDwarves.FindAll(d => d.HighThirst).ToList())
                    {
                        // I like close dwarves better
                        w = (Vector3.Distance(_dwarfTransf.position, dwarf.DwarfPosition) < GameEnvironment.Variables.H.vig_closeDwarfLimit)
                            ? 50
                            : 5;
                        destList.Add(new _WeightedObject(dwarf.DwarfPosition, w));
                    }
                    break;

                #endregion

                case ActivitiesLabel.GoToForge:

                    #region Adds forge (not questionable)

                    if (Pickaxe <= GameEnvironment.Variables.H.pickaxeLimit)
                    {
                        var d = GameEnvironment.Variables.forgePosition;
                        SavedDestination = d;
                        return d;
                    }
                    break;

                #endregion

                default:
                {
                    RethinkActivity();
                    return GetNewDestination(); //iteration until a destination is chosen
                }
            }

            #endregion
            
            if (!destList.Any()) // change to another activity and reset destination
            {
                RethinkActivity();
                try
                { return GetNewDestination(); }
                catch(StackOverflowException)
                {
                    Debug.Log("--------------------------------");
                    // TODO : find out how to solve this problem
                    Debug.Log(" The game freezes due to a problem whe cannot solve :");
                    Debug.Log(" Stackoverflow in DwarfMemory.GetNewDestination");
                    Debug.Log("On gameObject : " + gameObject.name);
                    Debug.Log(CurrentActivity);
                    Debug.Log("KnownMines :" + KnownMines.Count);
                    Debug.Log("KnownMines with enough gold :" + KnownMines.Count(m => m.Ore >= GameEnvironment.Variables.dwarfOreMiningRate * 10));
                    Debug.Log("KnownMines with dwarves inside :" + KnownMines.Count(m => m.DwarvesInTheMine > 0));
                    Debug.Log("KnownDwarves :" + KnownDwarves.Count);
                    Debug.Log("KnownDwarves thirsty :" + KnownDwarves.Count(d => d.HighThirst));
                    Debug.Log("KnownDwarves deviant + :" + KnownDwarves.Count(d => d.Deviant));
                    Debug.Log("--------------------------------");
                    return new Vector3(0, 0, 0);
                }
            }
            
            #region STEP TWO : SELECT AN OPTION
            var destinations = new WeightedList(destList);

            SavedDestination = (Vector3) destinations.SelectRandomItem();
            #endregion
            
            return (Vector3)SavedDestination;
        }
        
        private static Vector3 FixDestination(Vector3 destination) // make sure that the destination is accessible
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(destination, out hit, 1000, 1);
            return hit.position;
        }

        public class _KnownDwarf
        {
            public Vector3 DwarfPosition;
            public string Name;
            public bool HighThirst;
            public float InformatonTakenDateTime;
            public bool Deviant;

            public _KnownDwarf(DwarfMemory anotherPerson)
            {
                Name = anotherPerson.name;
                HighThirst = anotherPerson.ThirstSatisfaction < 50;
                Deviant = anotherPerson.CurrentActivity == ActivitiesLabel.Deviant;
                InformatonTakenDateTime = Time.time;
            }
        }

        public class _KnownMine
        {
            public string Name;
            public Vector3 MinePosition;
            public float InformatonTakenDateTime;

            public int Ore;

            private int _dwarvesInTheMine;
            public int DwarvesInTheMine { get { return _dwarvesInTheMine; } set { _dwarvesInTheMine = (value > 0) ? value : 0; } }

            private int _thirstyDwarves;
            public int ThirstyDwarves { get { return _thirstyDwarves; } set { _thirstyDwarves = (value > 0) ? value : 0; } }
            // number of dwarves under thirstyDwarvesGaugeLimit

            public _KnownMine(Vector3 minePosition, int dwarvesInTheMine, int thirstyDwarves, int ore,
                float newDateTime, string name)
            {
                Ore = ore;
                Name = name;
                MinePosition = minePosition;
                InformatonTakenDateTime = newDateTime;
                _dwarvesInTheMine = dwarvesInTheMine > 0 ? dwarvesInTheMine : 0;
                _thirstyDwarves = _dwarvesInTheMine < thirstyDwarves ? 0 : thirstyDwarves;
            }
        }
    }

    
}
