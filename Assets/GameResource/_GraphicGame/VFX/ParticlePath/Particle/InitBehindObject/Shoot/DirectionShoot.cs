using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class TrailsFollowing : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;


    [SerializeField]
    private UnityEngine.Vector3 oldPosition;

    [SerializeField]
    private float delayTimeSetOldPosition = 1;

    [SerializeField]
    private Vector3 direction;

    private float timer = 0f;
    void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= delayTimeSetOldPosition)
        {
            timer = 0f;

            direction = Vector3.Normalize( oldPosition - transform.position);
            visualEffect.SetVector3("OldPosition", direction);

        }

        if(timer == 0)
            oldPosition = transform.position;

    }


}
