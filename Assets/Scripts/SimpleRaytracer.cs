using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRaytracer : MonoBehaviour
{

    // Structs which match types in the HLSL for the raytracer 
    struct TSphere
    {
        public Vector3 center;
        public float radius;
        public Vector4 matColor;
    }

    public ComputeShader Raytracer;

    public ComputeBuffer oBuffer;

    public int ScreenWidth = 1920;
    public int ScreenHeight = 1080;

    // Texture to apply to camera
    private RenderTexture _tex;

    // List which holds all the objects in the scene to be considered when raytracing
    private List<GameObject> _raytracingObjects;
    // Start is called before the first frame update
    void Start()
    {
        // Add all objects with the Raytracing component tag to a list
        _raytracingObjects = new List<GameObject>();
        _raytracingObjects.AddRange(GameObject.FindGameObjectsWithTag("RaytracingComponent"));

        // Setup compute buffer
        int typeSize = (sizeof(float) * 3) + sizeof(float) + (sizeof(float) * 4);
        oBuffer = new ComputeBuffer(_raytracingObjects.Count, typeSize);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        // Get the texture for this frame by running raytracer compute shader   

        // Convert Raytrace tagged GameObjects into sphere structs
        TSphere[] spheres = new TSphere[_raytracingObjects.Count];
        for(int i =0; i < _raytracingObjects.Count; i++){
            GameObject rObj = _raytracingObjects[i];
            TSphere s = new TSphere();
            s.center = rObj.transform.position;
            s.radius = rObj.transform.localScale.x;
            //s.matColor = rObj.GetComponent<MeshRenderer>().material.color;
            s.matColor = new Vector4(0, 0, 0, 1);
            spheres[i] = s;
        }

        // Create the texture to apply if it doesn't exist
        if( _tex == null)
        {
            _tex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _tex.enableRandomWrite = true;
            _tex.Create();
        }

        // Get shader and set its output
        int kernel = Raytracer.FindKernel("CSMain");
        Raytracer.SetTexture(kernel, "Result", _tex);

        // Set up configuration constants
        Raytracer.SetVector("backgroundColor", new Vector4(125 / 255.0f, 206 / 255.0f, 235 / 255.0f, 1));
        Raytracer.SetVector("camLoc", gameObject.transform.position);
        Raytracer.SetInt("numObjects", spheres.Length);
        Raytracer.SetInt("screenWidthPixels", Screen.width);
        Raytracer.SetInt("screenHeightPixels", Screen.height);
        // Calculate the size of the camera in world coordiantes
        float aspect = (float)Screen.width / Screen.height;
        float worldWidth = 10;
        Raytracer.SetFloat("screenWidthCoords", worldWidth);
        Raytracer.SetFloat("screenHeightCoords", worldWidth / aspect);

        // Set buffer data
        oBuffer.SetData(spheres);
        Raytracer.SetBuffer(kernel, "Objects", oBuffer);

        // Dispatch shader
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        Raytracer.Dispatch(kernel, workgroupsX, workgroupsY, 1);


        // Set the destination texture to the shaders output
        Graphics.Blit(_tex, destination);
    }

    void OnDestroy(){
        oBuffer.Release();
    }
}
