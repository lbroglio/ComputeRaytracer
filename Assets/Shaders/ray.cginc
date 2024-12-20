// Struct which represents a Ray cast in the screen
struct Ray{
    float3 origin;
    float3 direction;
};


// Create a Ray struct from the origing and target points 
Ray createRay(float3 origin, float3 target){
    // Create the direction vector
    float3 direction = normalize(target - origin);

    // Create ray structure to return 
    Ray retVal;
    retVal.origin = origin;
    retVal.direction = direction;

    return retVal;
}