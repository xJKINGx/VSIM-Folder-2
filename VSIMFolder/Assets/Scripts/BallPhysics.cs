using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    const float grav = 9.81f;
    Vector3 posBefore, posAfter, velBefore, velAfter; // Position and movement
    Vector3 acceleration, gravity, gForce, nForce, sumForce; // Forces and acceleration

    [SerializeField] float mass = 0.1f;
    [SerializeField] float radius = 1.0f;
    [SerializeField] GameObject surfaceRef;
    SurfaceScript scriptRef;
    private CollisionInfo cTriangle = new CollisionInfo();

    void Start()
    {
        posBefore = transform.position;
        posAfter = posBefore;
        velBefore = new Vector3(0, 0, 0);
        acceleration = new Vector3(0, 0, 0);
        gravity = new Vector3(0, -grav, 0);
        gForce = mass * gravity;
        nForce = new Vector3(0, 0, 0);
        sumForce = new Vector3(0,0,0);
        if (surfaceRef != null)
        {
            scriptRef = surfaceRef.GetComponent<SurfaceScript>();
        } 
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        posBefore = transform.position;   
        sumForce = gForce; // Gravity always applies

        if (scriptRef == null)
        {
            Debug.Log("SCRIPTREF NULL");
        }

        cTriangle = scriptRef.CheckCollision(posBefore);   
        if (cTriangle.hitNormal != Vector3.zero)
        {
            Vector3 norm = cTriangle.hitNormal;
            Vector3 hit = cTriangle.hitPosition;

            // This if-sentence is our check to see if we're colliding with a surface
            // If we're above the surface we assume freefall
            // If we're perfectly at the surface's height or below we assume we're moving
            // along the surface beneath
            Vector3 distanceVector = posBefore - hit;
            float distance = distanceVector.magnitude;
            Debug.Log("Distance: " + distance);

            if (distance <= radius)
            {
                // We're colliding with the surface
                nForce = mass * grav * norm * Mathf.Cos(norm.y);
                Debug.Log("nForce: " + nForce);
                sumForce += nForce;
            }
        }

        acceleration = sumForce / mass;
        velAfter += acceleration * Time.fixedDeltaTime;
        transform.Translate(velAfter * Time.fixedDeltaTime);
    }
}
