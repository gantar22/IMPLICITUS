using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class disable_button_on_bools : MonoBehaviour
{
    private Button button;

    [SerializeField] private List<BoolRef> bools;

    [SerializeField]  bool negate;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        button.interactable = bools.All(b => negate ? !b.val : b.val);
    }
}
