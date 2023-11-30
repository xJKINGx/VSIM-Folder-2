using UnityEngine;

/*

	CANDIDATE NUMBER: 840

*/
public struct CollisionInfo
{
    public CollisionInfo(Vector3 hPos, Vector3 hNormal, TriangleInfo hitTri)
    {
        hitPosition = hPos;
        hitNormal = hNormal;
        hitTriangle = hitTri;
    }

    public Vector3 hitPosition;
    public Vector3 hitNormal;
    public TriangleInfo hitTriangle;
}