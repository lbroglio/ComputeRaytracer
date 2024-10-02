using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRaytracer : MonoBehaviour
{

    public int ScreenWidth = 1920;
    public int ScreenHeight = 1080;

    // List which holds all the objects in the scene to be considered when raytracing
    private List<GameObject> _raytracingObjects;
    // Start is called before the first frame update
    void Start()
    {
        // Add all objects with the Raytracing component tag to a list
        _raytracingObjects = new List<GameObject>();
        _raytracingObjects.AddRange(GameObject.FindGameObjectsWithTag("RaytracingComponent"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
