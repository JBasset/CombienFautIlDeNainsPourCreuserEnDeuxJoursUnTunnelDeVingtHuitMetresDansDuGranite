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

        private Transform _gameEnvironment;
        private Transform _dwarves;
        private Transform _mines;
        private int _spawnsLeft; // number of dwarves to create

        private Text _timeSinceStart;
        private Text _totalGoldMined;
        private Text _totalBeerDrank;

        private int _lastGeneralActivityAndMemoryUpdate;
        private int _lastGaneralGaugesUpdate;
        private int _lastSecond;

        void Start()
        {
            _gameEnvironment = GetComponent<Transform>();
            _dwarves = _gameEnvironment.FindChild("Dwarves");
            _mines = _gameEnvironment.FindChild("World").FindChild("Mines");
            _spawnsLeft = 8; //TODO more
            Time.timeScale = 5;

            var generalStatsPanel = UI.transform.FindChild("GeneralStats");
            _timeSinceStart = generalStatsPanel.FindChild("TimeSinceStart").FindChild("Value").GetComponent<Text>();
            _totalGoldMined = generalStatsPanel.FindChild("TotalGoldMined").FindChild("Value").GetComponent<Text>();
            _totalBeerDrank = generalStatsPanel.FindChild("TotalBeerDrank").FindChild("Value").GetComponent<Text>();

            _lastGeneralActivityAndMemoryUpdate = 0;
            _lastGaneralGaugesUpdate = 0;

            UpdateNoticeables();

            Time.timeScale = 5;
        }

        void Update()
        {
            if (_spawnsLeft > 0 && IsSpawnFree())
            {
                InstantiateDwarf();
                _spawnsLeft--;
            }
            if (Time.time - _lastGeneralActivityAndMemoryUpdate >= Variables.activityRethinkChangeRate)
            {
                _lastGeneralActivityAndMemoryUpdate = (int)Mathf.Floor(Time.time);
                foreach (var myDwarf in Variables.Dwarves)
                {
                    var now = Time.time;

                    // oblivion part one : work
                    myDwarf.GetComponent<DwarfMemory>().KnownMines.RemoveAll(work => (now - work.InformatonTakenDateTime) > Variables.OutOfDate);

                    // oblivion part two : friends
                    myDwarf.GetComponent<DwarfMemory>().KnownDwarves.RemoveAll(friend => (now - friend.InformatonTakenDateTime) > Variables.OutOfDate );
                    
                    // let's think about my condition
                    myDwarf.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                }
            }

            if (Time.time - _lastGaneralGaugesUpdate >= Variables.gaugeUpdateRate)
            {
                _lastGaneralGaugesUpdate = (int)Mathf.Floor(Time.time);
                foreach (var myDwarf in Variables.Dwarves)
                {
                    var myMemory = myDwarf.GetComponent<DwarfMemory>();
                    switch (myMemory.CurrentActivity)
                    {
                        case ActivitiesLabel.Miner:
                            if (myMemory.OccupiedMine)
                            {
                                myMemory.LowerBy(GaugesLabel.Pickaxe, 1);
                                if (myMemory.Pickaxe == 0)
                                    myDwarf.GetComponent<DwarfBehaviour>().UpdateActivityAndDestination();
                            }
                            break;
                        case ActivitiesLabel.Explorer:
                            break;
                        case ActivitiesLabel.Deviant:
                            break;
                        case ActivitiesLabel.Vigile:
                            break;
                        case ActivitiesLabel.Supply:
                            break;
                        case ActivitiesLabel.GoToForge:
                            break;
                        default: break;
                    }

                    myMemory.LowerBy(GaugesLabel.ThirstSatisfaction, 1);

                    // Work desire depends on Thirst Satisfaction
                    var ts = myMemory.ThirstSatisfaction;

                    if (ts == 100) // WD : 100
                        myMemory.IncreaseBy(GaugesLabel.Workdesire, 100);
                    else if (ts >= 90) // +10 WD between 80 and 90 TS
                        myMemory.IncreaseBy(GaugesLabel.Workdesire, 10);
                    else if (ts >= 80) // +2 WD between 80 and 100 TS
                        myMemory.IncreaseBy(GaugesLabel.Workdesire, 2);
                    else if (ts >= 60) // +1 WD between 60 and 80 TS
                        myMemory.IncreaseBy(GaugesLabel.Workdesire, 1);
                    else if (ts >= 20 && ts < 40) // -1 WD between 20 and 40 TS
                        myMemory.LowerBy(GaugesLabel.Workdesire, 1);
                    else if (ts < 20) // -2 WD between 0 and 20 TS
                        myMemory.LowerBy(GaugesLabel.Workdesire, 2);
                }
            }

            if (Time.time - _lastSecond >= 1)
            {
                _lastSecond = (int)Mathf.Floor(Time.time);
                Variables.TimeSinceStart = _lastSecond;
            }
            UpdateGeneralStats();
        }

        public void CreateDwarf(int quantity)
        {
            _spawnsLeft += quantity; // we add a dwarf to the list of dwarves left to create
        }

        private bool IsSpawnFree()
        {
            return Variables.Dwarves.All(d => !(Vector3.Distance(d.transform.position, Variables.dwarvesSpawn) <= 2));
            // if any dwarf is under 2 units from the spawn, returns false
        }

        private void InstantiateDwarf()
        {
            var newDwarf = Instantiate(DwarfPrefab, Variables.dwarvesSpawn, new Quaternion(0, 0, 0, 0)) as GameObject;
            if (newDwarf == null) return;
            newDwarf.transform.SetParent(_dwarves);
            UpdateDwarves();
            newDwarf.name = "Dwarf n°" + Variables.Dwarves.Count;
            var memory = newDwarf.GetComponent<DwarfMemory>();
            var behaviour = newDwarf.GetComponent<DwarfBehaviour>();
            newDwarf.GetComponent<DwarfBehaviour>().GE = this;
            memory.GameEnvironment = this;
            memory.DwarfMemoryInitialization();
            behaviour.Start();
            memory.Start();
            UI.SetDwarfButtons();
            behaviour.FirstMove();
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
            Variables.Dwarves = new List<GameObject>();
            for (var i = 0; i < _dwarves.childCount; i++)
                Variables.Dwarves.Add(_dwarves.GetChild(i).gameObject);
        }

        private void UpdateMines()
        {
            Variables.Mines = new List<GameObject>();
            for (var i = 0; i < _mines.childCount; i++)
                Variables.Mines.Add(_mines.GetChild(i).gameObject);
        }

        private void UpdateNoticeables()
        {
            UpdateDwarves();
            UpdateMines();
            Variables.NoticeableObjects = new List<GameObject>();
            foreach (var dwarf in Variables.Dwarves)
                Variables.NoticeableObjects.Add(dwarf);
            foreach (var mine in Variables.Mines)
                Variables.NoticeableObjects.Add(mine);
        }

        private void UpdateGeneralStats()
        {
            _timeSinceStart.text = "" + Variables.TimeSinceStart;
            _totalGoldMined.text = "" + Variables.TotalGoldMined;
            _totalBeerDrank.text = "" + Variables.TotalBeerDrank;
        }
    }
}