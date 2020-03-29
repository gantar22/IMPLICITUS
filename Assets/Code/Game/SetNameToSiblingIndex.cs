using TMPro;
using UnityEngine;

public class SetNameToSiblingIndex : MonoBehaviour
{
    [SerializeField] private int offset;
    [SerializeField] private TMP_Text text;
    private void Awake()
    {
        text.text = (transform.GetSiblingIndex() + offset).ToString();
    }
}
    