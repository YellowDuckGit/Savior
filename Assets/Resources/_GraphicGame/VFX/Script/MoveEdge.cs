using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MoveEdge : MonoBehaviour
{
    //[SerializeField]
    //private List<Vector3> points = new List<Vector3>();
    //[SerializeField]
    //private float speed;
    //private int currentWaypointIndex = 0; 

    //void Start()
    //{
    //    SceneView.lastActiveSceneView.drawGizmos = false;
    //    for (int i = 0; i < points.Count; i++)
    //    {
    //        points[i]+= transform.position;
    //    }

    //    StartMoving();
    //}

    //public void StartMoving()
    //{
    //    StartCoroutine(MoveToWaypoints());
    //}

    //private IEnumerator MoveToWaypoints()
    //{
    //    while (true)
    //    {
    //        if (currentWaypointIndex >= points.Count)
    //            currentWaypointIndex = 0;

    //        Vector3 targetPosition = points[currentWaypointIndex];

    //        while (transform.position != targetPosition)
    //        {
    //            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    //            //rotation
           
    //            yield return null;
    //        }

    //        currentWaypointIndex++;
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    for (int i = 0; i < points.Count-1; i++)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawLine(points[i]+transform.position, points[i + 1] + transform.position);
    //    }
    //}
}
