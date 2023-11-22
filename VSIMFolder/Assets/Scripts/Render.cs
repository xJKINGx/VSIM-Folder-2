using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using System;
using UnityEngine.Rendering;
using TMPro;

public class Render : MonoBehaviour
{
    int pointsCount;

    string terrainPath = @"D:\Github Clones\VSIM-Folder\VSIMFolder\Assets\Height Data\vertices.txt";

    GraphicsBuffer meshTriangles;
    GraphicsBuffer vertexPositions;
    GraphicsBuffer meshPositions;

    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    [SerializeField] TextAsset vertexData;
    [SerializeField] TextAsset indexData;

    [SerializeField] Material surfaceMaterial;

    private List<Vector3> vertices { get; set; }
    // Since we need more than a single variable of information to construct a triangle
    // it's easier to make a single list of structs instead multiple lists
    private List<TriangleInfo> triangles { get; set; }

    private Vector2 minMaxHeight;
    void Awake()
    {
        GenerateSurface();
    }

    // void Start() {

    //     List<Vector3> points = new List<Vector3>();

    //     StreamReader read = new StreamReader(terrainPath);

    //     pointsCount = int.Parse(read.ReadLine());

    //     string line;

    //     for (int i = 0; i < pointsCount; i++)
    //     {   
    //         line = read.ReadLine();
    //         List<String> pointValues = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList<string>();
            
    //         if (pointValues.Count() != 3)
    //         {
    //             continue;
    //         }
    //         Vector3 p = new Vector3(float.Parse(pointValues[0], CultureInfo.InvariantCulture.NumberFormat),
    //                                 float.Parse(pointValues[2], CultureInfo.InvariantCulture.NumberFormat),
    //                                 float.Parse(pointValues[1], CultureInfo.InvariantCulture.NumberFormat));
    //         points.Add(p);
    //         //Debug.Log(p);
    //     }

    //     /*
    //     Code below obtained form Unity's documentation on RenderPrimitives
    //     https://docs.unity3d.com/ScriptReference/Graphics.RenderPrimitives.html
    //     */
    //     meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
    //     meshTriangles.SetData(mesh.triangles);
        
    //     meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, pointsCount, 3 * sizeof(float));
    //     meshPositions.SetData(points.ToArray());

    //     vertexPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
    //     vertexPositions.SetData(mesh.vertices);
    // }


    /*
    Function obtained form Unity's documentation on RenderPrimitives
    https://docs.unity3d.com/ScriptReference/Graphics.RenderPrimitives.html
    */
    void OnDestroy()
    {
        meshTriangles?.Dispose();
        meshTriangles = null;
        meshPositions?.Dispose();
        meshPositions = null;
        vertexPositions?.Dispose();
        vertexPositions = null;
    }

    void Update()
    {
        // For pointclouds
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_Triangles", meshTriangles);
        rp.matProps.SetBuffer("_Positions", meshPositions);
        rp.matProps.SetBuffer("_VertexPositions", vertexPositions);
        rp.matProps.SetInt("_StartIndex", (int)mesh.GetIndexStart(0));
        rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        rp.matProps.SetFloat("_NumInstances", 10.0f);
        Graphics.RenderPrimitives(rp, MeshTopology.Triangles, (int)mesh.GetIndexCount(0), pointsCount);
   
        // For surface
        RenderParams srp = new RenderParams(surfaceMaterial);
        srp.matProps = new MaterialPropertyBlock();
    }

    private void GenerateSurface()
    {
        // A quick check to make sure we can access the files
        if (vertexData == null || indexData == null)
        {
            Debug.LogError("Vertex data or Index data not found");
            return;
        }

        // Here we add the components MeshFilter and Meshrenderer.
        // These are needed to create the surface mesh
        var filter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Here we call the GenerateMesh() function which we'll give to the filter
        // This is the mesh that will eventually be rendered
        filter.sharedMesh = GenerateMesh();

        // Here we set the material of the meshRenderer to the surfaceMaterial given
        // in the inspector
        meshRenderer.sharedMaterial = surfaceMaterial;

        // Debugging
        Debug.Log("Min z-value: " + minMaxHeight[0]);
        Debug.Log("Max z-value: " + minMaxHeight[1]);
    }

    // This function puts all indices of all triangles into a singular array
    // this is needed to create the surface mesh as it requires an array of
    // all indices
    private int[] MakeIndexArray()
    {
        // Total indices would be the total amount of triangles * 3, since we have 3 indices
        // per triangle
        var inds = new int[triangles.Count * 3];

        for (int i = 0; i < triangles.Count; i++)
        {
            // j < 3 could also be triangles[i].indices.Length, but since all triangles must have
            // 3 indices, we can just type it outright.
            for (int j = 0; j < 3; j++)
            {
                // Here we place the indices of the triangles in the correct place in
                // the inds arary. As mentioned before, this array will be used to generate
                // the terrain mesh
                inds[3 * i + j] = triangles[i].indices[j];
            }
        }

        return inds;
    }

