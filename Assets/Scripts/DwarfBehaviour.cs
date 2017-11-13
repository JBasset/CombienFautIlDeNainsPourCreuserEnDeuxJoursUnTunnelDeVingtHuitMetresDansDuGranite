using UnityEngine;
using System.Collections;

public class DwarfBehaviour : MonoBehaviour {

    public Transform Target;

    private NavMeshAgent agent;
    private float normalSpeed;
    private float roadSpeed;

	void Start ()
    {
        agent = GetComponent<NavMeshAgent>();

        normalSpeed = agent.speed;
        roadSpeed = normalSpeed * 2;
	}
	
	void Update ()
    {
        agent.SetDestination(Target.position);
	}

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Road"))
        {
            agent.speed = roadSpeed;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Road"))
        {
            agent.speed = normalSpeed;
        }
    }
}
