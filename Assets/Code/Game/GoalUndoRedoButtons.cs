using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalUndoRedoButtons : MonoBehaviour
{

    [SerializeField] private Button undo;

    [SerializeField] private Button redo;
    [SerializeField] private SymbolManager smt;
    
    // Start is called before the first frame update
    void Start()
    {
        undo.onClick.AddListener(() => StartCoroutine(smt.popForwards()));
        redo.onClick.AddListener(() => StartCoroutine(smt.popBackwards()));
        
    }

    // Update is called once per frame
    void Update()
    {
        undo.interactable = smt.HasForStack();
        redo.interactable = smt.HasBackStack();
    }
}
