using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class adhoc_visible_parens : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var lts = GetComponentsInChildren<LayoutTracker>();
        if (lts.Length == 1)
            lts[0].GetComponent<Image>().color = lts[0].GetComponent<Image>().color + new Color(0, 0, 0, 1);
        else if(lts.Length > 1)
            lts[0].GetComponent<Image>().color = lts[0].GetComponent<Image>().color - new Color(0, 0, 0, 1);
    }
}
