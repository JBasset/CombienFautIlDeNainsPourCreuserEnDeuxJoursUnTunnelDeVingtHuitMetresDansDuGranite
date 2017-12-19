using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    public class GameEnvironment : MonoBehaviour
    {
        public GameObject Target;
        public VariableStorage Variables = new VariableStorage();
        public GameObject DwarfPrefab;
        public UIBehaviour UI;

        private Transform gameEnvironment;
        private Transform dwarves;
        private Transform mines;
        private int spawnsLeft; // number of dwarves to create

        private Text timeSinceStart;
        private Text totalGoldMined;
        private Text totalBeerDrank;

        private int LastGeneralActivityUpdate;
        private int LastGaneralGaugesUpdate;
        private int lastSecond;

        void Start()
        {
            gameEnvironment = GetComponent<Transform>();
            dwarves = gameEnvironment.FindChild("Dwarves");
            mines = gameEnvironment.FindChild("World").FindChild("Mines");
            spawnsLeft = 0;

            Transform GeneralStatsPanel = UI.transform.FindChild("GeneralStats");
            timeSinceStart = GeneralStatsPanel.FindChild("TimeSinceStart").FindChild("Value").GetComponent<Text>();
            totalGoldMined = GeneralStatsPanel.FindChild("TotalGoldMined").FindChild("Value").GetComponent<Text>();
            totalBeerDrank = GeneralStatsPanel.FindChild("TotalBeerDrank").FindChild("Value").GetComponent<Text>();

            LastGeneralActivityUpdate = 0;
            LastGaneralGaugesUpdate = 0;

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
                    myDwarf.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                }
            }
            
            if (Time.time - LastGaneralGaugesUpdate >= Variables.gaugeUpdateRate)
            {
                LastGaneralGaugesUpdate = (int) Mathf.Floor(Time.time);
                foreach (var myDwarf in Variables.Dwarves)
                {
                    switch (myDwarf.GetComponent<DwarfMemory>().CurrentActivity)
                    {
                        case ActivitiesLabel.Miner:
                            if (myDwarf.GetComponent<DwarfMemory>().OccupiedMine)
                            {
                                myDwarf.GetComponent<DwarfMemory>().LowerBy(GaugesLabel.Pickaxe, 1);
                                if (myDwarf.GetComponent<DwarfMemory>().Pickaxe == 0)
                                    myDwarf.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                            }
                            break;
                        default: break;
                        /*case ActivitiesLabel.Deviant: break;
                        case ActivitiesLabel.Explorer: break;
                        case ActivitiesLabel.Vigile: break;
                        case ActivitiesLabel.Supply: break;
                        case ActivitiesLabel.GoToForge: break;*/
                    }

                    myDwarf.GetComponent<DwarfMemory>().LowerBy(GaugesLabel.ThirstSatisfaction, 1);

                    // Work desire depends on Thirst Satisfaction
                    if (myDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction >= 80) // +2 WD between 80 and 100 TS
                        myDwarf.GetComponent<DwarfMemory>().IncreaseBy(GaugesLabel.Workdesire, 2);
                    else if (myDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction >= 60) // +1 WD between 60 and 80 TS
                        myDwarf.GetComponent<DwarfMemory>().IncreaseBy(GaugesLabel.Workdesire, 1);
                    else if (myDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction >= 20 && myDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction < 40) // -1 WD between 20 and 40 TS
                        myDwarf.GetComponent<DwarfMemory>().LowerBy(GaugesLabel.Workdesire, 1);
                    else if (myDwarf.GetComponent<DwarfMemory>().ThirstSatisfaction < 20) // -2 WD between 0 and 20 TS
                        myDwarf.GetComponent<DwarfMemory>().LowerBy(GaugesLabel.Workdesire, 2);
                }
            }

            if (Time.time - lastSecond >= 1)
            {
                lastSecond = (int)Mathf.Floor(Time.time);
                Variables.TimeSinceStart = lastSecond;
            }
            UpdateGeneralStats();
        }

        public void CreateDwarf(int quantity)
        {
            spawnsLeft += quantity; // we add a dwarf to the list of dwarves left to create
        }

        private bool IsSpawnFree()
        {
            return Variables.Dwarves.All(d => !(Vector3.Distance(d.transform.position, Variables.dwarvesSpawn) <= 2));
            // if any dwarf is under 2 units from the spawn, returns false
        }

        private void InstantiateDwarf()
        {
            GameObject newDwarf = Instantiate(DwarfPrefab, Variables.dwarvesSpawn, new Quaternion(0, 0, 0, 0)) as GameObject;
            newDwarf.transform.SetParent(dwarves);
            UpdateDwarves();
            newDwarf.name = "Dwarf n°" + Variables.Dwarves.Count;
            newDwarf.GetComponent<DwarfBehaviour>().GE = this;
            newDwarf.GetComponent<DwarfMemory>().GameEnvironment = this;
            newDwarf.GetComponent<DwarfMemory>().DwarfMemoryInitialization();

            newDwarf.GetComponent<DwarfBehaviour>().Start();
            newDwarf.GetComponent<DwarfMemory>().Start();

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

        private void UpdateGeneralStats()
        {
            timeSinceStart.text = "" + Variables.TimeSinceStart;
            totalGoldMined.text = "" + Variables.TotalGoldMined;
            totalBeerDrank.text = "" + Variables.TotalBeerDrank;
        }
    }
}
