using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class _WeightedObject // create a weighted object, like : (chat, 3)
    {
        public Object _object; public int _weight;

        public _WeightedObject(Object obj, int weight)
        {
            this._object = obj; this._weight = weight;
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
        private static Random rnd = new Random();

        public List<Object> list = new List<Object>();

        public WeightedList(List<_WeightedObject> wList) {
            foreach (_WeightedObject wO in wList)
            {
                AddItem(wO);
            }
        }

        public void AddItem(_WeightedObject item) {
            for (int i = 0; i < item._weight; i++) {
                list.Add(item._object);
            }
        }

        public Object SelectRandomItem() {
            int index = rnd.Next(list.Count);
            return list[index];
        }
    }
}
