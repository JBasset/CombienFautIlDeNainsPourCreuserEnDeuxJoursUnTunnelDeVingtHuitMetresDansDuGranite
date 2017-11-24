using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    private Camera cam;
    private float minDownScroll;
    private Ray ray;
    private RaycastHit hit;
    private Collider lockedObject;

    void Start()
    {
        cam = GetComponent<Camera>();
        minDownScroll = 0;
    }

    void Update()
    {
        MoveCamera();

        Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mouseRay, out hit) && hit.collider.CompareTag("Agent"))
            {
                lockedObject = hit.collider; // on clicking on an agent, we set it as the camera lock
                lockedObject.transform.GetChild(0).gameObject.SetActive(true);
            }
            else if (lockedObject)
            {
                lockedObject.transform.GetChild(0).gameObject.SetActive(false);
                lockedObject = null; // if the click is not on an agent, the camera unlocks
            }
        }
        if (lockedObject) LockCamera(lockedObject);
    }

    private void MoveCamera()
    {
        if (Physics.Raycast(cam.transform.position, Vector3.down, out hit, 10)) // casts a ray from the camera towards the ground, at a max distance of 10 units and return the caracteristics of the ray in "hit". True if the ray hits
        {
            if (hit.distance < 10) // if the collided object is too close...
            {
                cam.transform.position = new Vector3
                    (cam.transform.position.x,
                    cam.transform.position.y + (10 - hit.distance), // ... the camera position is set to 10 unit above it.
                    cam.transform.position.z);
            }
            minDownScroll = cam.transform.position.y;
        }
        else
            minDownScroll = 0;

        // moving the camera back and forth
        if (Input.mousePosition.y < 0)
            cam.transform.position += Vector3.back;
        else if (Input.mousePosition.y > Screen.height)
            cam.transform.position += Vector3.forward;

        // moving the camera right and left
        if (Input.mousePosition.x < 0)
            cam.transform.position += Vector3.left;
        else if (Input.mousePosition.x > Screen.width)
            cam.transform.position += Vector3.right;

        // moving the camera up and down
        cam.transform.position += new Vector3(0, -(Input.mouseScrollDelta.y * 2), 0);

        // we clamp the camera position above the "world", and limit the down scroll to 10 units from the ground
        cam.transform.position = new Vector3
        (
            Mathf.Clamp(cam.transform.position.x, 0, 500),
            Mathf.Clamp(cam.transform.position.y, minDownScroll, 250),
            Mathf.Clamp(cam.transform.position.z, 0, 500)
        );
    }

    private void LockCamera(Collider agent)
    {
        cam.transform.position = new Vector3
            (agent.transform.position.x,
            cam.transform.position.y,
            agent.transform.position.z - 0.7f*(cam.transform.position.y - agent.transform.position.y)); // centers the camera on the target
    }
}
