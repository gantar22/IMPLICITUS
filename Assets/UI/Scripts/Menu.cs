using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private CanvasGroup mainMenu;
    [SerializeField] private GameObject codex;
    [SerializeField] private GameObject credits;

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
        //Load main gameplay scene
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
