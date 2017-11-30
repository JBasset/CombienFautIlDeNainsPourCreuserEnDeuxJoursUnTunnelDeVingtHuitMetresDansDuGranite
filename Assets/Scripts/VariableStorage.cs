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
        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner }
        public enum GaugesLabel { Specialisation, Tiredness, Thirst, Workdesire }
        public List<GameObject> NoticeableObjects;
    }
}
