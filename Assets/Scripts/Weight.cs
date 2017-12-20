using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class _WeightedObject // create a weighted object, like : (chat, 3)
    {
        public object TheObject;
        public int TheWeight;

        public _WeightedObject(object obj, int weight)
        {
            TheObject = obj;
            TheWeight = weight;
        }
    }

    public class WeightedList
    {
        /*
         *  starting with { (chat, 2) ; (chien, 1) ; (bière, 3) }
         *  we get the following list : { chat, chat, chien, bière, bière, bière)
         *  thus, weighted random selection \o/ 
         *  ---> just call selectRandomItem()
         */
        private static readonly Random Rnd = new Random();

        public List<object> List = new List<object>();

        public WeightedList(IEnumerable<_WeightedObject> wList) {
            foreach (var weiObj in wList)
            {
                AddItem(weiObj);
            }
        }

        public void AddItem(_WeightedObject item) {
            for (var i = 0; i < item.TheWeight; i++) {
                List.Add(item.TheObject);
            }
        }

        public object SelectRandomItem() {
            var index = Rnd.Next(List.Count);
            return List[index];
        }
    }
}
