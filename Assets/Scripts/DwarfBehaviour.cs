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

        private float normalSpeed;
        
        public void Start()
        {
            Debug.Log(" fedzsudsjid \r hfqugbrqzFUPEBHGQNZOZB Qhbreofijazhfe heeellloooooooooo");
            memory = GetComponent<DwarfMemory>();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        public void Update()
        {
            if (Input.GetButtonDown("Enter"))
                MoveTo(Target.position);
            
            if (agent.hasPath && Vector3.Distance(agent.destination, agent.transform.position) < 0.1f)
            {
                agent.ResetPath();
                animator.SetFloat("Walk", 0); //when the agent reaches his destination he stops

                switch (memory.CurrentActivity)
                {
                    case ActivitiesLabel.Deviant: UpdateActivityAndDestination(); break;
                    case ActivitiesLabel.Explorer: UpdateActivityAndDestination(); break;
                    case ActivitiesLabel.Vigile:
                        // TODO if reach target (vigile) do da thing
                        break;
                    case ActivitiesLabel.Supply:
                        // TODO if reach target (soifard) do da thing
                        // TODO j'allais juste dans une mine : normalement j'ai repéré des soifards ==> UpdateActivityAndDestination();
                        break;
                    case ActivitiesLabel.Miner:
                    {
                        foreach (GameObject mine in GE.GetComponent<GameEnvironment>().GetMines())
                        {
                            if (Vector3.Distance(mine.transform.FindChild("MineEntrance").position, agent.transform.position) < 0.1f)
                                EnterMine(mine);
                        }
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
            Debug.Log("###  entering void UpdateActivityAndDestination()");
            if (memory.RethinkActivity())
            {
                MoveTo(memory.GetNewDestination());
            }

            // TODO : il faut que les jauges bougent, que la pioche s'abîme ou se répare
            // TODO : ET QUE LES NAINS INTERAGISSENT QUAND ILS SE CROISENT
        }

        public void FirstMove()
        {
            Debug.Log("###  entering void FirstMove()");
            Debug.Log("### memory exists : pickaxe is " + memory.Pickaxe);
            Debug.Log(" ======== AAAAAND memory.GetNewDestination() ===========");
            var desti = memory.GetNewDestination();
            Debug.Log("desti = "+ desti);
            MoveTo(desti);
        }

        private void MoveTo(Vector3 pos)
        {
            Debug.Log("###  MoveTo("+ pos + ")");
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
