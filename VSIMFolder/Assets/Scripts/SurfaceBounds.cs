using System.Xml.Serialization;
using UnityEngine;

public struct SurfaceBounds
{
    public float xMin, xMax, yMin, yMax, zMin, zMax;
    public float xRange, yRange, zRange;
    public float resolution; // This is also known as the step length

    public SurfaceBounds(float xmax, float xmin, float ymax, float ymin, float zmax, float zmin, float res)
    {
        xMax = xmax;
        xMin = xmin;
        yMax = ymax;
        yMin = ymin;
        zMax = zmax;
        zMin = zmin;
        xRange = xMax - xMin;
        yRange = yMax - yMin;
        zRange = zMax - zMin;
        resolution = res;
    }
}