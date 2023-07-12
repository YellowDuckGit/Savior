using System;
using UnityEngine;

public sealed class DragAndDropManager : MonoBehaviour
{
    // Layer of the objects to be detected.
    [SerializeField]
    private LayerMask CardRaycastMask;
    [SerializeField]
    private LayerMask ZoneRaycastMask;


    [SerializeField, Range(0.1f, 2.0f)]
    private float dragSpeed = 1.0f;

    // Height at which we want the card to be in a drag operation.
    [SerializeField, Range(0.0f, 10.0f)]
    private float height = 1.0f;

    [SerializeField]
    private Vector2 cardSize;

    // Object to which we are doing a drag operation
    // or null if no drag operation currently exists.
    private IDrag currentDrag;

    // IDrag objects that the mouse passes over.
    private IDrag possibleDrag;

    // To know the position of the drag object.

    private Transform currentDragTransform;

    private DropZone currentZone;

    // 
    private Transform ZoneTransform;

    // How many impacts of the beam we want to obtain.
    private const int HitsCount = 5;

    // Information on the impacts of shooting a ray.
    private readonly RaycastHit[] raycastHitsCard = new RaycastHit[HitsCount];


    // Information on the impacts of shooting a ray.
    private readonly RaycastHit[] raycastHitsZone = new RaycastHit[1];

    // Information on impacts from the corners of a card.
    private readonly RaycastHit[] cardHits = new RaycastHit[4];

    // Information on impacts from the corners of a card.
    private readonly RaycastHit[] zoneHits = new RaycastHit[1];

    // Ray created from the camera to the projection of the mouse
    // coordinates on the scene.  
    private Ray mouseRay;

    // To calculate the mouse offset (in world-space).
    private Vector3 oldMouseWorldPosition;

