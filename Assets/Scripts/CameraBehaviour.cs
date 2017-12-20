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
        public GameObject MineInfoPanel;
        public GameEnvironment gameEnvironment;
        public GameObject UI;

        private Camera cam;
        private float minDownScroll;
        private bool rotating;
        private Quaternion lockedCameraRotation;
        private Vector3 initialRotationPosition;
        private Ray ray;
        private RaycastHit hit;
        private Collider lockedAgent;
        private Collider lockedMine;
        private int lastSecond;

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

        #region MineInfoPanel fields
        private Text mineName;
        private Text goldOre;
        private Text dwarvesInside;
        private Text oreExtracted;
        private Text timesInteracted; // Correspond to the number of dwarves interactions with the mine since the start : finding, entering, supplying...
        #endregion

        void Start()
        {
            cam = GetComponent<Camera>();
            minDownScroll = 0;
            rotating = false;
            lockedCameraRotation = cam.transform.rotation;

            #region DwarfInfoPanel fields

            var generalInfo = DwarfInfoPanel.transform.FindChild("GeneralInfo");
            var known = generalInfo.FindChild("Known");
            var gauges = generalInfo.FindChild("Gauges");
            var stats = DwarfInfoPanel.transform.FindChild("Stats");
            var beer = stats.FindChild("Beer");
            var time = stats.FindChild("Time");
            dwarfName = generalInfo.FindChild("DwarfName").GetComponent<Text>();
            dwarfActivity = generalInfo.FindChild("DwarfActivity").GetComponent<Text>();
            knownDwarves = known.FindChild("Dwarves").FindChild("Value").GetComponent<Text>();
            knownMines = known.FindChild("Mines").FindChild("Value").GetComponent<Text>();
            beerCarried = generalInfo.FindChild("BeerCarried").FindChild("Slider").GetComponent<Slider>();
            thirstSatisfaction = gauges.FindChild("ThirstSatisfaction").FindChild("Slider").GetComponent<Slider>();
            workDesire = gauges.FindChild("WorkDesire").FindChild("Slider").GetComponent<Slider>();
            pickaxe = gauges.FindChild("Pickaxe").FindChild("Slider").GetComponent<Slider>();
            oreMined = stats.FindChild("GoldOreMined").FindChild("Value").GetComponent<Text>();
            beerDrank = beer.FindChild("BeerDrank").FindChild("Value").GetComponent<Text>();
            beerGiven = beer.FindChild("BeerGiven").FindChild("Value").GetComponent<Text>();
            deviantsStopped = stats.FindChild("DeviantsStopped").FindChild("Value").GetComponent<Text>();
            timeAsMiner = time.FindChild("Miner").FindChild("Value").GetComponent<Text>();
            timeAsSupply = time.FindChild("Supply").FindChild("Value").GetComponent<Text>();
            timeAsExplorer = time.FindChild("Explorer").FindChild("Value").GetComponent<Text>();
            timeAsVigile = time.FindChild("Vigile").FindChild("Value").GetComponent<Text>();
            timeAsDeviant = time.FindChild("Deviant").FindChild("Value").GetComponent<Text>();

            #endregion

            #region MineInfoPanel fields

            generalInfo = MineInfoPanel.transform.FindChild("GeneralInfo");
            mineName = generalInfo.FindChild("MineName").GetComponent<Text>();
            goldOre = generalInfo.FindChild("GoldOre").FindChild("Value").GetComponent<Text>();
            dwarvesInside = generalInfo.FindChild("DwarvesInside").FindChild("Value").GetComponent<Text>();
            stats = MineInfoPanel.transform.FindChild("Stats");
            oreExtracted = stats.FindChild("OreExtracted").FindChild("Value").GetComponent<Text>();
            timesInteracted = stats.FindChild("TimesInteracted").FindChild("Value").GetComponent<Text>();

            #endregion
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                rotating = true;
                initialRotationPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                rotating = false;
            }

            if (rotating)
                RotateCamera();
            else
                MoveCamera();

            if (Input.GetMouseButtonDown(0))
            {
                ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                    if (hit.collider.CompareTag("Agent"))
                    {
                        lockedAgent = hit.collider; // on clicking on an agent, we set it as the camera lock
                        lockedMine = null;
                    }
                    else if (hit.collider.CompareTag("Mine"))
                    {
                        lockedMine = hit.collider;
                        lockedAgent = null;
                        DeactivateSpheres();
                    }
                    else if ((lockedAgent || lockedMine) &&
                             !EventSystem.current.IsPointerOverGameObject()) // TODO add the lockedMine
                    {
                        lockedAgent =
                            null; // if the click is not on an agent and not on an UI element, the camera unlocks
                        lockedMine = null;
                        DeactivateSpheres();
                        DwarfInfoPanel.SetActive(false);
                        MineInfoPanel.SetActive(false);
                    }
            }

            if (lockedAgent)
            {
                LockCamera(lockedAgent);
                UpdateDwarfInfoPanel();
            }
            else if (lockedMine)
            {
                LockCamera(lockedMine);
                UpdateMineInfoPanel();
            }

            if (Time.time - lastSecond >= 1)
            {
                lastSecond = (int)Mathf.Floor(Time.time);
                if (lockedMine)
                    UI.GetComponent<UIBehaviour>().SetDwarfInMineButtons(lockedMine.GetComponent<MineBehaviour>());
            }
        }

        private void RotateCamera()
        {
            cam.transform.Rotate(0, -(initialRotationPosition.x - Input.mousePosition.x) / 100, 0, Space.World);
        }

        private void MoveCamera()
        {
            if (Physics.Raycast(cam.transform.position, Vector3.down, out hit, 10)
            ) /* casts a ray from the camera towards the ground, at a max distance of 
                10 units and return the caracteristics of the ray in "hit". True if the ray hits */
            {
                if (hit.distance < 10) // if the collided object is too close...
                {
                    cam.transform.position = new Vector3
                    (cam.transform.position.x,
                        cam.transform.position.y +
                        (10 - hit.distance), // ... the camera position is set to 10 unit above it.
                        cam.transform.position.z);
                }
                minDownScroll = cam.transform.position.y;
            }
            else
            {
                minDownScroll = 0;
            }

            // moving the camera back and forth
            if (Input.mousePosition.y < 0)
                cam.transform.Translate(0, -(2f / 3f), -(1f / 3f), Space.Self);
            else if (Input.mousePosition.y > Screen.height)
                cam.transform.Translate(0, 2f / 3f, 1f / 3f, Space.Self);

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

        public void LockCamera(Collider col)
        {
            cam.transform.rotation = lockedCameraRotation;
            cam.transform.position = new Vector3
            (col.transform.position.x,
                cam.transform.position.y,
                col.transform.position.z -
                0.7f * (cam.transform.position.y - col.transform.position.y)); // centers the camera on the target

            if (col.CompareTag("Agent"))

                #region Agent

            {
                lockedAgent = col;
                DeactivateSpheres();
                var Sphere = col.gameObject.transform.FindChild("Sphere").gameObject;
                Sphere.SetActive(true);

                switch (col.GetComponent<DwarfMemory>().CurrentActivity)
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
                    case VariableStorage.ActivitiesLabel.GoToForge:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.grey;
                        break;
                    }
                    default:
                    {
                        Sphere.GetComponent<Renderer>().material.color = Color.grey;
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

                DwarfInfoPanel.SetActive(true);
                MineInfoPanel.SetActive(false);
            }

            #endregion

            else if (col.CompareTag("Mine"))
            {
                MineInfoPanel.SetActive(true);
                DwarfInfoPanel.SetActive(false);
            }
        }

        private void UpdateDwarfInfoPanel()
        {
            var memory = lockedAgent.GetComponent<DwarfMemory>();

            #region GeneralInfo

            dwarfName.text = lockedAgent.name;
            dwarfActivity.text = memory.CurrentActivity.ToString();

            knownDwarves.text = "" + memory.KnownDwarves.Count;
            knownMines.text = "" + memory.KnownMines.Count;

            var maxValueGauge = gameEnvironment.Variables.maxValueGauge;
            thirstSatisfaction.value = memory.ThirstSatisfaction / (float)maxValueGauge;
            workDesire.value = memory.WorkDesire / (float)maxValueGauge;
            pickaxe.value = memory.Pickaxe / (float)maxValueGauge;
            beerCarried.value = memory.BeerCarried / (float)maxValueGauge;

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

        private void UpdateMineInfoPanel()
        {
            var mine = lockedMine.GetComponent<MineBehaviour>();

            //general info
            mineName.text = lockedMine.name;
            goldOre.text = "" + mine.Ore;
            dwarvesInside.text = "" + mine.DwarvesInside.Count;

            // stats
            oreExtracted.text = "" + mine.OreExtracted;
            timesInteracted.text = "" + mine.TimesInteracted;
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