using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private CanvasGroup levelSelect;
    [SerializeField] private CanvasGroup codex;
    [SerializeField] private CanvasGroup credits;

#pragma warning restore 0649

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    public void Play()
    {
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
        //switch to codex
    }

    public void Credits()
    {
        //switch to credits
    }


    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else 
        Application.Quit();
#endif
    }
}
