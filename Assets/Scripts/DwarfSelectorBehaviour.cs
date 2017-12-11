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

        private Canvas thisCanvas;

        void Start()
        {
            thisCanvas = GetComponent<Canvas>();
        }

        public void SetDwarfButtons()
        {
            List<GameObject> Dwarves = GE.GetComponent<GameEnvironment>().GetDwarves();
            for (int i = 0; i < Dwarves.Count; i++)
            {
                Button newButton = Instantiate(DwarfButton);
                newButton.transform.SetParent(thisCanvas.transform, false);
                newButton.transform.localPosition = new Vector3(350, 200 - (i*40), 0);

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
            for (int i = 0; i < thisCanvas.transform.childCount; i++)
                if (thisCanvas.transform.GetChild(i).CompareTag("DwarfButton"))
                    Destroy(thisCanvas.transform.GetChild(i).gameObject);
        }
    }
}
