using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private int levelNum;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private TextMeshProUGUI numText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject disabledOverlay;

    [SerializeField] private float moveTime;
    [SerializeField] private AnimationCurve moveCurve;

#pragma warning restore 0649

    private LevelSelect levelSelect;
    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void SetLevelSelect(LevelSelect ls)
    {
        levelSelect = ls;
    }
    
    public void SetLevelNum(int num)
    {
        levelNum = num;
    }

    public void SetCurrentLevel()
    {
        levelLoader.setLevelIndex(levelNum );
    }

    public void ButtonHit()
    {
        SetCurrentLevel();
        levelLoader.loadSelectedLevel();
    }

    public void StoryButton()
    {
        SetCurrentLevel();
        levelSelect.OpenStory();
    }

    public void SetData(int num, string description)
    {
        numText.text = "Level " + num;
        descriptionText.text = description;
    }

    public void SetEnabled(bool _enabled)
    {
        if (_enabled)
        {
            disabledOverlay.SetActive(false);
            cg.alpha = 1;
        }
        else
        {
            disabledOverlay.SetActive(true);
            cg.alpha = 0.5f;
        }
    }

    public void MoveTo(Vector2 position)
    {
        StartCoroutine(AnimateMove(position));
    }

    private IEnumerator AnimateMove(Vector2 position)
    {
        RectTransform rt = GetComponent<RectTransform>();
        float timer = 0;
        Vector2 origPos = rt.anchoredPosition;
        //Vector3 pos = new Vector3(position.x, position.y, transform.position.z);
        while(timer < moveTime)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(origPos, position, moveCurve.Evaluate(timer / moveTime));
        }
    }
}
