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

        private RectTransform scrollablePanelRectTransform;
        private RectTransform mineUIScrollablePanelRectTransform;

        void Start()
        {
            scrollablePanelRectTransform = ScrollablePanel.GetComponent<RectTransform>();
            mineUIScrollablePanelRectTransform = MineUIScrollablePanel.GetComponent<RectTransform>();
        }

        public void SetDwarfButtons()
        {
            List<GameObject> Dwarves = GE.GetComponent<GameEnvironment>().GetDwarves();
            scrollablePanelRectTransform.sizeDelta = new Vector2(130, 50 + (Dwarves.Count-1) * 35);
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

            mineUIScrollablePanelRectTransform.sizeDelta = new Vector2(130, 50 + (Dwarves.Count - 1) * 35);
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

        private void LockCamera (GameObject Dwarf)
        {
            MainCam.GetComponent<CameraBehaviour>().LockCamera(Dwarf.GetComponent<Collider>());
        }

        public void RemoveDwarfButtons()
        {
            for (int i = 0; i < ScrollablePanel.transform.childCount; i++)
                if (ScrollablePanel.transform.GetChild(i).CompareTag("DwarfButton"))
                    Destroy(ScrollablePanel.transform.GetChild(i).gameObject);
        }
    }
}
