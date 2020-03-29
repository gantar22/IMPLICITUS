using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVariablePrefab_Tester : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;
    private int index;

    public void Set(int i)
    {
        index = i;
        GetComponent<UnityEngine.UI.Image>().sprite = sprites[i];
        Debug.Log(index);
    }
    public void indexCurrent()
    {
        Debug.Log(index);
    }
}
