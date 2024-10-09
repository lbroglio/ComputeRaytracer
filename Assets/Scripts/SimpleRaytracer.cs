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

    // Compute buffers
    public ComputeBuffer oBuffer;
    public ComputeBuffer rBuffer;

    // Array to hold random numbers 

    // Texture to apply to camera
    private RenderTexture _tex;

    // List which holds all the objects in the scene to be considered when raytracing
    private List<GameObject> _raytracingObjects;


    // The number of samples to take off every pixel for the purposes of anti aliasing
    public int numSamples = 4;

    // Globals used for creating the texture / running the shader
    private float aspect;
    private float worldHeight;
    private float worldWidth;

    // Start is called before the first frame update
    void Start()
    {
        // Add all objects with the Raytracing component tag to a list
        _raytracingObjects = new List<GameObject>();
        _raytracingObjects.AddRange(GameObject.FindGameObjectsWithTag("RaytracingComponent"));

        // Setup compute buffers
        int typeSize = (sizeof(float) * 3) + sizeof(float) + (sizeof(float) * 4);
        oBuffer = new ComputeBuffer(_raytracingObjects.Count, typeSize);
        typeSize = (sizeof(float) * 2);
        rBuffer = new ComputeBuffer(Screen.width * Screen.height * numSamples, typeSize);

        //Calculate the size of the camera in world coordinates
        aspect = ((float) Screen.width) / ((float) Screen.height);
        worldHeight = 1;
        worldWidth = worldHeight * aspect;
        float pixShiftX = worldWidth / Screen.width;
        float pixShiftY = (worldWidth / Screen.height) / aspect;

        float halfPixX = pixShiftX / 2;
        float halfPixY = pixShiftY / 2;

        // Setup random numbers to be used by the shader
        Vector2[] randNums = new Vector2[Screen.width * Screen.height * numSamples];
        for(int i = 0; i < Screen.width * Screen.height * numSamples; i++){
            // Generate random numbers
            float xOffset = Random.Range(halfPixX * -1, halfPixX);
            float yOffset = Random.Range(halfPixY * -1, halfPixY);

            // Create vector containing random offsets and add it to array
            randNums[i] = new Vector2(xOffset, yOffset);
            //Debug.Log(randNums[i]);
        }
        rBuffer.SetData(randNums);

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
            s.matColor = rObj.GetComponent<MeshRenderer>().material.color;
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
        Raytracer.SetFloat("screenWidthCoords", worldWidth);
        Raytracer.SetFloat("screenHeightCoords", worldHeight);
        Raytracer.SetInt("numSamples", numSamples);


        // Set buffer data
        oBuffer.SetData(spheres);
        Raytracer.SetBuffer(kernel, "Objects", oBuffer);
        Raytracer.SetBuffer(kernel, "RandomNums", rBuffer);

        // Dispatch shader
        int workgroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        Raytracer.Dispatch(kernel, workgroupsX, workgroupsY, 1);


        // Set the destination texture to the shaders output
        Graphics.Blit(_tex, destination);
    }

    void OnDestroy(){
        oBuffer.Release();
        rBuffer.Release();
    }
}
