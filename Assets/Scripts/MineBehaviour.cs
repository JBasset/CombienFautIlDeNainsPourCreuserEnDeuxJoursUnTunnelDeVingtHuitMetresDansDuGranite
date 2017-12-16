using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MineBehaviour : MonoBehaviour {

    private int ore;
    private List<GameObject> dwarvesInside;
    
    public void AddDwarfInside(GameObject dwarf)
    {
        dwarvesInside.Add(dwarf);
    }
}
