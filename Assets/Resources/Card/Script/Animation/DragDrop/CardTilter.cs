using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardTilter : MonoBehaviour
{
    [Header("Pitch")]

    [SerializeField]
    private float pitchForce = 10.0f;

    [SerializeField]
    private float pitchMinAngle = -25.0f;

    [SerializeField]
    private float pitchMaxAngle = 25.0f;

    [Space]

    [Header("Roll")]

    [SerializeField]
    private float rollForce = 10.0f;

    [SerializeField]
    private float rollMinAngle = -25.0f;

    [SerializeField]
    private float rollMaxAngle = 25.0f;

    [Space]

    [SerializeField]
    private float restTime = 1.0f;

    // Pitch angle and velocity.
    private float pitchAngle, pitchVelocity;

    // Roll angle and velocity.
    private float rollAngle, rollVelocity;

    // To calculate the velocity vector.
    private Vector3 oldPosition;

    // The original rotation
    private Vector3 originalAngles;

    private void Awake()
    {
        oldPosition = transform.position;
        originalAngles = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        // Calculate offset.
        Vector3 currentPosition = transform.position;
        Vector3 offset = currentPosition - oldPosition;

        // Limit the angle ranges.
        if (offset.sqrMagnitude > Mathf.Epsilon)
        {
            pitchAngle = Mathf.Clamp(pitchAngle + offset.z * pitchForce, pitchMinAngle, pitchMaxAngle);
            rollAngle = Mathf.Clamp(rollAngle + offset.x * rollForce, rollMinAngle, rollMaxAngle);
        }

        // The angles have 0 with time.
        pitchAngle = Mathf.SmoothDamp(pitchAngle, 0.0f, ref pitchVelocity, restTime * Time.deltaTime * 10.0f);
        rollAngle = Mathf.SmoothDamp(rollAngle, 0.0f, ref rollVelocity, restTime * Time.deltaTime * 10.0f);

        // Update the card rotation.
        transform.rotation = Quaternion.Euler(originalAngles.x + pitchAngle,
                                              originalAngles.y,
                                              originalAngles.z - rollAngle);

        oldPosition = currentPosition;
    }
}