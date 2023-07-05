using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotweenCirclePoints : MonoBehaviour
{
    public DOTweenPath path;

    public int ponts;
    public float radius;
    private void Start()
    {
        path.wps = GenerateCircle(ponts,radius);
    }
    public List<Vector3> GenerateCircle(int numPoints, float radius)
    {
        List<Vector3> circlePoints = new List<Vector3>();

        float angleIncrement = 360f / numPoints;

        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleIncrement;
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = 0f;
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * angle);

            Vector3 point = new Vector3(x, y, z);
            circlePoints.Add(point);
        }

        return circlePoints;
    }
}
