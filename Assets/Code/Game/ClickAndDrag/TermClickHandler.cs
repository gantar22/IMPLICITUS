using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Term = TypeUtil.Shrub<TypeUtil.Sum<Combinator,Lambda.Variable>>;

public interface TermClickHandler
{
    void HandleClick(SymbolManager manager, Term term, List<int> path, LayoutTracker root);
}