    private Vector3 MousePositionToWorldPoint()
    {
        Vector3 mousePosition = Input.mousePosition;
        if (Camera.main.orthographic == false)
            mousePosition.z = 10.0f;

        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void ResetCursor() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

    /// <summary>
    /// Returns the Transfrom of the object closest to the origin
    /// of the ray.
    /// </summary>
    /// <returns>Transform or null if there is no impact.</returns>
    private Transform MouseRaycastCard()
    {
        Transform hit = null;

        // Fire the ray!
        if (Physics.RaycastNonAlloc(mouseRay,
                                    raycastHitsCard,
                                    Camera.main.farClipPlane,
                                    CardRaycastMask) > 0)
        {
            // We order the impacts according to distance.
            System.Array.Sort(raycastHitsCard, (x, y) => x.distance.CompareTo(y.distance));

            // We are only interested in the first one.
            hit = raycastHitsCard[0].transform;
            print("Hit: " + hit);
        }
        return hit;
    }

    private Transform MouseRaycastZone()
    {
        Transform hit = null;

        // Fire the ray!
        if (Physics.RaycastNonAlloc(mouseRay,
                                    raycastHitsZone,
                                    Camera.main.farClipPlane,
                                    ZoneRaycastMask) > 0)
        {
            // We order the impacts according to distance.
            System.Array.Sort(raycastHitsZone, (x, y) => x.distance.CompareTo(y.distance));

            // We are only interested in the first one.
            hit = raycastHitsZone[0].transform;
        }
        return hit;
    }
    /// <summary>Detects an IDrop object under the mouse pointer.</summary>
    /// <returns>IDrop or null.</returns>
    private IDrop DetectDroppable()
    {
        IDrop droppable = null;

        // The four corners of the card.
        Vector3 cardPosition = currentDragTransform.position;
        Vector2 halfCardSize = cardSize * 0.5f;
        Vector3[] cardConner =
        {
      new(cardPosition.x + halfCardSize.x, cardPosition.y, cardPosition.z - halfCardSize.y),
      new(cardPosition.x + halfCardSize.x, cardPosition.y, cardPosition.z + halfCardSize.y),
      new(cardPosition.x - halfCardSize.x, cardPosition.y, cardPosition.z - halfCardSize.y),
      new(cardPosition.x - halfCardSize.x, cardPosition.y, cardPosition.z + halfCardSize.y)
    };

        int cardHitIndex = 0;
        Array.Clear(cardHits, 0, cardHits.Length);

        // We launch the four rays.
        for (int i = 0; i < cardConner.Length; ++i)
        {
            Ray ray = new(cardConner[i], Vector3.down);

            int hits = Physics.RaycastNonAlloc(ray, raycastHitsCard, Camera.main.farClipPlane, CardRaycastMask);
            if (hits > 0)
            {
                // We order the impacts by distance from the origin of the ray.
                Array.Sort(raycastHitsCard, (x, y) => x.transform != null ? x.distance.CompareTo(y.distance) : -1);

                // We are only interested in the closest one.
                cardHits[cardHitIndex++] = raycastHitsCard[0];
            }
        }

        if (cardHitIndex > 0)
        {
            // We are looking for the nearest possible IDrop.
            Array.Sort(cardHits, (x, y) => x.transform != null ? x.distance.CompareTo(y.distance) : -1);

            if (cardHits[0].transform != null)
                droppable = cardHits[0].transform.GetComponent<IDrop>();
        }

        return droppable;
    }

    /// <summary>Detects an IDrag object under the mouse pointer.</summary>
    /// <returns>IDrag or null.</returns>
    public IDrag DetectDraggable()
    {
        IDrag draggable = null;

        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Transform hit = MouseRaycastCard();
        if (hit != null)
        {
            print("hit != null");
            draggable = hit.GetComponent<IDrag>();
            if (draggable is { IsDraggable: true })
                currentDragTransform = hit;
            else
                draggable = null;
        }

        return draggable;
    }

    public DropZone DetectDropZone()
    {
        DropZone zone = null;

        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Transform hit = MouseRaycastZone();
        if (hit != null)
        {
            zone = hit.GetComponent<DropZone>();
            if(zone != null)
            {
                if (zone.isAvaiable)
                    ZoneTransform = hit;
                else
                    zone = null;
            }
          
        }

        return zone;
    }

    private void Update()
    {
        if (currentDrag != null)
        print(currentDrag.DragOriginPosition);

        if (currentDrag == null)
        {
            IDrag draggable = DetectDraggable();
            // Left mouse button pressed?
            if (Input.GetMouseButtonDown(0) == true)
            {
                // Is there an IDrag object under the mouse pointer?
                if (draggable != null)
                {
                    // We already have an object to start the drag operation!
                    currentDrag = draggable;
                    //currentDragTransform = hit;
                    oldMouseWorldPosition = MousePositionToWorldPoint();

                    // Hide the mouse icon.
                    Cursor.visible = false;
                    // And we lock the movements to the window frame,
                    // so we can't move objects out of the camera's view.          
                    Cursor.lockState = CursorLockMode.Confined;

                    // The drag operation begins.
                    currentDrag.Dragging = true;
                    currentDrag.OnBeginDrag(new Vector3(raycastHitsCard[0].point.x, raycastHitsCard[0].point.y + height, raycastHitsCard[0].point.z));
                }
            }
            else
            {

                // Left mouse button not pressed?

                // We pass over a new IDrag?
                if (draggable != null && possibleDrag == null)
                {
                    // We execute its OnPointerEnter.
                    possibleDrag = draggable;
                    possibleDrag.OnPointerEnter(raycastHitsCard[0].point);
                }

                // We are leaving an IDrag?
                if (draggable == null && possibleDrag != null)
                {
                    // We execute its OnPointerExit.
                    possibleDrag.OnPointerExit(raycastHitsCard[0].point);
                    possibleDrag = null;

                    ResetCursor();
                }
            }
        }
        else
        {
            print("Is the left mouse button held down?");


            IDrop droppable = DetectDroppable();

            // Is the left mouse button held down?
            if (Input.GetMouseButton(0) == true)
            {
                // Calculate the offset of the mouse with respect to its previous position.
                Vector3 mouseWorldPosition = MousePositionToWorldPoint();
                Vector3 offset = (mouseWorldPosition - oldMouseWorldPosition) * dragSpeed;

                // OnDrag is executed.
                currentDrag.OnDrag(offset, droppable);

                oldMouseWorldPosition = mouseWorldPosition;
            }
            else if (Input.GetMouseButtonUp(0) == true)
            {
                // The left mouse button is released and
                // the drag operation is finished.
                currentDrag.Dragging = false;
                currentDrag.OnEndDrag(raycastHitsCard[0].point, droppable);
                currentDrag = null;
                currentDragTransform = null;

                // We return the mouse icon to its normal state.
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        //if (currentZone == null)
        //{
        //    DropZone dropZone = DetectDropZone();
        //    // Left mouse button pressed?
        //    if (Input.GetMouseButtonDown(0) == true)
        //    {
        //        print("ZoneButtonDown");
        //        // Is there an IDrag object under the mouse pointer?
        //        if (dropZone != null)
        //        {
        //            // We already have an object to start the drag operation!
        //            currentZone = dropZone;
        //            //currentDragTransform = hit;
        //            oldMouseWorldPosition = MousePositionToWorldPoint();

        //            // Hide the mouse icon.
        //            Cursor.visible = false;
        //            // And we lock the movements to the window frame,
        //            // so we can't move objects out of the camera's view.          
        //            Cursor.lockState = CursorLockMode.Confined;

        //            // The drag operation begins.
        //            currentZone.isOvering = true;

        //            if (currentDrag != null)
        //            {
        //                print("CurrentZone != null && have drag");
        //                currentDrag.DragOriginPosition = ZoneTransform.position;
        //            }
        //            //currentZone.OnBeginDrag(new Vector3(raycastHits[0].point.x, raycastHits[0].point.y + height, raycastHits[0].point.z));
        //        }
        //    }
        //    else
        //    {

        //    }
        //}
        //else
        //{
        //    // Is the left mouse button held down?
        //    if (Input.GetMouseButton(0) == true)
        //    {

        //    }
        //    else if (Input.GetMouseButtonUp(0) == true)
        //    {
        //        currentZone.isOvering = false;
        //        currentZone = null;
        //        ZoneTransform = null;

        //        // We return the mouse icon to its normal state.
        //        Cursor.visible = true;
        //        Cursor.lockState = CursorLockMode.None;
        //    }

        //}

        if(Input.GetMouseButton(0) == true)
        {
            DropZone dropZone = DetectDropZone();

            print("ZoneButtonDown");
            // Is there an IDrag object under the mouse pointer?
            if (dropZone != null)
            {
                // We already have an object to start the drag operation!
                currentZone = dropZone;
                //currentDragTransform = hit;
                oldMouseWorldPosition = MousePositionToWorldPoint();

                // Hide the mouse icon.
                Cursor.visible = false;
                // And we lock the movements to the window frame,
                // so we can't move objects out of the camera's view.          
                Cursor.lockState = CursorLockMode.Confined;

                // The drag operation begins.
                currentZone.isOvering = true;

                if (currentDrag != null)
                {
                    print("CurrentZone != null && have drag");
                    currentDrag.DragOriginPosition = ZoneTransform.position;
                }
                //currentZone.OnBeginDrag(new Vector3(raycastHits[0].point.x, raycastHits[0].point.y + height, raycastHits[0].point.z));
            }
        }
        else if(Input.GetMouseButtonUp(0) == true)
        {
            currentZone.isOvering = false;
            currentZone = null;
            ZoneTransform = null;

            // We return the mouse icon to its normal state.
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }

    }

    private void OnEnable()
    {
        possibleDrag = null;
        currentDragTransform = null;

        ResetCursor();
    }
}