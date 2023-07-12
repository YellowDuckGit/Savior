using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDrop
{
    /// <summary> Is it droppable? </summary>
    public bool IsDroppable { get; }

    /// <summary> Accept an IDrag? </summary>
    /// <param name="drag">Object IDrag.</param>
    /// <returns>Accept or not the object.</returns>
    public bool AcceptDrop(IDrag drag);

    /// <summary> Performs the drop option of an IDrag object. </summary>
    /// <param name="drag">Object IDrag.</param>
    public void OnDrop(IDrag drag);
}