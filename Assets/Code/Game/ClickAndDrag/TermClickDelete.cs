using System.Collections;
using System.Collections.Generic;
using Lambda;
using TypeUtil;
using UnityEngine;

public class TermClickDelete : MonoBehaviour, TermClickHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleClick(SymbolManager manager, Shrub<Sum<Combinator, Variable>> term, List<int> path, LayoutTracker root)
    {
        Destroy(manager.RemoveAtAndReturn(path,root).gameObject);
    }
}
