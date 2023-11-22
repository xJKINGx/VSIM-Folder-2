using UnityEngine;

public struct CollisionInfo
{
    public CollisionInfo(Vector3 hPos, Vector3 hNormal, int hTri)
    {
        hitPosition = hPos;
        hitNormal = hNormal;
        hitTriangle = hTri;
    }

    public Vector3 hitPosition;
    public Vector3 hitNormal;
    public int hitTriangle;
}