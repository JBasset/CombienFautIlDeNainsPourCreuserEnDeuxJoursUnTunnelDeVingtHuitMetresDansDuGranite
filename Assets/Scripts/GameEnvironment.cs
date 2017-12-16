using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class GameEnvironment : MonoBehaviour
    {
        public VariableStorage Variables;
        public GameObject DwarfPrefab;

        private Transform gameEnvironment;
        private Transform dwarves;
        private Transform mines;
        private Vector3 dwarvesSpawn;
        private int spawnsLeft; // number of dwarves to create

        void Start()
        {
            Variables = new VariableStorage();
            gameEnvironment = GetComponent<Transform>();
            dwarves = gameEnvironment.FindChild("Dwarves");
            mines = gameEnvironment.FindChild("World").FindChild("Mines");
            dwarvesSpawn = new Vector3(212, 1.2f, 250); // center of the village
            spawnsLeft = 1;

            UpdateNoticeables();
        }

        void Update()
        {
            if (spawnsLeft > 0 && IsSpawnFree())
            {
                InstantiateDwarf();
                spawnsLeft--;
            }
        }

        public void CreateDwarf(int quantity)
        {
            spawnsLeft += quantity; // we add a dwarf to the list of dwarves left to create
        }

        private bool IsSpawnFree()
        {
            foreach (GameObject Dwarf in Variables.Dwarves)
                if (Vector3.Distance(Dwarf.transform.position, dwarvesSpawn) <= 2) // if any dwarf is under 2 units from the spawn
                    return false;
            return true;
        }

        private void InstantiateDwarf()
        {
            GameObject newDwarf = Instantiate(DwarfPrefab, dwarvesSpawn, new Quaternion(0, 0, 0, 0)) as GameObject;
            newDwarf.transform.SetParent(dwarves);
            UpdateDwarves();
            newDwarf.name = "Dwarf n°" + Variables.Dwarves.Count;
        }

        public List<GameObject> GetDwarves()
        {
            UpdateDwarves();
            return Variables.Dwarves;
        }

        private void UpdateDwarves()
        {
            Variables.Dwarves = new List<GameObject> { };
            for (int i = 0; i < dwarves.childCount; i++)
                Variables.Dwarves.Add(dwarves.GetChild(i).gameObject);
        }

        private void UpdateMines()
        {
            Variables.Mines = new List<GameObject> { };
            for (int i = 0; i < mines.childCount; i++)
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
