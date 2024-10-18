using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingSphere : MonoBehaviour
{

    // The point to rotate this object around
    Vector3 orbitingPoint = new Vector3(0, 1.5f ,4);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(orbitingPoint, Vector3.up, Time.deltaTime * 50f);
    }
}
