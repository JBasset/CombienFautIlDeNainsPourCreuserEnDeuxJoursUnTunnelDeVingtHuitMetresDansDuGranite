using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class VariableStorage
    {
        public int maxValueGauge = 100;
        public int minValueGauge = 0;
        public enum ActivitiesLabel { Explorer, Deviant, Vigile, Supply, Miner, GoToForge, GoToSleep }
        public enum GaugesLabel { Specialisation, Tiredness, Thirst, Workdesire, Pickaxe}

        // new dwarves start with those activities :
        public WeightedList startingActivity = new WeightedList(
            new List<_WeightedObject>{
                new _WeightedObject(ActivitiesLabel.Explorer, 1),
                new _WeightedObject(ActivitiesLabel.Miner, 0) // currently zero chance to be a miner :(
            } 
        );        
    }
}
