using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class CameraBehaviour : MonoBehaviour
    {
        public GameObject Dwarves;
        public GameObject DwarfInfoPanel;
        public GameEnvironment gameEnvironment;

        private Camera cam;
        private float minDownScroll;
        private bool rotating;
        private Quaternion lockedCameraRotation;
        private Vector3 initialRotationPosition;
        private Ray ray;
        private RaycastHit hit;
        private Collider lockedObject;

        #region DwarfInfoPanel fields
        private Text dwarfName;
        private Text dwarfActivity;
        private Text knownDwarves;
        private Text knownMines;
        private Slider beerCarried;
        private Slider thirstSatisfaction;
        private Slider workDesire;
        private Slider pickaxe;
        private Text oreMined;
        private Text beerDrank;
        private Text beerGiven;
        private Text deviantsStopped;
        private Text timeAsMiner;
        private Text timeAsSupply;
        private Text timeAsExplorer;
        private Text timeAsVigile;
        private Text timeAsDeviant;
        #endregion

        void Start()
        {
            cam = GetComponent<Camera>();
            minDownScroll = 0;
            rotating = false;
            lockedCameraRotation = cam.transform.rotation;

            #region DwarfInfoPanel fields
            Transform GeneralInfo = DwarfInfoPanel.transform.FindChild("GeneralInfo");
            Transform Known = GeneralInfo.FindChild("Known");
            Transform Gauges = GeneralInfo.FindChild("Gauges");
            Transform Stats = DwarfInfoPanel.transform.FindChild("Stats");
            Transform Beer = Stats.FindChild("Beer");
            Transform Time = Stats.FindChild("Time");
            dwarfName = GeneralInfo.FindChild("DwarfName").GetComponent<Text>();
            dwarfActivity = GeneralInfo.FindChild("DwarfActivity").GetComponent<Text>();
            knownDwarves = Known.FindChild("Dwarves").FindChild("Value").GetComponent<Text>();
            knownMines = Known.FindChild("Mines").FindChild("Value").GetComponent<Text>();
            beerCarried = GeneralInfo.FindChild("BeerCarried").FindChild("Slider").GetComponent<Slider>();
            thirstSatisfaction = Gauges.FindChild("ThirstSatisfaction").FindChild("Slider").GetComponent<Slider>();
            workDesire = Gauges.FindChild("WorkDesire").FindChild("Slider").GetComponent<Slider>();
            pickaxe = Gauges.FindChild("Pickaxe").FindChild("Slider").GetComponent<Slider>();
            oreMined = Stats.FindChild("GoldOreMined").FindChild("Value").GetComponent<Text>();
            beerDrank = Beer.FindChild("BeerDrank").FindChild("Value").GetComponent<Text>();
            beerGiven = Beer.FindChild("BeerGiven").FindChild("Value").GetComponent<Text>();
            deviantsStopped = Stats.FindChild("DeviantsStopped").FindChild("Value").GetComponent<Text>();
            timeAsMiner = Time.FindChild("Miner").FindChild("Value").GetComponent<Text>();
            timeAsSupply = Time.FindChild("Supply").FindChild("Value").GetComponent<Text>();
            timeAsExplorer = Time.FindChild("Explorer").FindChild("Value").GetComponent<Text>();
            timeAsVigile = Time.FindChild("Vigile").FindChild("Value").GetComponent<Text>();
            timeAsDeviant = Time.FindChild("Deviant").FindChild("Value").GetComponent<Text>();
            #endregion
        }

    void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                rotating = true;
                initialRotationPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
                rotating = false;

            if (rotating)
                RotateCamera();
            else
                MoveCamera();

            if (Input.GetMouseButtonDown(0))
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("Agent"))
                {
                    lockedObject = hit.collider; // on clicking on an agent, we set it as the camera lock
                }
                else if (lockedObject && !EventSystem.current.IsPointerOverGameObject())
                {
                    lockedObject = null; // if the click is not on an agent and not on an UI element, the camera unlocks
                    DeactivateSpheres();
                    DwarfInfoPanel.SetActive(false);
                }
            }

            if (lockedObject)
            {
                LockCamera(lockedObject);
                UpdateDwarfInfoPanel();
            }
        }

        private void RotateCamera()
        {
            cam.transform.Rotate(0, -(initialRotationPosition.x - Input.mousePosition.x)/100, 0, Space.World);
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
                cam.transform.Translate(0, -(2f / 3f), -(1f / 3f), Space.Self);
            else if (Input.mousePosition.y > Screen.height)
                cam.transform.Translate(0, (2f / 3f), (1f / 3f), Space.Self);

            // moving the camera right and left
            if (Input.mousePosition.x < 0)
                cam.transform.Translate(Vector3.left, Space.Self);
            else if (Input.mousePosition.x > Screen.width)
                cam.transform.Translate(Vector3.right, Space.Self);

            // moving the camera up and down (if the mouse isn't above UI element)
            if (!EventSystem.current.IsPointerOverGameObject())
                cam.transform.position += new Vector3(0, -(Input.mouseScrollDelta.y * 2), 0);

            // we clamp the camera position above the "world", and limit the down scroll to 10 units from the ground
            cam.transform.position = new Vector3
            (
                Mathf.Clamp(cam.transform.position.x, 0, 500),
                Mathf.Clamp(cam.transform.position.y, minDownScroll, 250),
                Mathf.Clamp(cam.transform.position.z, 0, 500)
            );
        }

        public void LockCamera(Collider agent)
        {
            lockedObject = agent;
            cam.transform.rotation = lockedCameraRotation;
            cam.transform.position = new Vector3
                (agent.transform.position.x,
                cam.transform.position.y,
                agent.transform.position.z - 0.7f * (cam.transform.position.y - agent.transform.position.y)); // centers the camera on the target
            DeactivateSpheres();

            #region sphere color
            GameObject Sphere = agent.gameObject.transform.FindChild("Sphere").gameObject;
            Sphere.SetActive(true);

            switch (agent.GetComponent<DwarfMemory>().CurrentActivity)
            {
                case VariableStorage.ActivitiesLabel.Miner:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.yellow;
                        break;
                    }
                case VariableStorage.ActivitiesLabel.Explorer:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.green;
                        break;
                    }
                case VariableStorage.ActivitiesLabel.Supply:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.blue;
                        break;
                    }
                case VariableStorage.ActivitiesLabel.Vigile:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.red;
                        break;
                    }
                case VariableStorage.ActivitiesLabel.Deviant:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.black;
                        break;
                    }
            }
            /*
            Sphere Color :
            - Miner : Yellow
            - Explorer : Green
            - Supply : Blue
            - Vigile : Red
            - Deviant : Black
            */
            #endregion

            DwarfInfoPanel.SetActive(true);
        }

        private void UpdateDwarfInfoPanel()
        {
            DwarfMemory memory = lockedObject.GetComponent<DwarfMemory>();
            #region GeneralInfo
            dwarfName.text = lockedObject.name;
            dwarfActivity.text = memory.CurrentActivity.ToString();

            //TODO : knownDwarves, knownMines, beerCarried.value

            int maxValueGauge = gameEnvironment.Variables.maxValueGauge;
            thirstSatisfaction.value = (float)memory.ThirstSatisfaction / (float)maxValueGauge;
            workDesire.value = (float)memory.WorkDesire / (float)maxValueGauge;
            pickaxe.value = (float)memory.Pickaxe / (float)maxValueGauge;
            #endregion

            #region Stats
            oreMined.text = "" + memory.GoldOreMined;
            beerDrank.text = "" + memory.BeerDrank;
            beerGiven.text = "" + memory.BeerGiven;
            deviantsStopped.text = "" + memory.DeviantsStopped;
            timeAsMiner.text = "" + memory.TimeAsMiner;
            timeAsSupply.text = "" + memory.TimeAsSupply;
            timeAsExplorer.text = "" + memory.TimeAsExplorer;
            timeAsVigile.text = "" + memory.TimeAsVigile;
            timeAsDeviant.text = "" + memory.TimeAsDeviant;
            #endregion
        }

        private void DeactivateSpheres()
        {
            for (int i = 0; i < Dwarves.transform.childCount; i++)
            {
                if (Dwarves.transform.GetChild(i).FindChild("Sphere").gameObject.activeSelf)
                    Dwarves.transform.GetChild(i).FindChild("Sphere").gameObject.SetActive(false);
            }
        }
    }
}
