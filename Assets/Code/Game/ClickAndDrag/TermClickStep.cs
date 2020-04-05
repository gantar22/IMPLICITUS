using System.Collections.Generic;
using Lambda;
using TypeUtil;
using UnityEngine;

public class TermClickStep : MonoBehaviour, TermClickHandler
{
    public void HandleClick(SymbolManager manager, Shrub<Sum<Combinator, Variable>> term, List<int> path, LayoutTracker root)
    {
        print("Click happened");
    }
}