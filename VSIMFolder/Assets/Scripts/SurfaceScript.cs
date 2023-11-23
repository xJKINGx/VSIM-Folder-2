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
using System.Runtime.CompilerServices;
using UnityEditor.VersionControl;

public class SurfaceScript : MonoBehaviour
{
    //int pointsCount;

    //string terrainPath = @"D:\Github Clones\VSIM-Folder\VSIMFolder\Assets\Height Data\vertices.txt";

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

    private SurfaceBounds bounds;
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
        // RenderParams rp = new RenderParams(material);
        // rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds
        // rp.matProps = new MaterialPropertyBlock();
        // rp.matProps.SetBuffer("_Triangles", meshTriangles);
        // rp.matProps.SetBuffer("_Positions", meshPositions);
        // rp.matProps.SetBuffer("_VertexPositions", vertexPositions);
        // rp.matProps.SetInt("_StartIndex", (int)mesh.GetIndexStart(0));
        // rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
        // rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        // rp.matProps.SetFloat("_NumInstances", 10.0f);
        // Graphics.RenderPrimitives(rp, MeshTopology.Triangles, (int)mesh.GetIndexCount(0), pointsCount);
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
        
        // Here we're giving the graph shader the maximum height value
        // This is needed to give the correct colors to the surface
        meshRenderer.sharedMaterial.SetFloat(Shader.PropertyToID("_Max_Height"), bounds.yMax);

        // Debugging
        Debug.Log("Min z-value: " + bounds.yMin);
        Debug.Log("Max z-value: " + bounds.yMax);
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
        }

        // Setting the public vertices list to be equal to the verts array
        vertices = verts.ToList();

        // Here we fill the bounds struct, this will mostly be needed for 
        // calculating barycentric coordinates
	    float xMin = 0.0f, yMin = 0.0f, zMin = 0.0f, x, y, z, res = 0.0f;
	    float xMax = 0.0f, yMax = 0.0f, zMax = 0.0f;

        for (int i = 0; i < verticesAmount; i++)
        {
            // We assign the current values to the x, y and z variables
            x = vertices[i].x;
            y = vertices[i].y;
            z = vertices[i].z;

            // if this is the first run through, we assume the first values are both
            // the smallest and the largest values that exist
            if (i == 0)
            {
                xMax = xMin = x;
                yMax = yMin = y;
                zMax = zMin = z;
            }
            // if we can reach this if sentence that means there are at least 2 points
            // we can then find the resolution/step length 
            if (i == 1)
            {
                res = vertices[i].z - vertices[0].z;
            }

            // Here we check the different values to find the largest and smallest ones
            // The ternary oparator makes it work like this:
            // if xMax is less than x, then set xMax to x, otherwise keep xMax the way it is
            xMax = xMax < x ? x : xMax;
            xMin = xMin > x ? x : xMin;
            yMax = yMax < y ? y : yMax;
            yMin = yMin > y ? y : yMin;
            zMax = zMax < z ? z : zMax;
            zMin = zMin > z ? z : zMin;
        }

        // Here we set the bounds variable with the values we've found
        bounds = new SurfaceBounds(xMax, xMin, yMax, yMin, zMax, zMin, res);
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

    public CollisionInfo CalcBaryCoords(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 pos)
    {
        Vector3 a = new Vector3(0, 0, 0);
        Vector3 b = new Vector3(0, 0, 0);

        // First we need to find which grid cell the position is in
        int i = (int)(pos.x / 2.5f); // i is used for the x-axis
        int j = (int)(pos.z / 2.5f); // j is used for the z-axis

        i = i < 0 ? i : 0;
        i = i > 1 ? i : 1;
        j = j < 0 ? j : 0;
        j = j > 1 ? j : 1;

        Vector3 v10 = v1 - v0;
        Vector3 v20 = v2 - v0;

        float area = Vector3.Cross(v10, v20).z;

        Vector3 v0p = v0 - pos;
        Vector3 v1p = v1 - pos;
        Vector3 v2p = v2 - pos;

        float u = Vector3.Cross(v0p, v1p).z / area;
        float v = Vector3.Cross(v1p, v2p).z / area;
        float w = Vector3.Cross(v2p, v0p).z / area;

        int neighbourIndex = 0;

        if (u < 0 || v < 0 || w < 0)
        {
            // The position given is not within the current triangle

            // Find out which neighbour the object might be in
            if (u < v) { neighbourIndex = 0; }
            else if (v < w) { neighbourIndex = 1; }
            else {neighbourIndex = 2; }
        }

        return new CollisionInfo(a, b, 1);
    }

    // pos is Vector2 since we don't know the height, we therefore pass the x- and z-values
    // into the function
    public float FetchHeight(Vector2 pos)
    {
        
        return 0.0f;
    }

}