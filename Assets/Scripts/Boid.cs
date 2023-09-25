using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [HideInInspector]
    public BoidManager manager;

    [HideInInspector]
    public Vector3 pos;
    [HideInInspector]
    public Vector3 heading;

    protected Vector3 velocity;
    protected Vector3 deltaV;

    [HideInInspector]
    public float distToCenter;
    [HideInInspector]
    public float constraintMultiplier;

    [HideInInspector]
    public Vector3 separationHeading;
    [HideInInspector]
    public Vector3 avgNeighborHeading;
    [HideInInspector]
    public Vector3 avgNeighborPos;
    [HideInInspector]
    public Vector3 fear;
    [HideInInspector]
    public int numNeighbors;

    [HideInInspector]
    public bool isBaitBalling = false;
    [HideInInspector]
    public Vector3 circlePos;

    Transform thisTransform;

    
    public void InitializeBoid(Vector3 pos, Vector3 dir, Color color)
    {
        manager = gameObject.GetComponentInParent<BoidManager>();
        GetComponentInChildren<MeshRenderer>().material.color = color;

        this.pos = pos;
        this.velocity = dir * (manager.minSpeed + manager.maxSpeed) / 2;
        thisTransform = transform;
    }



    public void UpdateBoid()
    {
        deltaV = Vector3.zero;

        if (isBaitBalling)
        {
            deltaV += AccelerateTowards(circlePos - pos) * manager.schoolCentreWeight;
            //Debug.DrawLine(this.pos, circlePos, Color.blue);
        }

        if (manager.isTerrorSphere)
        {
            deltaV += AccelerateTowards(fear) * manager.fearWeight;
        }

        if (numNeighbors > 0)
        {
            deltaV += AccelerateTowards(separationHeading) * manager.separationWeight;
            if (!isBaitBalling)
            {
                deltaV += AccelerateTowards(avgNeighborHeading) * manager.alignmentWeight;
                deltaV += AccelerateTowards(avgNeighborPos / numNeighbors - pos) * manager.cohesionWeight;
            }
        }

        if (distToCenter > manager.constraintRadius - 5)
        {
            deltaV += AccelerateTowards(Vector3.zero - pos) * constraintMultiplier;
        }

        velocity += deltaV * Time.fixedDeltaTime;

        float speed = velocity.magnitude; //velocity.magnitude uses a square root, so doing it as few times as possible will help with proformance
        heading = velocity / speed;
        velocity = heading * Mathf.Clamp(speed, manager.minSpeed, manager.maxSpeed);
        //velocity = normalized velocity (heading) * speed clamped between the max and min speed

        pos = thisTransform.localPosition += velocity * Time.fixedDeltaTime;

        

        thisTransform.localPosition = pos;
        thisTransform.localRotation = Quaternion.LookRotation(heading, Vector3.forward);
    }

    

    Vector3 AccelerateTowards(Vector3 pos)
    {
        Vector3 delta = (pos.normalized * manager.maxSpeed) - velocity;
        return Vector3.ClampMagnitude(delta, manager.steerWeight);
    }

    

    Vector3 CalculateCircling()
    {
        /*float r = 2f;
        float t = 1f;
        return new Vector3(
            Mathf.Cos(Time.time / t) * r,
            Mathf.Sin(Time.time / t) * r,
            0
        );*/

        /*
        Vector2 pos1 = pos;
        Vector2 pos2 = pos + velocity;
        //ax + by + c = 0
        float a = pos1.y - pos2.y;
        float b = pos2.x - pos1.x;
        float c = pos1.x * pos2.y - pos1.y * pos2.x;

        Vector3 closestPoint = new Vector3(
            (b * (b * manager.schoolCentre.x - a * manager.schoolCentre.y) - a * c) / (a * a + b * b),
            (a * (-b * manager.schoolCentre.x + a * manager.schoolCentre.y) - b * c) / (a * a + b * b),
            0
        );

        return .1f*(manager.schoolCentre - closestPoint) + closestPoint;
        */

        float theta = Mathf.Asin(velocity.magnitude / 2 / ((Vector2)manager.baitBallPos - (Vector2)pos).magnitude);
        Vector3 closestPoint = new Vector3(
            (pos.x - manager.baitBallPos.x) * Mathf.Cos(theta) - (pos.y - manager.baitBallPos.y) * Mathf.Sin(theta) + manager.baitBallPos.x,
            (pos.x - manager.baitBallPos.x) * Mathf.Sin(theta) + (pos.y - manager.baitBallPos.y) * Mathf.Cos(theta) + manager.baitBallPos.y,
            0
        );
        return .5f * (manager.baitBallPos - closestPoint) + closestPoint;
    }

}
