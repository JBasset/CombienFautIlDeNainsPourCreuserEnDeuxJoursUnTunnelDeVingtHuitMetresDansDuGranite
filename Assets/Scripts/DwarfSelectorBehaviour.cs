using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class DwarfSelectorBehaviour : MonoBehaviour
    {
        public Button DwarfButton;
        public GameObject GE;
        public Camera MainCam;
        public GameObject ScrollablePanel;

        private RectTransform scrollablePanelRectTransform;

        void Start()
        {
            scrollablePanelRectTransform = ScrollablePanel.GetComponent<RectTransform>();
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

                GameObject Dwarf = Dwarves[i];
                newButton.onClick.AddListener(delegate { lockCamera(Dwarf); });
            }
        }

        private void lockCamera (GameObject Dwarf)
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
