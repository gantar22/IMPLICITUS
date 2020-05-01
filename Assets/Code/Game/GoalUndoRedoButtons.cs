using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalUndoRedoButtons : MonoBehaviour
{

    [SerializeField] private Button undo;

    [SerializeField] private Button redo;
    [SerializeField] private SymbolManager smt;

    private bool shouldBeHiddenUndo;

    private bool shouldBeHiddenRedo;
    // Start is called before the first frame update
    void Start()
    {
        undo.onClick.AddListener(() => StartCoroutine(undoRoutine()));
        redo.onClick.AddListener(() => StartCoroutine(redoRoutine()));
        
    }

    IEnumerator undoRoutine()
    {
        shouldBeHiddenUndo = true;
        yield return smt.popForwards();
        shouldBeHiddenUndo = false;
    }
    
    
    IEnumerator redoRoutine()
    {
        shouldBeHiddenRedo = true;
        yield return smt.popBackwards(_ => {});
        shouldBeHiddenRedo = false;
    }

    // Update is called once per frame
    void Update()
    {
        undo.interactable = smt.HasForStack() && !shouldBeHiddenUndo;
        redo.interactable = smt.HasBackStack() && !shouldBeHiddenRedo;
    }
}
