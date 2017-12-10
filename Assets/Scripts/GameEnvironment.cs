using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class GameEnvironment : MonoBehaviour
    {
        public VariableStorage Variables;

        private Transform gameEnvironment;
        private Transform dwarves;
        private Transform mines;

        void Start()
        {
            Variables = new VariableStorage();
            gameEnvironment = GetComponent<Transform>();
            dwarves = gameEnvironment.FindChild("Dwarves");
            mines = gameEnvironment.FindChild("World").FindChild("Mines");

            UpdateNoticeables();
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

        public List<GameObject> GetDwarves()
        {
            UpdateDwarves();
            return Variables.Dwarves;
        }
    }
}
