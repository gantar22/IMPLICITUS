using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAndDrag : MonoBehaviour
{
    [SerializeField] private SymbolManager SM;

    private float startPosX;
    private float startPosY;
    private bool isBeingHeld = false;

    void Update()
    {
        if (isBeingHeld)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            this.gameObject.transform.localPosition = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, 0);
        }
        
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(this.gameObject);
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            startPosX = mousePos.x - this.transform.localPosition.x;
            startPosY = mousePos.y - this.transform.localPosition.y;

            isBeingHeld = true;
        }
    }

    private void OnMouseUp()
    {
        isBeingHeld = false;

        Drop(this.transform.localPosition);

    }

    private void Drop(Vector3 location) 
    {
        if () // If location is not valid, destroy this object
        {
            Destroy(this);
        }
        else // If location is valid, add to correct location by changing skeleton hierarchy and symbol hierarchy
        {

        }
    }
}
