using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelIntroduction : MonoBehaviour
{
    [SerializeField] IntEvent Initialize;
    [SerializeField] IntEvent emphasizePane;

    [SerializeField] UnitEvent cancelTutorialEvent;

    [SerializeField] GameObject gameObjectText;

    //Hold text for each pane
    [SerializeField] TextAsset[] textToDisplay;

    //State of tutorial: neutral waiting state or emphasizing a pane state, 
    //                   or tutorial cancel all actions state
    enum State {neutral, highlighting, canceled}; 
    
    enum Pane {
        Goal,
        Proposal,
        Variable,
        LeftPane
    };

    private CanvasGroup group;           //Holder for canvas group of canvas
    private GameObject currentTextMesh;  //Holder for currently show textMesh gameobject
    private RectTransform canvasRect;    //Holds Transform of canvas
    int currentPane;                     //Records what pane is being emphasized now
    State currentState; //Records if map is emphasized or not emphsized 


    void Start()
    {
        group = GetComponent<CanvasGroup>();
        canvasRect = GetComponent<RectTransform>();
        currentState = State.neutral; //Initial state is neutral

        Initialize.AddListener(highlight);
        cancelTutorialEvent.AddListener(cancelTutorial);
    }

    //Emphasizes specified Pane
    private void highlight(int chosenPane)
    {
        //If tutorial was canceled, do not highlight anything!
        if (currentState == State.canceled)
            return;


        //If no pane is being highlighted, highlight
        if (currentState == State.neutral)
        {
            emphasizePane.Invoke(chosenPane);
            group.alpha = 0.25f;
            openTextBox(chosenPane);

            currentPane = chosenPane;

            currentState = State.highlighting;
        }

        //If Pane is being highlighted, undo
        else 
        {
            emphasizePane.Invoke(currentPane);
            group.alpha = 1;
            closeTextBox();

            currentState = State.neutral;

            //If A different pane was invoked, highlight that new pane
            if (chosenPane != currentPane)
            {
                emphasizePane.Invoke(chosenPane);
                group.alpha = 0.25f;
                openTextBox(chosenPane);

                currentPane = chosenPane;
                
                currentState = State.highlighting;
            }
        }
    }

    //Creates and places text box onto the canvas
    private void openTextBox(int selectedTextBox)
    {
        //Create the text gameobject
        currentTextMesh = (GameObject)Instantiate(gameObjectText,gameObject.transform);

        
        //CURRENTLY PREFAB IS ALREADY POSITIONED CORRECTLY
        //Position the text box appropriatly
        //RectTransform cTMTransform = currentTextMesh.GetComponent<RectTransform>();
        //cTMTransform.transform.position = (new Vector3(cTMTransform.transform.position.x,
        //                                               cTMTransform.transform.position.y,
        //                                               cTMTransform.transform.position.z ));
        
        
        //Place the correct Text in the textbox
        currentTextMesh.GetComponent<TMP_Text>().text = textToDisplay[selectedTextBox].text;
    }

    //Removes the textbox that is currently present
    private void closeTextBox()
    {
        Destroy(currentTextMesh); 
    }

    //Cancels in tutorial
    private void cancelTutorial()
    {


    }
}
