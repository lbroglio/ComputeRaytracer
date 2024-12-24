using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SimpleRaytracer : MonoBehaviour
{

    // Structs which match types in the HLSL for the raytracer 
    struct TSphere
    {
        public Vector3 center;
        public float radius;
        public TMaterial m;

    }

    // Struct which represents a material 
    public struct TMaterial{
        /* Type Mapping
        * 0 = Lambertian
        * 1 = Glossy
        */
        public uint type;
        public Vector4 baseColor;
        // TODO: Add more material properties
    };

    // Type to mirror a uint4 on the GPU
    public struct TUint4{
        public uint x;
        public uint y;
        public uint z;
        public uint w;
    }

    public ComputeShader Raytracer;

    // Compute buffers
    public ComputeBuffer oBuffer;
    public ComputeBuffer rBuffer;

    // The number of samples to take per pixel
    public int SamplesPerPixel = 10;
    // The max number of bounces a scattered ray can make
    public int MaxDepth = 10;

    // Array to hold random numbers 

    // Texture to apply to camera
    private RenderTexture _tex;

    // List which holds all the objects in the scene to be considered when raytracing
    private List<RaytracingObject> _raytracingObjects;

    // Globals used for creating the texture / running the shader
    private float aspect;
    public float WorldHeight = 4;
    private float worldWidth;

    // Start is called before the first frame update
    void Start()
    {
        // Add all objects with the Raytracing component tag to a list
        _raytracingObjects = new List<RaytracingObject>(FindObjectsOfType<RaytracingObject>());


        // Setup compute buffers
        oBuffer = new ComputeBuffer(_raytracingObjects.Count, Marshal.SizeOf(typeof(TSphere)));
        rBuffer = new ComputeBuffer(16 * 16, Marshal.SizeOf(typeof(TUint4)));

        //Calculate the size of the camera in world coordinates
        aspect = ((float) Screen.width) / ((float) Screen.height);
        WorldHeight = 1;
        worldWidth = WorldHeight * aspect;

        // Setup random numbers to be used by the shader
        TUint4[] seeds = new TUint4[16 * 16];
        for(int i = 0; i < seeds.Length; i++){
            // Generate random numbers
            TUint4 randSeed;
            randSeed.x = (uint) Random.Range(128, 512);
            randSeed.y = (uint) Random.Range(128, 512);
            randSeed.z = (uint) Random.Range(128, 512);
            randSeed.w = (uint) Random.Range(128, 512);


            // Create vector containing random offsets and add it to array
            seeds[i] = randSeed;
        }
        rBuffer.SetData(seeds);

        // Set Raytracer constants that don't change each frame
        int kernel = Raytracer.FindKernel("CSMain");
        Raytracer.SetVector("backgroundColor", new Vector4(125 / 255.0f, 206 / 255.0f, 235 / 255.0f, 1));
        Raytracer.SetBuffer(kernel, "RandomStates", rBuffer);
        Raytracer.SetInt("screenWidthPixels", Screen.width);
        Raytracer.SetInt("screenHeightPixels", Screen.height);
        Raytracer.SetFloat("screenWidthCoords", worldWidth);
        Raytracer.SetFloat("screenHeightCoords", WorldHeight);
        Raytracer.SetInt("numSamples", SamplesPerPixel);
        Raytracer.SetInt("maxDepth", MaxDepth);


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
            RaytracingObject rObj = _raytracingObjects[i];
            TSphere s = new TSphere();
            s.center = rObj.transform.TransformPoint(rObj.GetComponent<SphereCollider>().center);
            // y is inverted to match coordinate systems
            //s.center = new Vector3(s.center.x, -1 * s.center.y, s.center.z);
            s.radius = rObj.transform.localScale.x / 2;
            s.m = rObj.GetMaterialStruct();

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

        Raytracer.SetVector("camLoc", gameObject.transform.position);
        Raytracer.SetInt("numObjects", spheres.Length);

        // Setup the light
        GameObject light = GameObject.Find("RaytracingLight");
        Raytracer.SetVector("lightPos", light.transform.position);


        // Set buffer data
        oBuffer.SetData(spheres);
        Raytracer.SetBuffer(kernel, "Objects", oBuffer);

        // Dispatch shader
        int workgroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
        int workgroupsY = Mathf.CeilToInt(Screen.height / 16.0f);
        Raytracer.Dispatch(kernel, workgroupsX, workgroupsY, 1);


        // Set the destination texture to the shaders output
        Graphics.Blit(_tex, destination);
    }

    void OnDestroy(){
        oBuffer.Release();
        rBuffer.Release();
    }
}
