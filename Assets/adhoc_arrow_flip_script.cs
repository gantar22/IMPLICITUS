using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class adhoc_arrow_flip_script : MonoBehaviour
{
    [SerializeField] private BoolRef forwardmod;

    [SerializeField] private Sprite forwardsprite;
    [SerializeField] private Sprite backwardsprite;
    
    private Image im;
    // Start is called before the first frame update
    void Start()
    {
        im = GetComponent<Image>();
        StartCoroutine(flipper());
    }

    IEnumerator flipper()
    {
        while (true)
        {
            yield return new WaitUntil(() => !forwardmod.val);
            im.fillOrigin = 1;
            yield return shrink();
            im.sprite = backwardsprite;
            yield return grow();
            
            
            yield return new WaitUntil(() => forwardmod.val);
            im.fillOrigin = 0;
            yield return shrink();
            im.sprite = forwardsprite;
            yield return grow();
        }
    }

    IEnumerator shrink()
    {
        for (float dur = 0; dur < 1; dur += Time.deltaTime * 6 + Time.deltaTime * Mathf.Sqrt(dur) * 15)
        {
            im.fillAmount = 1 - dur;
            yield return null;
        }

        im.fillAmount = 0;
    }

    IEnumerator grow()
    {
        for (float dur = 0; dur < 1; dur += Time.deltaTime * 5 + Time.deltaTime * Mathf.Sqrt(dur) * 5)
        {
            im.fillAmount = dur;
            yield return null;
        }        
        im.fillAmount = 1;

    }
}
