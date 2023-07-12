using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FronkonGames.TinyTween;
using Ease = FronkonGames.TinyTween.Ease;
/// <summary>
/// Drag card.
/// </summary>
[RequireComponent(typeof(Collider))]
public sealed class CardDrag : MonoBehaviour, IDrag
{
    public bool IsDraggable { get; private set; } = true;

    public bool Dragging { get; set; }
    public Vector3 DragOriginPosition { get; set; }

    [SerializeField]
    private Ease riseEaseIn = Ease.Linear;

    [SerializeField]
    private Ease riseEaseOut = Ease.Linear;

    [SerializeField, Range(0.0f, 5.0f)]
    private float riseDuration = 0.2f;

    [SerializeField]
    private Ease dropEaseIn = Ease.Linear;

    [SerializeField]
    private Ease dropEaseOut = Ease.Linear;

    [SerializeField, Range(0.0f, 5.0f)]
    private float dropDuration = 0.2f;

    [SerializeField]
    private Ease invalidDropEase = Ease.Linear;

    [SerializeField, Range(0.0f, 5.0f)]
    private float invalidDropDuration = 0.2f;


    public void OnPointerEnter(Vector3 position) { }

    public void OnPointerExit(Vector3 position) { }

    public void OnBeginDrag(Vector3 position)
    {
        DragOriginPosition = transform.position;

        IsDraggable = false;

        TweenFloat.Create()
          .Origin(DragOriginPosition.y)
          .Destination(position.y)
          .Duration(riseDuration)
          .EasingIn(riseEaseIn)
          .EasingOut(riseEaseOut)
          .OnUpdate(tween => transform.position = new Vector3(transform.position.x, tween.Value, transform.position.z))
          .OnEnd(_ => IsDraggable = true)
          .Owner(this)
          .Start();
    }

    public void OnDrag(Vector3 deltaPosition, IDrop droppable)
    {
        deltaPosition.y = 0.0f;
        transform.position += deltaPosition;
    }

    public void OnEndDrag(Vector3 position, IDrop droppable)
    {
        if (droppable is { IsDroppable: true } && droppable.AcceptDrop(this) == true)
            TweenFloat.Create()
              .Origin(transform.position.y)
              .Destination(position.y)
              .Duration(dropDuration)
              .EasingIn(dropEaseIn)
              .EasingOut(dropEaseOut)
              .OnUpdate(tween => transform.position = new Vector3(transform.position.x, tween.Value, transform.position.z))
              .Owner(this)
              .Start();
        else
        {
            IsDraggable = false;

            transform.TweenMove(DragOriginPosition, invalidDropDuration, invalidDropEase).OnEnd(_ => IsDraggable = true);
        }
    }

    private void OnEnable()
    {
        DragOriginPosition = transform.position;
    }
}