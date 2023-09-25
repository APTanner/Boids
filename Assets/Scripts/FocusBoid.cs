using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusBoid : Boid
{
    static int circleFidelity = 100;
    GameObject viewRadius;
    GameObject avoidRadius;

    private void Start()
    {
        Mesh mesh = GenerateCircle();

        viewRadius = transform.GetChild(1).gameObject;

        viewRadius.GetComponent<MeshFilter>().sharedMesh = mesh;
        viewRadius.GetComponent<MeshRenderer>().material.color = Color.gray;
        viewRadius.SetActive(false);


        mesh = GenerateCircle();

        avoidRadius = transform.GetChild(2).gameObject;

        avoidRadius.GetComponent<MeshFilter>().sharedMesh = mesh;
        avoidRadius.GetComponent<MeshRenderer>().material.color = new Vector4(148f/255, 9f/255, 9f/255, 1);
        avoidRadius.SetActive(false);
    }

    private Mesh GenerateCircle()
    {
        Mesh mesh = new Mesh();
        List<Vector3> verticiesList = new List<Vector3> { };
        float x;
        float y;
        for (int i = 0; i < circleFidelity; i++)
        {
            x = Mathf.Sin((2 * Mathf.PI * i) / circleFidelity);
            y = Mathf.Cos((2 * Mathf.PI * i) / circleFidelity);
            verticiesList.Add(new Vector3(x, y, 0f));
        }
        Vector3[] verticies = verticiesList.ToArray();

        List<int> trianglesList = new List<int> { };
        for (int i = 0; i < (circleFidelity - 2); i++)
        {
            trianglesList.Add(0);
            trianglesList.Add(i + 1);
            trianglesList.Add(i + 2);
        }
        int[] triangles = trianglesList.ToArray();

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void FixedUpdate()
    {
        if (manager.showViewRadius && !viewRadius.activeSelf)
        {
            viewRadius.SetActive(true);
        }
        else if (!manager.showViewRadius && viewRadius.activeSelf)
        {
            viewRadius.SetActive(false);
        }
        //very hacky solution
        viewRadius.transform.localScale = new Vector3(manager.viewRange, manager.viewRange, manager.viewRange);

        if (manager.showAvoidRadius && !avoidRadius.activeSelf)
        {
            avoidRadius.SetActive(true);
        }
        else if(!manager.showAvoidRadius && avoidRadius.activeSelf)
        {
            avoidRadius.SetActive(false);
        }

        avoidRadius.transform.localScale = new Vector3(manager.avoidRange, manager.avoidRange, manager.avoidRange);




        /*Color color = new Vector4(1, 1, 1, (Mathf.Sin(Time.time) + 1) / 2);
        Debug.DrawLine(pos, circlePos, color);*/


        if (numNeighbors > 0)
        {
            foreach (var boid in manager.boids)
            {
                if (boid != this)
                {
                    float distSqrd = (pos - boid.pos).sqrMagnitude;

                    if (distSqrd < Mathf.Pow(manager.viewRange, 2))
                    {
                        if (distSqrd < Mathf.Pow(manager.avoidRange, 2))
                        {
                            float colorOffset = Mathf.Lerp(0, Mathf.Pow(manager.avoidRange, 2), distSqrd);
                            Debug.DrawLine(pos, boid.pos, new Vector4(1, colorOffset, colorOffset, 1));
                        }
                    }
                }
            }
        }
    }
}
