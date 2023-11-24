using UnityEngine;

public struct CollisionInfo
{
    public CollisionInfo(Vector3 hPos, Vector3 hNormal)
    {
        hitPosition = hPos;
        hitNormal = hNormal;
    }

    public Vector3 hitPosition;
    public Vector3 hitNormal;
}