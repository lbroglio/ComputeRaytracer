using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingSphere : MonoBehaviour
{

    public float xSpeed = 0.002f;
    public float ySpeed = 0f;

    private float viewHeight;
    private float viewWidth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get the size of the view
        GameObject camera = GameObject.Find("Main Camera");
        viewHeight = camera.GetComponent<SimpleRaytracer>().WorldHeight;
        float aspect = ((float) Screen.width) / ((float) Screen.height);
        viewWidth = viewHeight * aspect;

        // Get the position of the camera
        Vector3 camPos = camera.transform.position;

        // Radius of this sphere
        float radius = GetComponent<SphereCollider>().radius;

        // Invert y speed if its at the edge
        if((transform.position.y + radius >= camPos.y + (viewHeight / 2) && ySpeed > 0) || 
            (transform.position.y - radius <= camPos.y - (viewHeight / 2)) && ySpeed < 0){
            ySpeed *= -1;
        }

        // Invert x speed if its at the edge
        if((transform.position.x + radius >= camPos.x + viewWidth && xSpeed > 0) || 
            (transform.position.x - radius <= camPos.x - viewWidth) && xSpeed < 0){
            xSpeed *= -1;
        }

        // Modify x and y position by speed
        transform.position += new Vector3(xSpeed, ySpeed, 0);


    }
}
