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
        
        void Start()
        {
            memory = GetComponent<DwarfMemory>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
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
