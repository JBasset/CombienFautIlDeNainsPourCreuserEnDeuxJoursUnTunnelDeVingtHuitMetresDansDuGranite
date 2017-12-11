using UnityEngine;
using System.Collections;

namespace Assets.Scripts
{
    public class DwarfBehaviour : MonoBehaviour
    {
        public Transform Target;

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
            }
        }

        private void MoveTo(Vector3 pos)
        {
            agent.SetDestination(pos);
            animator.SetFloat("Walk", 1);
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
