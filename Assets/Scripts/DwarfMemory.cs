using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ActivitiesLabel = Assets.Scripts.VariableStorage.ActivitiesLabel;
using GaugesLabel = Assets.Scripts.VariableStorage.GaugesLabel;

namespace Assets.Scripts
{
    

    class KnownDwarf
    {
        public Vector3 dwarfPosition;
        public int id; public bool highThirst; public DateTime lastInteraction;
        
        public KnownDwarf() { // TODO: mettre un nain en paramètre
            // this.id = nain.id
            // this.highThirst = nain.memory. 
            // TODO: associer une memoire à chaque nain
            this.lastInteraction = DateTime.Now;
        }
    }

    class KnownMine
    {
        public Vector3 minePosition;
        public int id; public bool highThirst; public DateTime lastInteraction;

        public KnownMine(int _id, bool _highThirst)
        {
            // this.id = minePosition.id
            // this.highThirst = mine.thirstEvaluation()
            // TODO: trouver un moyen d'évaluer la soif, genre thirstEvaluation()
            this.lastInteraction = DateTime.Now;
        }
    }

    class Gauges {
        private VariableStorage variables;

        private int[] _gauges = new int[5];

        #region get/set (specialisation, tiredness, thirst, workdesire, pickaxe)
        public int Specialisation {
            get { return _gauges[0]; }
            set { _gauges[0] = stockGauge(value); }
        }
        public int Tiredness {
            get { return _gauges[1]; }
            set { _gauges[1] = stockGauge(value); }
        }
        public int Thirst {
            get { return _gauges[2]; }
            set { _gauges[2] = stockGauge(value); }
        }
        public int WorkDesire {
            get { return _gauges[3]; }
            set { _gauges[3] = stockGauge(value); }
        }
        public int Pickaxe{
            get { return _gauges[4]; }
            set { _gauges[4] = stockGauge(value); }
        }
        #endregion

        public Gauges(int specialisation, int tiredness, int thirst, int workDesire, int pickaxe) {
            _gauges[0] = stockGauge(specialisation);
            _gauges[1] = stockGauge(tiredness);
            _gauges[2] = stockGauge(thirst);
            _gauges[3] = stockGauge(workDesire);
            _gauges[4] = stockGauge(pickaxe);
        }

        private int stockGauge(int value) {
            int max = variables.maxValueGauge;
            int min = variables.minValueGauge;
            return (value > min) ? ((value < max) ? value : max) : min;
        }
    }

    class DwarfMemory
    {
        private VariableStorage variables;
        
        private ActivitiesLabel _currentActivity;
        public ActivitiesLabel CurrentActivity { get { return _currentActivity; } }

        private ActivitiesLabel _previousActivity;
        public ActivitiesLabel PreviousActivity { get { return _previousActivity; } }

        private DateTime _lastActivityChange;
        public DateTime LastActivityChange {  get { return _lastActivityChange; } }

        private Gauges Gauges;
        public int Specialisation { get { return Gauges.Specialisation; } }
        public int Tirednesss { get { return Gauges.Tiredness; } }
        public int Thirst { get { return Gauges.Thirst; } }
        public int WorkDesire { get { return Gauges.WorkDesire; } }
        public int Pickaxe { get { return Gauges.Pickaxe; } }

        int? targetDwarf; // identité de la cible si c'est un vigile

        // int targetPosition; // position cible ?

        private List<KnownDwarf> _knownDwarves = new List<KnownDwarf>(); // nainConnus
        public List<KnownDwarf> KnownDwarves { get { return _knownDwarves; } }

        private List<KnownMine> _knownMines = new List<KnownMine>();
        public List<KnownMine> KnownMines { get { return _knownMines; } }

        public DwarfMemory() { // création d'une mémoire

            // variables.startingActivity
            _currentActivity = (ActivitiesLabel)variables.startingActivity.selectRandomItem();
            
            int max = variables.maxValueGauge;
            this.Gauges = new Gauges(max, max, max, max, max);
        }

        #region increase and lower functions ( param : VariableStorage.GaugesLabel theGauge, int byValue )
        public void increaseBy(GaugesLabel theGauge, int byValue) {
            // exemple of use : one dwarf gets thirsty, his thirst increaseBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Specialisation) Gauges.Specialisation += byValue;
                else if (theGauge == GaugesLabel.Tiredness) Gauges.Tiredness += byValue;
                else if (theGauge == GaugesLabel.Thirst) Gauges.Thirst += byValue;
                else if (theGauge == GaugesLabel.Workdesire) Gauges.WorkDesire += byValue;
                else if (theGauge == GaugesLabel.Pickaxe) Gauges.Pickaxe += byValue;
            }
        }

        public void lowerBy(GaugesLabel theGauge, int byValue)
        {
            // exemple of use : one dwarf drinks, his thirst lowerBy(Thirst,10)
            if (byValue > 0)
            {
                if (theGauge == GaugesLabel.Specialisation) Gauges.Specialisation -= byValue;
                else if (theGauge == GaugesLabel.Tiredness) Gauges.Tiredness -= byValue;
                else if (theGauge == GaugesLabel.Thirst) Gauges.Thirst -= byValue;
                else if (theGauge == GaugesLabel.Workdesire) Gauges.WorkDesire -= byValue;
                else if (theGauge == GaugesLabel.Pickaxe) Gauges.Pickaxe -= byValue;
            }
        }
        
        #endregion

        public Vector3 getNewDestination()
        {
            Vector3 destination = new Vector3();
            if (this._currentActivity == ActivitiesLabel.Deviant)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Explorer)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Miner)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Supply)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.Vigile)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.GoToForge)
            { /* TODO: remplir */ }
            else if (this._currentActivity == ActivitiesLabel.GoToSleep)
            { /* TODO: remplir */ }

            return destination;
        }

    }
}
