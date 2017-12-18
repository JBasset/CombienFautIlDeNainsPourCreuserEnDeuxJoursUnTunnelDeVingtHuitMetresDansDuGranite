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
        public Transform Target;
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
            #region  whenever a dwarf is close from a mine, he learns about it

            foreach (var observableMine in GE.Variables.Mines)
            {
                var minePosition = observableMine.transform.FindChild("MineEntrance").position;
                if (Vector3.Distance(agent.transform.position, minePosition) < 0.3f) // maybe change 0.3f ? TODO hahaha
                {
                    var dwarvesInTheMine = observableMine.GetComponent<MineBehaviour>().DwarvesInside;
                    var thirstyDwarves =
                        dwarvesInTheMine.Count(d => d.GetComponent<DwarfMemory>().ThirstSatisfaction <
                                                    GE.Variables.thirstyDwarvesGaugeLimit);
                    var ore = observableMine.GetComponent<MineBehaviour>().Ore;

                    memory.UpdateMine(minePosition, dwarvesInTheMine.Count, thirstyDwarves, ore, DateTime.Now,
                        observableMine.name);
                }
            }

            #endregion

            // TODO penser à débugger la position des nains

            #region  whenever a dwarf is close from another

            // whatever you're doing : stop
            if (DwarvesInSight().Any())
            {
                switch (memory.CurrentActivity)
                {
                    case ActivitiesLabel.Explorer:
                        foreach (var myD in DwarvesInSight())
                        {
                            if (memory.KnownDwarves.All(kd => kd.Name != myD.name))
                            {
                                MoveTo(myD.transform.position);
                                UpdateActivityAndDestination();
                                break;
                            }
                        }
                        break;
                    case ActivitiesLabel.Deviant:
                        break;
                    case ActivitiesLabel.Vigile:
                        foreach (var myD in DwarvesInSight())
                        {
                            // si il est déviant
                        }
                        break;
                    case ActivitiesLabel.Supply:
                        foreach (var myD in DwarvesInSight())
                        {
                            // si il a soif
                        }
                        break;
                    case ActivitiesLabel.Miner:
                        foreach (var myD in DwarvesInSight())
                        {
                            if (memory.KnownDwarves.All(kd => kd.Name != myD.name))
                            {
                                MoveTo(myD.transform.position);
                                UpdateActivityAndDestination();
                                break;
                            }
                        }
                        break;
                    case ActivitiesLabel.GoToForge:
                        break;
                }
            }

            #endregion

            if (DwarvesInSight().Any())
            {

                #region  whenever a dwarf is REALLY close from another, he learns from him

                foreach (var seenDwarf in DwarvesInSight())
                {
                    if (Vector3.Distance(agent.transform.position, seenDwarf.transform.position) <
                        3f) // maybe change ? TODO hahaha
                    {
                        memory.UpdateMemory(seenDwarf.GetComponent<DwarfMemory>().KnownDwarves,
                            seenDwarf.GetComponent<DwarfMemory>().KnownMines);
                        memory.UpdateDwarf(seenDwarf.GetComponent<DwarfMemory>(), DateTime.Now,
                            seenDwarf.transform.position);
                    }
                }

                #endregion


                if (Time.time - LastSecond >= 1)
                {
                    LastSecond = (int) Mathf.Floor(Time.time);
                    if (memory.CurrentActivity == ActivitiesLabel.Miner) memory.TimeAsMiner += 1;
                    else if (memory.CurrentActivity == ActivitiesLabel.Supply) memory.TimeAsSupply += 1;
                    else if (memory.CurrentActivity == ActivitiesLabel.Explorer) memory.TimeAsExplorer += 1;
                    else if (memory.CurrentActivity == ActivitiesLabel.Vigile) memory.TimeAsVigile += 1;
                    else if (memory.CurrentActivity == ActivitiesLabel.Deviant) memory.TimeAsDeviant += 1;
                }

                if (Input.GetButtonDown("Enter"))
                    MoveTo(Target.position);


                if (agent.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Explorer)
                {
                    // if there is any mine in sight
                    if (MinesInSight().Any())
                    {
                        foreach (var myMine in MinesInSight())
                        {
                            if (memory.KnownMines.All(kmine => kmine.Name != myMine.name))
                            {
                                MoveTo(myMine.transform.FindChild("MineEntrance").position);
                                UpdateActivityAndDestination();
                                break;
                            }
                        }
                    }
                }

                if (agent.hasPath && Vector3.Distance(agent.destination, agent.transform.position) < 2)
                {
                    {
                        if (Vector3.Distance(agent.destination, GE.Variables.beerPosition) < 0.1f)
                        {
                            memory.IncreaseBy(GaugesLabel.ThirstSatisfaction, GE.Variables.maxValueGauge);
                        }

                        if (Vector3.Distance(agent.destination, GE.Variables.forgePosition) < 0.1f)
                        {
                            memory.IncreaseBy(GaugesLabel.Pickaxe, GE.Variables.maxValueGauge);
                        }

                        agent.ResetPath();
                        animator.SetFloat("Walk", 0); //when the agent reaches his destination he stops
                        animator.SetFloat("Run", 0);

                        switch (memory.CurrentActivity)
                        {
                            case ActivitiesLabel.Deviant:
                                UpdateActivityAndDestination();
                                break;

                            case ActivitiesLabel.Explorer:
                                UpdateActivityAndDestination();
                                break;

                            case ActivitiesLabel.Vigile:

                                #region Vigile

                                List<GameObject> deviantsInSight = DwarvesInSight()
                                    .Where(d => d.GetComponent<DwarfMemory>().CurrentActivity ==
                                                ActivitiesLabel.Deviant)
                                    .ToList();
                                if (deviantsInSight.Any()) // if the Vigile sees a deviant dwarf

                                {
                                    GameObject TargetedDeviant = deviantsInSight[0];
                                    if (Vector3.Distance(this.transform.position,
                                            TargetedDeviant.transform.position) < 2) // if he reached him
                                    {
                                        TargetedDeviant.GetComponent<DwarfMemory>()
                                            .IncreaseBy(GaugesLabel.Workdesire, GE.Variables.maxValueGauge);
                                        memory.DeviantsStopped++;
                                        TargetedDeviant.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                                        UpdateActivityAndDestination();
                                    }
                                    else
                                    {
                                        animator.SetFloat("Run", 1);
                                        MoveTo(TargetedDeviant.transform.position);
                                    }
                                }
                                else if (memory.KnownDwarves.Any(d => d.Deviant))
                                {
                                    foreach (DwarfMemory._KnownDwarf Dwarf in memory.KnownDwarves)
                                    {
                                        if (Dwarf.Deviant)
                                        {
                                            MoveTo(Dwarf.DwarfPosition);
                                            break;
                                        }
                                    }
                                }
                                else
                                    UpdateActivityAndDestination();
                                break;

                            #endregion

                            case ActivitiesLabel.Supply:
                                // TODO if reach target (soifard) do da thing
                                // TODO j'allais juste dans une mine : normalement j'ai repéré des soifards ==> UpdateActivityAndDestination();
                                break;

                            case ActivitiesLabel.Miner:

                                #region Miner

                                List<GameObject> mine = GE
                                    .GetComponent<GameEnvironment>()
                                    .GetMines()
                                    .Where(m => Vector3.Distance(
                                                    agent.transform.position,
                                                    m.transform.FindChild("MineEntrance").position) < 0.1f
                                    ).ToList();
                                if (mine.Any())
                                    EnterMine(mine[0]);
                                else if (memory.KnownMines.Any(m => m.Ore > GE.Variables.dwarfOreMiningRate * 10))
                                {
                                    foreach (DwarfMemory._KnownMine Mine in memory.KnownMines)
                                    {
                                        if (Mine.Ore > GE.Variables.dwarfOreMiningRate)
                                        {
                                            MoveTo(Mine.MinePosition);
                                            break;
                                        }
                                    }
                                }
                                else
                                    UpdateActivityAndDestination();
                                break;

                            #endregion

                            case ActivitiesLabel.GoToForge:
                                // TODO if gotoforge ENTERFORGE
                                break;

                            default: break;
                        }
                    }
                }
            }
        }

        public void UpdateActivityAndDestination()
        {
            if (memory.RethinkActivity() || !agent.hasPath)
            {
                MoveTo(memory.GetNewDestination());
            }

            // TODO : il faut que les jauges bougent, que la pioche s'abîme ou se répare
            // TODO : ET QUE LES NAINS INTERAGISSENT QUAND ILS SE CROISENT
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
        }

        private void EnterMine(GameObject mine)
        {
            mine.GetComponent<MineBehaviour>().AddDwarfInside(this.gameObject);
            memory.OccupiedMine = mine;
            this.gameObject.SetActive(false);
        }

        private void ExitMine()
        {
            memory.OccupiedMine.GetComponent<MineBehaviour>().RemoveDwarfInside(this.gameObject);
            memory.OccupiedMine = null;
            this.gameObject.SetActive(true);
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
            List<GameObject> dwarvesInSight =  new List<GameObject>();
            foreach (GameObject Dwarf in GE.Variables.Dwarves)
            {
                if (Dwarf != this.gameObject && Vector3.Distance(this.transform.position, Dwarf.transform.position) <= GE.Variables.SightDistance)
                {
                    ray = new Ray(this.transform.position, (Dwarf.transform.position - this.transform.position));
                    if (Physics.Raycast(ray, out hit, GE.Variables.SightDistance) && hit.collider.gameObject == Dwarf)
                    {
                        dwarvesInSight.Add(Dwarf);
                    }
                }
            }
            return dwarvesInSight;
        }

        public List<GameObject> MinesInSight()
        {
            List<GameObject> minesInSight = new List<GameObject>();
            foreach (GameObject Mine in GE.Variables.Mines)
            {
                if (Vector3.Distance(this.transform.position, Mine.transform.position) <= GE.Variables.SightDistance)
                {
                    ray = new Ray(this.transform.position, (Mine.transform.position - this.transform.position));
                    if (Physics.Raycast(ray, out hit, GE.Variables.SightDistance) && hit.collider.gameObject == Mine)
                    {
                        minesInSight.Add(Mine);
                    }
                }
            }
            return minesInSight;
        }
    }
}
