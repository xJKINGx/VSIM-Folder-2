using System;
using UnityEngine;

public struct TriangleInfo
{
    public TriangleInfo(int i0, int i1, int i2, int tri0, int tri1, int tri2, Vector3 Normal, int triIndex)
    {
        indices = new[]{i0, i1, i2};
        neighbours = new[]{tri0, tri1, tri2};
        surfaceNormal = Normal;
        index = triIndex;
    }

    public int[] indices { get; }
    public int[] neighbours { get; }

    public Vector3 surfaceNormal { get; }

    // This index is used to differentiate the triangles
    public int index { get; }

}