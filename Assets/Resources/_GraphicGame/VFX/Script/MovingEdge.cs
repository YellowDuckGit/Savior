using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEdge : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private List<Vector3> points = new List<Vector3>();
    [SerializeField]
    private float speed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        for(int i=0; i<points.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(points[i], points[i+1]);
        }
    }
}
