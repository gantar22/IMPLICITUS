using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class optionaltext : MonoBehaviour
{
    [SerializeField] private List<BoolRef> toggle;
    [SerializeField] private bool negate = false;
    
    [SerializeField] private TMPro.TMP_Text txt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        txt.alpha = toggle.All(t => negate ? !t.val : t.val) ? 1 : 0;
    }
}
