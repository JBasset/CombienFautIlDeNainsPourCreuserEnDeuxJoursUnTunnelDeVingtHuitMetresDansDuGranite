using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        public int maxValueGauge = 100;
        public int minValueGauge = 0;

        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner, GoToForge, GoToSleep }
        public enum GaugesLabel { Specialisation, Tiredness, Thirst, Workdesire, Pickaxe}

        public List<GameObject> NoticeableObjects; // objects dwarves can see. used to test dwarves line of sight
        public List<GameObject> Dwarves; // list of all living dwarves in the game
        public List<GameObject> Mines; // list of the mines in the World

        // new dwarves start with those activities :
        public WeightedList startingActivity = new WeightedList(
            new List<_WeightedObject>{
                new _WeightedObject(ActivitiesLabel.Explorer, 1),
                new _WeightedObject(ActivitiesLabel.Miner, 0) // currently zero chance to be a miner :(
            } 
        );
    }
}