    private Mesh GenerateMesh()
    {
        Mesh newMesh = new Mesh();

        // This is where we'll fetch the data form the two text files given
        FetchVertices();
        FetchIndices();

        // Changing the index format makes us able to create a mesh with up to 4 billion
        // vertices. UInt16 only allows up to 65536 vertices, which is why we need more.
        newMesh.indexFormat = IndexFormat.UInt32;
        newMesh.vertices = vertices.ToArray();
        newMesh.SetIndices(MakeIndexArray(), MeshTopology.Triangles, 0);

        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        return newMesh;
    }

    private void FetchVertices()
    {
        // This will split the file into lines and put each line as an element in "lines"
        var lines = vertexData.text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

        // Here we get the total amount of vertices in the file
        // This is assuming there is such a number at the top of the vertices.txt file
        int verticesAmount = int.Parse(lines[0]);

        // An array of all the vertices we'll be adding
        var verts = new Vector3[verticesAmount];

        // We start at 1 to avoid having to read the first line containing how
        // many vertices are in the file
        for (int i = 1; i <= verticesAmount; i++)
        {
            // Each line, except the first, looks like this "x y z" each number is seperated
            // by a space " ", here we'll get 3 floats, since there's 3 numbers per line
            var nums = lines[i].Split(new[]{" "}, StringSplitOptions.RemoveEmptyEntries);
            if (nums.Length < 3)
            {
                // This current line does not have enough information to make a vector3
                Debug.LogWarning("Could not make vertex of line " + i);
                continue;
            }

            // i - 1 since we start at 1
            verts[i - 1] = new Vector3
            (
                // InvariantCulture is used since in the vertex file comma (,) is used for the floats
                // while c# likes period (.) to be used.
                // We also swap the y- and z-values so the surface doesn't stand vertically
                float.Parse(nums[0], CultureInfo.InvariantCulture), // x
                float.Parse(nums[2], CultureInfo.InvariantCulture), // z  
                float.Parse(nums[1], CultureInfo.InvariantCulture)  // y

                // The input file has the z-axis as upwards while unity uses the y-axis
                // Swapping theses two values resolves the issue.
            );

            // minMaxHeight[0] is the minimum value, [1] is the maximum value
            if (i == 1)
            {
                // We first assume the first value we get is the min and max z-values
                minMaxHeight[0] = verts[i - 1].y;
                minMaxHeight[1] = verts[i - 1].y;
            }
            else
            {
                // Comparing the values of the following points to find the minmax z-values
                if (verts[i - 1].y < minMaxHeight[0])
                {
                    minMaxHeight[0] = verts[i - 1].y;
                }
                else if (verts[i - 1].y > minMaxHeight[1])
                {
                    minMaxHeight[1] = verts[i - 1].y;
                }
                
            }
        }

        // Setting the public vertices list to be equal to the verts array
        vertices = verts.ToList();
    }

    // FetchIndices works very similarly to the FetchVertices function
    private void FetchIndices()
    {
        var lines = indexData.text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

        int triangleAmount = int.Parse(lines[0]);

        // This list is the public list created at the top of this script
        triangles = new List<TriangleInfo>();
        
        for (int i = 1; i <= triangleAmount; i++)
        {
            var nums = lines[i].Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (nums.Length < 3)
            {
                Debug.LogWarning("Could not make triangle of line " + i);
                continue;   
            }

            // Indices
            int i0 = int.Parse(nums[0]),
                i1 = int.Parse(nums[1]),
                i2 = int.Parse(nums[2]);

            triangles.Add(new TriangleInfo
            (
                // The indices
                i0, i1, i2,
                // nums 3, 4 and 5 are the neighbours
                int.Parse(nums[3]),
                int.Parse(nums[4]),
                int.Parse(nums[5]),
                CreateSurfaceNormal(i0, i1, i2), // Unit surface normal for the triangle
                i - 1 // The index is i - 1 since i starts at 1
            ));
        }
    }

    private Vector3 CreateSurfaceNormal(int i0, int i1, int i2)
    {
        Vector3 normal = new Vector3();

        // We do i1 - i0 and i2 - i0 to create the vectors between the points
        // then we normalize the normal vector.
        normal = Vector3.Cross(vertices[i1] - vertices[i0], vertices[i2] - vertices[i0]).normalized;

        // After the calculation we return the result
        return normal;
    }
}