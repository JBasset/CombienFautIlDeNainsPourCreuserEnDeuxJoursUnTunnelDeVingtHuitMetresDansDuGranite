using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public class GameEnvironment : MonoBehaviour
    {
        public VariableStorage Variables = new VariableStorage();
        public GameObject DwarfPrefab;
        public DwarfSelectorBehaviour UI;

        private Transform gameEnvironment;
        private Transform dwarves;
        private Transform mines;
        private Vector3 dwarvesSpawn;
        private int spawnsLeft; // number of dwarves to create

        private int LastGeneralActivityUpdate;

        void Start()
        {
            gameEnvironment = GetComponent<Transform>();
            dwarves = gameEnvironment.FindChild("Dwarves");
            mines = gameEnvironment.FindChild("World").FindChild("Mines");
            dwarvesSpawn = new Vector3(212, 1.2f, 250); // center of the village
            spawnsLeft = 2;
            LastGeneralActivityUpdate = 0;

            UpdateNoticeables();
        }

        void Update()
        {
            if (spawnsLeft > 0 && IsSpawnFree())
            {
                InstantiateDwarf();
                spawnsLeft--;
            }
            if (Time.time - LastGeneralActivityUpdate >= Variables.activityRethinkChangeRate)
            {
                LastGeneralActivityUpdate = (int)Mathf.Floor(Time.time);
                foreach (var myDwarf in Variables.Dwarves)
                {
                    myDwarf.GetComponent<DwarfBehaviour>().UpdateActivityAndPosition();
                    Debug.Log(
                        myDwarf.name + 
                        " Target :" + myDwarf.GetComponent<DwarfBehaviour>().Target +
                        "\r Pioche :" + myDwarf.GetComponent<DwarfMemory>().Pickaxe +
                        "\r CurrentActivity :" + myDwarf.GetComponent<DwarfMemory>().CurrentActivity);
                }
            }
        }

        public void CreateDwarf(int quantity)
        {
            spawnsLeft += quantity; // we add a dwarf to the list of dwarves left to create
        }

        private bool IsSpawnFree()
        {
            return Variables.Dwarves.All(d => !(Vector3.Distance(d.transform.position, dwarvesSpawn) <= 2));
            // if any dwarf is under 2 units from the spawn, returns false
        }

        private void InstantiateDwarf()
        {
            GameObject newDwarf = Instantiate(DwarfPrefab, dwarvesSpawn, new Quaternion(0, 0, 0, 0)) as GameObject;
            newDwarf.transform.SetParent(dwarves);
            UpdateDwarves();
            newDwarf.name = "Dwarf n°" + Variables.Dwarves.Count;
            newDwarf.GetComponent<DwarfBehaviour>().GE = this;
            newDwarf.GetComponent<DwarfMemory>().GameEnvironment = this;
            newDwarf.GetComponent<DwarfMemory>().DwarfMemoryInitialization();
            Debug.Log(
                "After DwarfMemoryInitialization, " + newDwarf.name + "'s pickaxe is " + newDwarf.GetComponent<DwarfMemory>().Pickaxe);
            Debug.Log("### call +"+ newDwarf + ".GetComponent<DwarfBehaviour>().FirstMove();");
            Debug.Log(newDwarf.GetComponent<DwarfBehaviour>() + " (.GetComponent<DwarfBehaviour>())");

            newDwarf.GetComponent<DwarfBehaviour>().Start();
            newDwarf.GetComponent<DwarfMemory>().Start();

            Debug.Log(newDwarf.GetComponent<DwarfMemory>() + " (.GetComponent<DwarfBehaviour>())");

            UI.SetDwarfButtons();

            newDwarf.GetComponent<DwarfBehaviour>().FirstMove();
        }

        public List<GameObject> GetDwarves()
        {
            UpdateDwarves();
            return Variables.Dwarves;
        }

        public List<GameObject> GetMines()
        {
            UpdateMines();
            return Variables.Mines;
        }

        private void UpdateDwarves()
        {
            Variables.Dwarves = new List<GameObject> { };
            for (var i = 0; i < dwarves.childCount; i++)
                Variables.Dwarves.Add(dwarves.GetChild(i).gameObject);
        }

        private void UpdateMines()
        {
            Variables.Mines = new List<GameObject> { };
            for (var i = 0; i < mines.childCount; i++)
                Variables.Mines.Add(mines.GetChild(i).gameObject);
        }

        private void UpdateNoticeables()
        {
            UpdateDwarves();
            UpdateMines();
            Variables.NoticeableObjects = new List<GameObject> { };
            foreach (GameObject dwarf in Variables.Dwarves)
                Variables.NoticeableObjects.Add(dwarf);
            foreach (GameObject mine in Variables.Mines)
                Variables.NoticeableObjects.Add(mine);
        }
    }
}
