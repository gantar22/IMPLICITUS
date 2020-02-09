using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(TMPro.TMP_Text))]
[ExecuteInEditMode]
public class set_text : MonoBehaviour
{
    [SerializeField] private StringRef s;

    private string old_text;
    
    private TMPro.TMP_Text txt;
    // Start is called before the first frame update
    void Start()
    {
        txt = GetComponent<TMPro.TMP_Text>();
        StartCoroutine(makeVis());
        txt.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            txt.text = s.val;
            txt.maxVisibleCharacters = txt.text.Length;
        }
    }

    IEnumerator makeVis()
    {
        while (true)
        {
            txt.maxVisibleCharacters = txt.text.Length;
            while(txt.maxVisibleCharacters > 0)
            {
                txt.maxVisibleCharacters--;
                yield return new WaitForSeconds(.005f);
            }
            yield return  new WaitForSeconds(.5f);
            txt.text = s.val;
            while(txt.maxVisibleCharacters < txt.text.Length && txt.text.Equals(s.val))
            {
                txt.maxVisibleCharacters++;
                yield return new WaitForSeconds(.25f / txt.text.Length + .005f);
            }
            yield return new WaitUntil(() => !txt.text.Equals(s.val));
        }
        
    }


    public int first_dif<T>(IEnumerable<T> a, IEnumerable<T> b,int c) 
    {
        if (!a.Any() || !b.Any()) return c;
        if (!a.First().Equals(b.First()))
            return c;
        else
            return first_dif(a.Skip(1), b.Skip(1), c + 1);
    }
    
}
