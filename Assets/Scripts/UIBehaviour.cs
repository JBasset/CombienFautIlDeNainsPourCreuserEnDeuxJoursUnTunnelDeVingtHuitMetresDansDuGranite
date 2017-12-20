using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class UIBehaviour : MonoBehaviour
    {
        public Button DwarfButton;
        public GameObject GE;
        public Camera MainCam;
        public GameObject ScrollablePanel;
        public GameObject MineUIScrollablePanel;
        public Transform StartingPanel;
        public GameObject ActivateDwarfSelectionPanelButton;
        public GameObject GeneralStatsPanel;

        private RectTransform _scrollablePanelRectTransform;
        private RectTransform _mineUiScrollablePanelRectTransform;

        void Start()
        {
            _scrollablePanelRectTransform = ScrollablePanel.GetComponent<RectTransform>();
            _mineUiScrollablePanelRectTransform = MineUIScrollablePanel.GetComponent<RectTransform>();
        }

        public void StartSimulation()
        {
            InputField NbDwarves = StartingPanel.FindChild("NbDwarves").FindChild("Value").GetComponent<InputField>();
            InputField OblivionRate = StartingPanel.FindChild("OblivionRate").FindChild("Value").GetComponent<InputField>();
            InputField TimeScale = StartingPanel.FindChild("TimeScale").FindChild("Value").GetComponent<InputField>();

            try { GE.GetComponent<GameEnvironment>()._spawnsLeft = int.Parse(NbDwarves.text); }
            catch { GE.GetComponent<GameEnvironment>()._spawnsLeft = 40; }

            try { GE.GetComponent<GameEnvironment>().Variables.OutOfDate = int.Parse(OblivionRate.text); }
            catch { GE.GetComponent<GameEnvironment>().Variables.OutOfDate = 180; }

            try
            {
                if (int.Parse(TimeScale.text) > 0)
                    Time.timeScale = int.Parse(TimeScale.text);
                else
                    Time.timeScale = 1;
            }
            catch { Time.timeScale = 1; }
            
            GE.GetComponent<GameEnvironment>().StartingTime = (int)Mathf.Floor(Time.time);

            StartingPanel.gameObject.SetActive(false);
            ActivateDwarfSelectionPanelButton.SetActive(true);
            GeneralStatsPanel.SetActive(true);
        }

        public void SetDwarfButtons()
        {
            List<GameObject> Dwarves = GE.GetComponent<GameEnvironment>().GetDwarves();
            _scrollablePanelRectTransform.sizeDelta = new Vector2(130, 50 + (Dwarves.Count-1) * 35);
            for (int i = 0; i < Dwarves.Count; i++)
            {
                Button newButton = Instantiate(DwarfButton);
                newButton.transform.SetParent(ScrollablePanel.transform, false);
                newButton.transform.localPosition = new Vector3
                    (
                        newButton.transform.localPosition.x,
                        newButton.transform.localPosition.y - (i*35),
                        newButton.transform.localPosition.z
                    );
                newButton.transform.FindChild("Text").GetComponent<Text>().text = Dwarves[i].name;

                GameObject Dwarf = Dwarves[i];
                newButton.onClick.AddListener(delegate { LockCamera(Dwarf); });
            }
        }

        public void SetDwarfInMineButtons(MineBehaviour mine)
        {
            List<GameObject> Dwarves = mine.DwarvesInside;

            for (int i = 0; i < MineUIScrollablePanel.transform.childCount; i++)
                Destroy(MineUIScrollablePanel.transform.GetChild(i).gameObject);

            _mineUiScrollablePanelRectTransform.sizeDelta = new Vector2(130, 50 + (Dwarves.Count - 1) * 35);
            for (int i = 0; i < Dwarves.Count; i++)
            {
                Button newButton = Instantiate(DwarfButton);
                newButton.transform.SetParent(MineUIScrollablePanel.transform, false);
                newButton.transform.localPosition = new Vector3
                    (
                        newButton.transform.localPosition.x,
                        newButton.transform.localPosition.y - (i * 35),
                        newButton.transform.localPosition.z
                    );
                newButton.transform.FindChild("Text").GetComponent<Text>().text = Dwarves[i].name;

                GameObject Dwarf = Dwarves[i];
                newButton.onClick.AddListener(delegate { LockCamera(Dwarf); });
            }
        }

        private void LockCamera (GameObject dwarf)
        {
            MainCam.GetComponent<CameraBehaviour>().LockCamera(dwarf.GetComponent<Collider>());
        }

        public void RemoveDwarfButtons()
        {
            for (int i = 0; i < ScrollablePanel.transform.childCount; i++)
                if (ScrollablePanel.transform.GetChild(i).CompareTag("DwarfButton"))
                    Destroy(ScrollablePanel.transform.GetChild(i).gameObject);
        }
    }
}
