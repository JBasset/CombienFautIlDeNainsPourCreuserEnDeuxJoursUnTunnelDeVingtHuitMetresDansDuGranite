using UnityEngine;
using System;
using System.Collections;
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
                agent.ResetPath();
                animator.SetFloat("Walk", 0); //when the agent reaches his destination he stops

                if (memory.CurrentActivity == ActivitiesLabel.Miner)
                {
                    foreach (GameObject mine in GE.GetComponent<GameEnvironment>().GetMines())
                    {
                        if (Vector3.Distance(mine.transform.FindChild("MineEntrance").position, agent.transform.position) < 0.1f)
                            EnterMine(mine);
                    }
                }
            }
        }

        public void UpdateActivityAndPosition()
        {
            if (memory.RethinkActivity())
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
    }
}
