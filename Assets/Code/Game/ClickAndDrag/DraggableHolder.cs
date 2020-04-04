using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableHolder : MonoBehaviour
{
    [Serializable]
    public enum DraggableType {NoDragging, LeftPane, Proposal, RedundantParens}


    public DraggableType myType;
}
