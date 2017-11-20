using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    private Camera cam;
    private float minDownScroll;
    private Ray ray;
    private RaycastHit hit;

    void Start()
    {
        cam = GetComponent<Camera>();
        minDownScroll = 0;
    }

    void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        if (Physics.Raycast(cam.transform.position, Vector3.down, out hit, 10))
            minDownScroll = cam.transform.position.y;
        else
            minDownScroll = 0;

        if (Physics.Raycast(cam.transform.position, Vector3.forward, out hit, 10))
        {
            minDownScroll += 10;
            Debug.Log("HAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        }

        if (Input.mousePosition.y < 0)
            cam.transform.position += Vector3.back;
        else if (Input.mousePosition.y > Screen.height)
            cam.transform.position += Vector3.forward;

        if (Input.mousePosition.x < 0)
            cam.transform.position += Vector3.left;
        else if (Input.mousePosition.x > Screen.width)
            cam.transform.position += Vector3.right;

        cam.transform.position += new Vector3(0, Input.mouseScrollDelta.y * 2, 0);

        cam.transform.position = new Vector3
        (
            Mathf.Clamp(cam.transform.position.x, 0, 500),
            Mathf.Clamp(cam.transform.position.y, minDownScroll, 250),
            Mathf.Clamp(cam.transform.position.z, 0, 500)
        );
    }
}
