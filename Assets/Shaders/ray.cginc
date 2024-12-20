// Struct which represents a Ray cast in the screen
struct Ray{
    float3 origin;
    float3 direction;
};

// Create a Ray struct from the origing and target points 
Ray CreateRay(float3 origin, float3 target){
    // Create the direction vector
    float3 direction = normalize(target - origin);

    // Create ray structure to return 
    Ray retVal;
    retVal.origin = origin;
    retVal.direction = direction;

    return retVal;
}

// Create a ray which shoots through the pixel with the given x and y location on the screen
// Also takes in a random offset from the center of the pixel so a random location within the pixel 
//can be sampled; this is used for anti-aliasing. 
Ray CreateRayFromPixelLoc(uint x, uint y, float xOffset, float yOffset){
    // Calculate the size of a pixel in world coords
    float pixelSize = screenWidthCoords / screenWidthPixels;  

    // Calculate the x and y change in world coords needed to move by a pixel
    float aspectRatio = (float)screenWidthPixels / (float)screenHeightPixels;
    float pixShiftX = screenWidthCoords / screenWidthPixels;
    float pixShiftY = (screenWidthCoords / screenHeightPixels) / aspectRatio;

    // Calculate the position in world coords of the upper left pixel 
    float3 worldUpperLeft = float3(camLoc.x - (screenWidthCoords / 2), camLoc.y + (screenHeightCoords / 2), camLoc.z + 1);
    float3 firstPixelLoc = float3(
        worldUpperLeft.x + (0.5 * pixShiftX), 
        worldUpperLeft.y - (0.5 * pixShiftY), 
        worldUpperLeft.z);
    
    // Get the location of a point within this pixel to sample
    float3 pixelWordPos = float3(
        firstPixelLoc.x + (x * pixShiftX) + xOffset, 
        firstPixelLoc.y - (y * pixShiftY) + yOffset, 
        camLoc.z + 1.0);

    Ray retVal = CreateRay(camLoc, pixelWordPos);

    return retVal;
}

float3 RandomUnitVector()