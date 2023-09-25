using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BoidManager : MonoBehaviour
{
    public int numberOfBoids = 10;

    public float separationWeight;
    public float alignmentWeight;
    public float cohesionWeight;
    public float fearWeight;

    public float schoolCentreWeight;

    public float minSpeed;
    public float maxSpeed;
    public float steerWeight;
    [Range(0,2)]
    public float viewRange;
    public float avoidRange;

    public bool useFocusBoid;

    private GameObject sphere;
    private Vector3 spherePos;
    public float terrorShereDiameter = 1;
    public bool isTerrorSphere = false;

    public bool baitBall;
    private void OnValidate()
    {
        OnBaitBallStatusChange(baitBall);
    }
    public Vector3 baitBallPos;

    public float constraintRadius = 20;

    [HideInInspector]
    public Boid[] boids;

    public Boid boidPrefab;
    public FocusBoid focusPrefab;
    public bool showViewRadius, showAvoidRadius;

    public ComputeShader boidComputeShader;

    private int boidMeshFidelity = 10;

    private void Start()
    {
        boids = GenerateBoids(numberOfBoids, Vector3.zero);
        Init();
    }

    private void Init()
    {
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(terrorShereDiameter, terrorShereDiameter, terrorShereDiameter);
        sphere.transform.parent = this.transform;
        sphere.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
    }

    Boid[] GenerateBoids(int numBoids, Vector3 spawnPos)
    {
        Boid[] boids = new Boid[numBoids + (useFocusBoid ? 1:0)];
        Transform transform = this.transform;
        if(boidPrefab != null)
        {
            for (int i = 0; i < numBoids; i++)
            {
                boids[i] = Instantiate(boidPrefab, transform);
                float grey = Random.Range(0, 255) / 255f;
                boids[i].InitializeBoid(Random.insideUnitCircle, Random.insideUnitCircle, new Color(grey, grey, grey));
                //if using 3d boids
                //boids[i].transform.GetChild(0).GetComponent<MeshFilter>().mesh = GenerateBoidMesh();
            }
            if (useFocusBoid)
            {
                boids[numBoids] = Instantiate(focusPrefab, transform);
                
                boids[numBoids].InitializeBoid(Random.insideUnitCircle, Random.insideUnitCircle, Color.red);
            }
        }
        else
        {
            Debug.LogError("No Boid Prefab Attached");
        }
        return boids;
    }

    Mesh GenerateBoidMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Mesh";
        Vector3[] verts = new Vector3[boidMeshFidelity * 2];
        int[] tris = new int[(boidMeshFidelity - 1) * 6];
        Vector2[] uvs = new Vector2[verts.Length];

        float hGap = 1f / (boidMeshFidelity - 1);

        for (int x = 0; x < boidMeshFidelity; x++)
        {
            verts[x * 2] = new Vector3(hGap * x, 0, 0);
            uvs[x * 2] = new Vector2(hGap * x / 1f, 0);
            verts[x * 2 + 1] = new Vector3(hGap * x, 1, 0);
            uvs[x * 2 + 1] = new Vector2(hGap * x / 1f, 1);
        }

        for (int x = 0, i = 0; x < boidMeshFidelity - 1; x++, i += 6)
        {
            tris[i] = x * 2;
            tris[i + 1] = tris[i + 4] = x * 2 + 1;
            tris[i + 2] = tris[i + 3] = (x + 1) * 2;
            tris[i + 5] = (x + 1) * 2 + 1;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        return mesh;
    }

    void OnBaitBallStatusChange(bool isBaitBall)
    {
        foreach(var boid in boids)
        {
            boid.isBaitBalling = isBaitBall;
        }
    }

    private void FixedUpdate()
    {
        if(isTerrorSphere)
        {
            sphere.SetActive(true);
            spherePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spherePos.z = 0;
            sphere.transform.position = spherePos;
        }
        else
        {
            sphere.SetActive(false);
        }

        BoidData[] boidDatas = new BoidData[numberOfBoids + (useFocusBoid ? 1 : 0)];
        for (int i = 0; i < numberOfBoids + (useFocusBoid ? 1 : 0); i++)
        {
            boidDatas[i].pos = boids[i].pos;
            boidDatas[i].heading = boids[i].heading;
            boidDatas[i].isBaitBalling = boids[i].isBaitBalling == true ? 1 : 0;
        }

        int boidDataSize = (sizeof(float) * 3) * 7 + sizeof(float) * 2 + sizeof(int) * 2;

        ComputeBuffer buffer = new ComputeBuffer(numberOfBoids + (useFocusBoid ? 1 : 0), boidDataSize);
        buffer.SetData(boidDatas);

        boidComputeShader.SetBuffer(0, "boidDatas", buffer);
        boidComputeShader.SetInt("dataLength", numberOfBoids + (useFocusBoid ? 1 : 0));
        boidComputeShader.SetFloat("tankRadius", constraintRadius);
        boidComputeShader.SetFloat("avgSpeed", (maxSpeed + minSpeed) / 2);
        boidComputeShader.SetFloat("viewRange", viewRange);
        boidComputeShader.SetFloat("avoidRange", avoidRange);
        boidComputeShader.SetBool("isTerrorSphere", isTerrorSphere);
        boidComputeShader.SetFloat("terrorSphereDiameter", terrorShereDiameter);
        boidComputeShader.SetVector("terrorSpherePos", spherePos);
        boidComputeShader.SetVector("baitBallPos", baitBallPos);

        int numThreadGroups = Mathf.CeilToInt((numberOfBoids + (useFocusBoid ? 1 : 0)) / (float)1024);
        boidComputeShader.Dispatch(0, numThreadGroups, 1, 1);

        buffer.GetData(boidDatas);
        for (int i = 0; i < numberOfBoids + (useFocusBoid ? 1 : 0); i++)
        {
            boids[i].distToCenter = boidDatas[i].distToCenter;
            boids[i].constraintMultiplier = boidDatas[i].constraintMultiplier;
            boids[i].separationHeading = boidDatas[i].separationHeading;
            boids[i].avgNeighborHeading = boidDatas[i].avgNeighborHeading;
            boids[i].avgNeighborPos = boidDatas[i].avgNeighborPos;
            boids[i].fear = boidDatas[i].fear;
            boids[i].numNeighbors = boidDatas[i].numNeighbors;
            boids[i].circlePos = boidDatas[i].circlePos;
            boids[i].UpdateBoid();
        }

        buffer.Release();
    }

    public struct BoidData
    {
        public Vector3 pos;
        public Vector3 heading;

        public float distToCenter;
        public float constraintMultiplier;

        public Vector3 separationHeading;
        public Vector3 avgNeighborHeading;
        public Vector3 avgNeighborPos;
        public Vector3 fear;
        public int numNeighbors;

        public int isBaitBalling;
        public Vector3 circlePos;
    }

    void OnEnable()
    {
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Tools.hidden = false;
    }
}
