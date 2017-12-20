using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    public class DwarfBehaviour : MonoBehaviour
    {
        public GameEnvironment GE;

        private DwarfMemory memory;
        private Animator animator;
        private NavMeshAgent agent;
        private int LastSecond;

        private Ray ray;
        private RaycastHit hit;

        private float normalSpeed;
        
        public void Start()
        {
            memory = GetComponent<DwarfMemory>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            LastSecond = 0;
        }

        public void Update()
        {
            // If the agent's path is not usable, we reset it
            if (agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathPartial)
                UpdateActivityAndDestination();

            #region UPDATE - A - Whenever a dwarf is close from another

            // whatever you're doing : stop
            if (DwarvesInSight().Any())
            {
                #region  whenever a dwarf is REALLY close from another, they talk : one learns from another

                foreach (var seenDwarf in DwarvesInSight())
                    if (Vector3.Distance(agent.transform.position, seenDwarf.transform.position) <
                        5) // maybe change ? TODO hahaha
                    {
                        memory.UpdateMemory(seenDwarf.GetComponent<DwarfMemory>().KnownDwarves,
                            seenDwarf.GetComponent<DwarfMemory>().KnownMines);
                        memory.UpdateDwarf(seenDwarf.GetComponent<DwarfMemory>(), Time.time,
                            seenDwarf.transform.position);
                    }

                #endregion

                #region if this dwarf is a Vigile and the really close dwarf a Deviant not willing to go back to work
                if (memory.CurrentActivity == ActivitiesLabel.Vigile && DwarvesInSight().Any(d => d.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Deviant && d.GetComponent<DwarfMemory>().WorkDesire < 80))
                {
                    foreach (var deviant in DwarvesInSight().Where(d => d.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Deviant && d.GetComponent<DwarfMemory>().WorkDesire < 80))
                    {
                        if (Vector3.Distance(transform.position, deviant.transform.position) < 3) // if he reached him
                        {
                            animator.SetFloat("Run", 0);
                            deviant.GetComponent<DwarfMemory>()
                                .IncreaseBy(GaugesLabel.Workdesire, GE.Variables.maxValueGauge);
                            memory.DeviantsStopped++;
                            deviant.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                            UpdateActivityAndDestination();
                        }
                    }
                }
                #endregion

                #region switch : comportement adapté selon l'activité

                foreach (var myD in DwarvesInSight())
                {
                    switch (memory.CurrentActivity)
                    {
                        case ActivitiesLabel.Explorer:
                            if (memory.KnownDwarves.All(kd => kd.Name != myD.name))
                            {
                                MoveTo(myD.transform.position);
                            }
                            break;
                        case ActivitiesLabel.Vigile:
                            if (myD.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Deviant && Vector3.Distance(myD.transform.position, transform.position) >= 3) //if we're not close to the deviant yet
                            {
                                MoveTo(myD.transform.position);
                                animator.SetFloat("Run", 1);
                            }
                            break;
                        case ActivitiesLabel.Supply:
                            if (myD.GetComponent<DwarfMemory>().ThirstSatisfaction < 50)
                            {
                                MoveTo(myD.transform.position);
                                animator.SetFloat("Run", 1);
                            }
                            break;
                        case ActivitiesLabel.Miner:
                            if (memory.KnownDwarves.All(kd => kd.Name != myD.name))
                            {
                                MoveTo(myD.transform.position);
                            }
                            break;
                        case ActivitiesLabel.GoToForge:
                            break;
                        default:
                            break;
                    }
                }

                #endregion

                #region depending on dwarf in sight
                // if this dwarf wants to work, AND the dwarf he sees is a deviant OR is thirsty, consider changing activity
                if (memory.WorkDesire >= 50 && (DwarvesInSight().Any(d => d.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Deviant) || DwarvesInSight().Any(d => d.GetComponent<DwarfMemory>().ThirstSatisfaction < 50)))
                {
                    UpdateActivityAndDestination();
                }
                #endregion / <-- depending on dwarf in sight
            } // ~~~ END if (DwarvesInSight().Any())

            #endregion / <-- UPDATE - A - Whenever a dwarf is close from another

            #region UPDATE - B - Stats update each second

            if (Time.time - LastSecond >= 1)
            {
                LastSecond = (int) Mathf.Floor(Time.time);
                switch (memory.CurrentActivity)
                {
                    case ActivitiesLabel.Miner:
                        memory.TimeAsMiner += 1;
                        break;
                    case ActivitiesLabel.Supply:
                        memory.TimeAsSupply += 1;
                        break;
                    case ActivitiesLabel.Explorer:
                        memory.TimeAsExplorer += 1;
                        break;
                    case ActivitiesLabel.Vigile:
                        memory.TimeAsVigile += 1;
                        break;
                    case ActivitiesLabel.Deviant:
                        memory.TimeAsDeviant += 1;
                        break;
                    case ActivitiesLabel.GoToForge:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            #endregion / <-- UPDATE - B - Stats update each second

            #region UPDATE - C - Explorers move towards Mine Entrance

            if (agent.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Explorer)
                if (MinesInSight().Any())
                    foreach (var myMine in MinesInSight())
                        if (memory.KnownMines.All(kmine => kmine.Name != myMine.name))
                        {
                            MoveTo(myMine.transform.FindChild("MineEntrance").position);
                            UpdateActivityAndDestination();
                            break;
                        }

            #endregion / <-- UPDATE - C - Explorers move towards Mine Entrance

            #region UPDATE - D - Miners exit empty mines
            if (agent.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Miner)
            {
                var om = agent.GetComponent<DwarfMemory>().OccupiedMine;
                if (om && om.GetComponent<MineBehaviour>().Ore == 0)
                    ExitMine();
            }
            #endregion / <-- UPDATE - D - Miners exit empty mines

            #region UPDATE - E - Agent arrived destination

            if (agent.hasPath && Vector3.Distance(agent.destination, agent.transform.position) < 2)
            {
                memory.SavedDestination = null;

                #region UPDATE - E - 1 - Concerning the village (BeerStorage and Forge)
                if (Vector3.Distance(agent.transform.position, GE.Variables.beerPosition) < 2)
                {
                    memory.IncreaseBy(GaugesLabel.BeerCarried, GE.Variables.maxValueGauge);
                    memory.BeerDrank += GE.Variables.maxValueGauge - memory.ThirstSatisfaction;
                    GE.Variables.TotalBeerDrank += GE.Variables.maxValueGauge - memory.ThirstSatisfaction;
                    memory.IncreaseBy(GaugesLabel.ThirstSatisfaction, GE.Variables.maxValueGauge);
                }

                if (Vector3.Distance(agent.transform.position, GE.Variables.forgePosition) < 2)
                    memory.IncreaseBy(GaugesLabel.Pickaxe, GE.Variables.maxValueGauge);
                #endregion / <-- UPDATE - E - 1 - Concerning the village (BeerStorage and Forge)

                #region UPDATE - E - 2 - When the agent reaches his destination he stops
                agent.ResetPath();
                animator.SetFloat("Walk", 0);
                animator.SetFloat("Run", 0);
                #endregion / <-- UPDATE - E - 2 - When the agent reaches his destination he stops

                #region UPDATE - E - 3 - switch (memory.CurrentActivity)
                // I have no path anymore....
                switch (memory.CurrentActivity)
                {
                    case ActivitiesLabel.Deviant:
                        #region Deviant
                        UpdateActivityAndDestination();
                        break;
                        #endregion

                    case ActivitiesLabel.Explorer:
                        #region Explorer

                        var minesClose = MinesInSight()
                            .Where(m => Vector3.Distance(agent.transform.position,
                                            m.transform.FindChild("MineEntrance").position) < 2).ToList();
                        if (minesClose.Any())
                        {
                            var observableMine = minesClose.First();
                            var minePosition = observableMine.transform.FindChild("MineEntrance").position;
                            observableMine.GetComponent<MineBehaviour>().TimesInteracted++;
                            var dwarvesInTheMine = observableMine.GetComponent<MineBehaviour>().DwarvesInside;
                            var thirstyDwarves =
                                dwarvesInTheMine.Count(d => d.GetComponent<DwarfMemory>().ThirstSatisfaction <
                                                            GE.Variables.H.thirstyDwarvesGaugeLimit);
                            var ore = observableMine.GetComponent<MineBehaviour>().Ore;

                            memory.UpdateMine(minePosition, dwarvesInTheMine.Count, thirstyDwarves, ore,
                                Time.time,
                                observableMine.name);
                        }
                        UpdateActivityAndDestination();
                        break;

                    #endregion

                    case ActivitiesLabel.Vigile:
                        #region Vigile

                        var deviantsInSight = DwarvesInSight()
                            .Where(d => d.GetComponent<DwarfMemory>().CurrentActivity ==
                                        ActivitiesLabel.Deviant)
                            .ToList();
                        if (deviantsInSight.Any()) // if the Vigile sees a deviant dwarf

                        {
                            var targetedDeviant = deviantsInSight.First();
                            animator.SetFloat("Run", 1);
                            MoveTo(targetedDeviant.transform.position);
                        }
                        else if (memory.KnownDwarves.Any(d => d.Deviant))
                        {
                            foreach (var Dwarf in memory.KnownDwarves)
                                if (Dwarf.Deviant)
                                {
                                    MoveTo(Dwarf.DwarfPosition);
                                    break;
                                }
                        }
                        else
                        {
                            UpdateActivityAndDestination();
                        }
                        break;

                    #endregion

                    case ActivitiesLabel.Supply:
                        #region Supply
                        List<GameObject> ThirstyDwarves = DwarvesInSight().Where(d => d.GetComponent<DwarfMemory>().ThirstSatisfaction < 50).ToList();
                        if (memory.BeerCarried < 20)
                        {
                            MoveTo(GE.Variables.beerPosition);
                        }
                        else if (ThirstyDwarves.Any())
                        {
                            GameObject ThirstyDwarf = ThirstyDwarves.First();
                            if (Vector3.Distance(transform.position, ThirstyDwarf.transform.position) < 2) // if he reached him
                            {
                                animator.SetFloat("Run", 0);
                                if (agent.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Supply &&
                                ThirstyDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction < 50
                                && agent.GetComponent<DwarfMemory>().BeerCarried > 0)
                                {
                                    var carriedBeer = agent.GetComponent<DwarfMemory>().BeerCarried;
                                    var missingBeer = GE.Variables.maxValueGauge - ThirstyDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction;
                                    var usedBeer = (carriedBeer >= missingBeer)
                                        ? missingBeer
                                        : carriedBeer;

                                    ThirstyDwarf.GetComponent<DwarfMemory>().IncreaseBy(GaugesLabel.ThirstSatisfaction, usedBeer);
                                    agent.GetComponent<DwarfMemory>().LowerBy(GaugesLabel.BeerCarried, usedBeer);
                                    GE.Variables.TotalBeerDrank += usedBeer;
                                    agent.GetComponent<DwarfMemory>().BeerGiven += usedBeer;
                                    ThirstyDwarf.GetComponent<DwarfMemory>().BeerDrank += usedBeer;
                                }
                            }
                            else
                            {
                                animator.SetFloat("Run", 1);
                                MoveTo(ThirstyDwarf.transform.position);
                            }
                        }
                        else
                        {
                            UpdateActivityAndDestination();
                        }
                        break;
                    #endregion

                    case ActivitiesLabel.Miner:
                        #region Miner

                        var mine = GE
                            .GetComponent<GameEnvironment>()
                            .GetMines()
                            .Where(m => Vector3.Distance(
                                            agent.transform.position,
                                            m.transform.FindChild("MineEntrance").position) < 3
                            ).ToList();
                        if (mine.Any())
                            EnterMine(mine.First());
                        
                        else
                            UpdateActivityAndDestination();
                        break;

                    #endregion

                    default: break;
                }
                #endregion / <-- UPDATE - E - 3 - switch (memory.CurrentActivity)
            }

            #endregion / <-- UPDATE - E - Agent arrived destination

        } // <-- public void Update()

        public void UpdateActivityAndDestination()
        {
            if (memory.RethinkActivity() || !agent.hasPath)
            {
                MoveTo(memory.GetNewDestination());
            }
        }

        public void FirstMove()
        {
            var desti = memory.GetNewDestination();
            MoveTo(desti);
        }

        private void MoveTo(Vector3 pos)
        {
            if (memory.OccupiedMine)
                ExitMine();
            agent.SetDestination(pos);
            animator.SetFloat("Walk", 1);
            animator.SetFloat("Run", 0);
        }

        private void EnterMine(GameObject mine)
        {
            var minebehaviour = mine.GetComponent<MineBehaviour>();
            UpdateMineInfo(minebehaviour);
            minebehaviour.TimesInteracted++;
            minebehaviour.GetComponent<MineBehaviour>().AddDwarfInside(this.gameObject);
            memory.OccupiedMine = mine;
            this.gameObject.SetActive(false);
        }

        public void ExitMine()
        {
            var mine = memory.OccupiedMine.GetComponent<MineBehaviour>();
            mine.RemoveDwarfInside(this.gameObject);
            UpdateMineInfo(mine);
            memory.OccupiedMine = null;
            this.gameObject.SetActive(true);
        }

        private void UpdateMineInfo(MineBehaviour mine)
        {
            var minePos = mine.transform.FindChild("MineEntrance").position;
            var dim = mine.DwarvesInside.Count;
            var ore = mine.Ore;
            var mineName = mine.gameObject.name;

            // we count thirsty dwarves in the mine
            var td = mine.DwarvesInside
                .Count(
                    dwarf => dwarf.GetComponent<DwarfMemory>().ThirstSatisfaction
                                < GE.Variables.H.thirstyDwarvesGaugeLimit);

            memory.UpdateMine(minePos, td, dim, ore, Time.time, mineName);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Road"))
            {
                animator.SetFloat("Run", 1);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Road"))
            {
                animator.SetFloat("Run", 0);
            }
        }

        public List<GameObject> DwarvesInSight()
        {
            var dwarvesInSight = new List<GameObject>();
            
            foreach (var dwarf in GE.Variables.Dwarves)
            {
                var sd = GE.Variables.H.SightDistance;

                // if the dwarf happens to be me, myself or I : ignore
                if (dwarf == gameObject ) { continue; }

                // if the dwarf is too far away : ignore
                if (Vector3.Distance(transform.position, dwarf.transform.position) > sd)
                { continue ; }

                var eyesPosition = new Vector3(transform.position.x, transform.position.y + 2,
                    transform.position.z);
                ray = new Ray(eyesPosition, dwarf.transform.position - eyesPosition);
                if (Physics.Raycast(ray, out hit, sd) && hit.collider.gameObject == dwarf)
                    dwarvesInSight.Add(dwarf);
            }
            return dwarvesInSight;
        } // <-- public List<GameObject> DwarvesInSight()

        public List<GameObject> MinesInSight()
        {
            var minesInSight = new List<GameObject>();
            foreach (var mine in GE.Variables.Mines)
            {
                var sd = GE.Variables.H.SightDistance;

                // if the mine is too far away : ignore
                if (Vector3.Distance(transform.position, mine.transform.position) > sd)
                { continue ; }
                
                var eyesPosition = new Vector3(transform.position.x, transform.position.y + 2,
                    transform.position.z);
                ray = new Ray(eyesPosition, mine.transform.position - eyesPosition);
                if (Physics.Raycast(ray, out hit, sd) && hit.collider.CompareTag("Mine"))
                    minesInSight.Add(mine);
            }
            return minesInSight;
        }
    }
}
