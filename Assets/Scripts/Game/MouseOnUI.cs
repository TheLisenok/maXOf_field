using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnUI : MonoBehaviour
{
    private bool onUI;

    private void OnMouseEnter()
    {
        onUI = true;
        Debug.Log(onUI);
    }
    private void OnMouseLeave() 
    {  
        onUI = false;
        Debug.Log(onUI);
    }
    private void Update()
    {
        //Debug.Log(EventSystem.current.currentSelectedGameObject);
    }
}
