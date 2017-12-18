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

            foreach (var observableMine in GE.Variables.Mines)
            {
                var minePosition = observableMine.transform.FindChild("MineEntrance").position;
                if (Vector3.Distance(agent.transform.position, minePosition) < 0.1f)
                {
                    var dwarvesInTheMine = observableMine.GetComponent<MineBehaviour>().dwarvesInside;
                    var thirstyDwarves = dwarvesInTheMine.Count(d => d.GetComponent<DwarfMemory>().ThirstSatisfaction < GE.Variables.thirstyDwarvesGaugeLimit);
                    var ore = observableMine.GetComponent<MineBehaviour>().ore;

                    memory.UpdateMine(minePosition, dwarvesInTheMine.Count, thirstyDwarves, ore, DateTime.Now);
                }
            }

            /*foreach (var myDwarf in GE.Variables.Dwarves)
            {
                foreach (var metDwarf in myDwarf.GetComponent<DwarfBehaviour>().DwarvesInSight())
                {
                    foreach (var knownMine in metDwarf.GetComponent<DwarfMemory>().KnownMines)
                    {
                        myDwarf.GetComponent<DwarfMemory>().UpdateMine(knownMine);
                    }
                }
            }*/

            if (Time.time - LastSecond >= 1)
            {
                LastSecond = (int)Mathf.Floor(Time.time);
                if (memory.CurrentActivity == ActivitiesLabel.Miner) memory.TimeAsMiner += 1;
                else if (memory.CurrentActivity == ActivitiesLabel.Supply) memory.TimeAsSupply += 1;
                else if (memory.CurrentActivity == ActivitiesLabel.Explorer) memory.TimeAsExplorer += 1;
                else if (memory.CurrentActivity == ActivitiesLabel.Vigile) memory.TimeAsVigile += 1;
                else if (memory.CurrentActivity == ActivitiesLabel.Deviant) memory.TimeAsDeviant += 1;
            }

            if (Input.GetButtonDown("Enter"))
                MoveTo(Target.position);
            
            if (agent.hasPath && Vector3.Distance(agent.destination, agent.transform.position) < 0.1f)
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
                    case ActivitiesLabel.Deviant: UpdateActivityAndDestination(); break;

                    case ActivitiesLabel.Explorer: UpdateActivityAndDestination(); break;

                    case ActivitiesLabel.Vigile:
                        List<GameObject> deviantsInSight = DwarvesInSight().Where(d => d.GetComponent<DwarfMemory>().CurrentActivity == ActivitiesLabel.Deviant).ToList();
                        if (deviantsInSight.Any())
                        {
                            GameObject TargetedDeviant = deviantsInSight[0];
                            if (Vector3.Distance(this.transform.position, TargetedDeviant.transform.position) < 0.1f)
                            {
                                TargetedDeviant.GetComponent<DwarfMemory>().IncreaseBy(GaugesLabel.Workdesire, GE.Variables.maxValueGauge);
                                TargetedDeviant.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                                UpdateActivityAndDestination();
                            }
                            else
                            {
                                animator.SetFloat("Run", 1);
                                MoveTo(TargetedDeviant.transform.position);
                            }
                        }
                        // TODO : si un vigile atteint un déviant blah blah
                        // TODO if reach target (vigile) do da thing
                        break;

                    case ActivitiesLabel.Supply:
                        // TODO if reach target (soifard) do da thing
                        // TODO j'allais juste dans une mine : normalement j'ai repéré des soifards ==> UpdateActivityAndDestination();
                        break;

                    case ActivitiesLabel.Miner:
                        List<GameObject> mine = GE.GetComponent<GameEnvironment>().GetMines()
                            .Where(m => Vector3.Distance(
                        agent.transform.position, m.transform.FindChild("MineEntrance").position) < 0.1f
                        ).ToList();
                        if (mine.Any())
                        {
                            EnterMine(mine[0]);
                            // TODO : make sure the dwarf dig ( c'est dans le switch ? )
                        }
                        break;

                    case ActivitiesLabel.GoToForge:
                        // TODO if gotoforge ENTERFORGE
                        break;

                    default: break;
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
