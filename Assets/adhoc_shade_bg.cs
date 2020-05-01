using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class adhoc_shade_bg : MonoBehaviour
{
    [SerializeField] private BoolRef forwardmod;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(flipper());
    }
    IEnumerator flipper()
    {
        transform.localRotation = Quaternion.Euler(0, 0,0);
        GetComponent<Image>().color = new Color(0.96f, 0.96f, 0.96f);
        while (true)
        {
            yield return new WaitUntil(() => !forwardmod.val);
            transform.localRotation = Quaternion.Euler(0, 0,180);
            GetComponent<Image>().color = new Color(0.86f, 0.86f, 0.86f);
            
            yield return new WaitUntil(() => forwardmod.val);
            transform.localRotation = Quaternion.Euler(0, 0,0);
            GetComponent<Image>().color = new Color(0.96f, 0.96f, 0.96f);
        }
    }
}
