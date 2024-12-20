// Struct which represents a material 
struct Material{
    /* Type Mapping
    * 0 = Lambertian
    * 1 = Glossy
    */
    uint type;
    float4 albedo;
    float attenuation;
    // TODO: Add more material properties
};

