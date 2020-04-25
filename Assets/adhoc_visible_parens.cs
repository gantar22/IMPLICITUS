using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class adhoc_visible_parens : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var lts = GetComponentsInChildren<LayoutTracker>();
        if (lts.Length == 1)
        {
            var c = lts[0].GetComponent<Image>().color;
            lts[0].GetComponent<Image>().color = new Color(c.r, c.g, c.b, 1);
            Destroy(lts[0].GetComponent<HighlightParen>());
        }
        else if (lts.Length > 1)
        {
            print("2");
            var c = lts[0].GetComponent<Image>().color;
            lts[0].GetComponent<Image>().color = new Color(c.r, c.g, c.b, 0);
            Destroy(lts[0].GetComponent<HighlightParen>());
        }
    }
}
