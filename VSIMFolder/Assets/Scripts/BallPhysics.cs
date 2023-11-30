using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

/*

	CANDIDATE NUMBER: 840

*/

public class BallPhysics : MonoBehaviour
{
    const float grav = 9.81f;
    Vector3 posBefore, posAfter, velBefore, velAfter; // Position and movement
    Vector3 acceleration, gravity, gForce, nForce, sumForce; // Forces and acceleration
    Vector3 normBefore, normAfter;

    int indexBefore, indexAfter;

    [SerializeField] float mass = 0.1f;
    [SerializeField] float radius = 1.0f;
    [SerializeField] GameObject surfaceRef;
    SurfaceScript scriptRef;
    private CollisionInfo cTriangle = new CollisionInfo();

    private bool onSurface = false;


    void Start()
    {
        posBefore = transform.position;
        posAfter = posBefore;
        velBefore = new Vector3(0, 0, 0);
        velAfter = velBefore;
        acceleration = new Vector3(0, 0, 0);
        gravity = new Vector3(0, -grav, 0);
        gForce = mass * gravity;
        nForce = new Vector3(0, 0, 0);
        sumForce = new Vector3(0,0,0);
        indexBefore = -1;
        indexAfter = indexBefore;
        normBefore = Vector3.zero;
        normAfter = normBefore;

        if (surfaceRef != null)
        {
            scriptRef = surfaceRef.GetComponent<SurfaceScript>();
        } 
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.DrawSphere(transform.position, radius);
    // }

    // Update is called once per frame
    void FixedUpdate() 
    {
        posBefore = transform.position;   
        velBefore = velAfter;
        sumForce = gForce; // Gravity always applies

        if (scriptRef == null)
        {
            Debug.Log("SCRIPTREF NULL");
        }

        // We're finding the triangle
        cTriangle = scriptRef.CheckCollision(posBefore);   
        CollisionInfo nHitCol = scriptRef.CheckCollision(posBefore + velBefore * Time.fixedDeltaTime);

        // A check to see if we're on a triangle, or if we're out of bounds
        if (cTriangle.hitNormal != Vector3.zero)
        {
            normAfter = cTriangle.hitNormal;
            Vector3 hit = cTriangle.hitPosition;
            Vector3 nextHitNormal = nHitCol.hitNormal;
            int indexAfter = cTriangle.hitTriangle.index;

            if (indexBefore == -1) { indexBefore = indexAfter; }
            
            float distance = Vector3.Dot(posBefore - hit, normAfter.normalized);

            //Debug.Log("Distance: " + distance);
            // This if-sentence is our check to see if we're colliding with a surface
            if (Mathf.Abs(distance) <= radius)
            {
                // We're colliding with the surface
                nForce = mass * (-gravity) * Mathf.Cos(normAfter.normalized.z);
                //Debug.Log("nForce: " + nForce);
                sumForce += nForce;
                onSurface = true;
                transform.position = posBefore + (Vector3.Dot(hit - posBefore, normAfter.normalized)) * normAfter.normalized + radius * normAfter.normalized;
                posBefore = transform.position;
            }

            // accelerationvector, speed and position (8.12, 8.14, 8.15)
            acceleration = sumForce / mass;
            velAfter = velBefore +  acceleration * Time.fixedDeltaTime;
            if (onSurface)
            {
                velAfter = Vector3.ProjectOnPlane(velAfter, normAfter.normalized);
            }
            posAfter = posBefore + velAfter * Time.fixedDeltaTime;

            //Debug.Log("Acceleration:" + acceleration);
            //Debug.Log("velAfter:" + velAfter);
            //Debug.Log("posAfter:" + posAfter);

            if (indexBefore != indexAfter)
            {
                //Debug.Log("Index changed");
                //Debug.Log("Before:" + indexBefore + " | After: " + indexAfter);
                // Here the ball has rolled over to another triangle
                Vector3 xVec = (normBefore.normalized + normAfter.normalized) / Vector3.Magnitude(normBefore.normalized + normAfter.normalized);
                velAfter = velBefore - 2 * Vector3.Project(velBefore, xVec);
                velAfter = Vector3.ProjectOnPlane(velAfter, normAfter.normalized);
                posAfter = posBefore + velAfter * Time.fixedDeltaTime;
                normBefore = normAfter;
                indexBefore = indexAfter;
            }

            // if (distance < radius)
            // {
            //     posAfter += posAfter + (Vector3.Dot(hit - posAfter, normAfter)) * normAfter + radius * normAfter.normalized;
            // }
        }
        else
        {
            Debug.Log("Vector returned null");
            Destroy(this);
        }


        transform.position = posAfter;
    }
}
