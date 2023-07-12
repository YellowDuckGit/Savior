using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MMFeedbackManager : MonoBehaviour
{
    //public static MMFeedbackManager instance;

    [SerializeField]
    private MMFeedbacks MMFeedbackPrefab;

    //DrawCard
    //Last Point is hand
    [SerializeField] private List<Transform> DrawCard_StopPoints;

    private void Awake()
    {
        //if (instance != null && instance != this)
        //{
        //    Debug.LogError("MMFeedbackManager have 2");
        //    Destroy(gameObject);
        //}
        //else
        //{
        //    instance = this;
        //}
    }

    private void Start()
    {
        CreateDrawCardFeedback(gameObject.transform);
        CreateDrawCardFeedback(gameObject.transform);

    }

    /// <summary>
    /// Create Object MMFeedbacks intro a children of target animater
    /// </summary>
    /// <param name="TargetAnimater"></param>
    /// <returns></returns>
    public MMFeedbacks CreateDrawCardFeedback(Transform TargetAnimater)
    {
        MMFeedbacks mMFeedbacks = MMFeedbacks.Instantiate(MMFeedbackPrefab);
        mMFeedbacks.transform.parent = TargetAnimater;

        foreach (Transform t in DrawCard_StopPoints)
        {
            MMFeedbackPosition fbPosition0 = AddFeedBack(ref mMFeedbacks, TargetAnimater);

            fbPosition0.DestinationPositionTransform = t;
        }
        return mMFeedbacks;
    }

    public MMFeedbackPosition AddFeedBack(ref MMFeedbacks mMFeedbacks,Transform TargetAnimater)
    {
        mMFeedbacks.AddFeedback(typeof(MMFeedbackPosition), true);
        MMFeedbackPosition fbPosition = (MMFeedbackPosition)mMFeedbacks.Feedbacks.Last();
        fbPosition.AnimatePositionTarget = TargetAnimater.gameObject;
        fbPosition.InitialPositionTransform = fbPosition.AnimatePositionTarget.transform;
        return fbPosition;
    }
}
