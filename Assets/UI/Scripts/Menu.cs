using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup levelSelect;
    [SerializeField] private CanvasGroup codex;
    [SerializeField] private CanvasGroup credits;

    [SerializeField] IntEvent effectAudioEvent; //Event Calls audio sound
    [SerializeField] IntEvent songAudioEvent; //Event Calls audio sounds

#pragma warning restore 0649

    private Animator animator;
    private bool onTitle = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    public void TitleToMain()
    {
        if (onTitle)
        {
            onTitle = false;
            animator.SetTrigger("Next");
        }
    }

    public void Play()
    {

        effectAudioEvent.Invoke(0); //Button Press Sound
        songAudioEvent.Invoke(1);   //Plays Level Select Track

        //=============================
        //TODO: Animate this transition
        //=============================
        mainMenu.alpha = 0;
        mainMenu.blocksRaycasts = false;
        mainMenu.interactable = false;
        levelSelect.gameObject.SetActive(true);
        levelSelect.alpha = 1;
        levelSelect.blocksRaycasts = true;
        levelSelect.interactable = true;
    }

    public void BackFromLevel()
    {
        effectAudioEvent.Invoke(0); //Button Press Sound
        songAudioEvent.Invoke(0);   //Play Title Screen Track

        //=============================
        //TODO: Animate this transition
        //=============================
        levelSelect.alpha = 0;
        levelSelect.blocksRaycasts = false;
        levelSelect.interactable = false;
        levelSelect.gameObject.SetActive(false);
        mainMenu.alpha = 1;
        mainMenu.blocksRaycasts = true;
        mainMenu.interactable = true;
        
    }

    public void Codex()
    {
        effectAudioEvent.Invoke(0); //Button Press Sound

        //switch to codex
    }

    public void Credits()
    {
        effectAudioEvent.Invoke(0); //Button Press Sound

        //switch to credits
    }


    public void Quit()
    {
        songAudioEvent.Invoke(-1);   //Turns Off Music
        effectAudioEvent.Invoke(1); //Quit Game Sound

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else 
        Application.Quit();
#endif
    }
}
