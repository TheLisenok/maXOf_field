using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnUI : MonoBehaviour
{
    // Переменная для проверки объекта на курсоре
    private bool onUI;

    private void OnMouseEnter()
    {
        // Курсор на интерфейсе
        onUI = true;
    }
    private void OnMouseLeave() 
    {  
        // Курсор вышел с интерфейса
        onUI = false;
    }
}
