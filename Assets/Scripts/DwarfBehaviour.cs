using UnityEngine;
using System.Collections;

namespace Assets.Scripts
{
    public class DwarfBehaviour : MonoBehaviour
    {
        public Transform Target;
        public GameObject GE;

        private Animator animator;
        private NavMeshAgent agent;
        private float normalSpeed;
        private float roadSpeed;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            normalSpeed = agent.speed;
        }

        void Update()
        {
            if (Input.GetButtonDown("Enter"))
                MoveTo(Target.position);

            if (agent.hasPath && Vector3.Distance(agent.destination, agent.transform.position) < 0.1f)
            {
                agent.ResetPath();
                animator.SetFloat("Walk", 0); //when the agent reaches his destination he stops

                // TODO : IF DWARF IS A MINER
                foreach (GameObject mine in GE.GetComponent<GameEnvironment>().GetMines())
                {
                    if (Vector3.Distance(mine.transform.FindChild("MineEntrance").position, agent.transform.position) < 0.1f)
                        EnterMine(mine);
                }
            }
        }

        private void MoveTo(Vector3 pos)
        {
            agent.SetDestination(pos);
            animator.SetFloat("Walk", 1);
        }

        private void EnterMine(GameObject mine)
        {
            mine.GetComponent<MineBehaviour>().AddDwarfInside(this.gameObject);
            this.gameObject.SetActive(false);
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
