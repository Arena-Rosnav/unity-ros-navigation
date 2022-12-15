using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Unity;

public class PossibleObjects : MonoBehaviour {

    public List<GameObject> possibleObjects;

    static public Dictionary<string, GameObject> possibleObjectsWithNames; 


    // Start is called before the first frame update
    void Start()
    {
        PossibleObjects.possibleObjectsWithNames = new Dictionary<string, GameObject>();

        foreach (GameObject item in possibleObjects)
        {   
            Debug.Log(item.name);

            PossibleObjects.possibleObjectsWithNames[item.name] = item;
        }
    }
}
