using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaytracingObject : MonoBehaviour
{
    public uint MaterialType = 0;

    public Vector4 BaseColor = Vector3.zero;

    public float Attenuation = 0.1f;

    // Get the material properties of this Object as a struct which can be passed to the GPU
    public SimpleRaytracer.TMaterial GetMaterialStruct(){
        SimpleRaytracer.TMaterial m;
        m.type = MaterialType;
        m.baseColor = BaseColor;
        m.attenuation = Attenuation;

        return m;
    }



    // Start is called before the first frame update
    void Start()
    {
        BaseColor = gameObject.GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
