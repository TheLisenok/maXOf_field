using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnUI : MonoBehaviour
{
    // ���������� ��� �������� ������� �� �������
    private bool onUI;

    private void OnMouseEnter()
    {
        // ������ �� ����������
        onUI = true;
    }
    private void OnMouseLeave() 
    {  
        // ������ ����� � ����������
        onUI = false;
    }
}
