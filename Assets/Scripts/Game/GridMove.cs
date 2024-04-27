using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour
{
    [SerializeField] private Transform cameraPos; // Позиция камеры в данный момент
    [SerializeField] private float zOffsetOfCamera = 10f; // Отступ от начала координат по z оси, чтобы сетка была сзади
    [SerializeField] private int cellMultiplicity = 2; // Размер клетки
    
    private Transform gridTransform; // Позиция сетки в данный момент
    void Awake()
    {
        // Задаём позицию сетки в переменную
        gridTransform = gameObject.transform;
        
        // Если камеры нет, то задаём ту, которая на сцене основная
        if (cameraPos == null)
        {
            cameraPos = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        // Находим позицию, используя математическое округление и учитывая размер клетки
        var pos = new Vector3(Mathf.Round(cameraPos.position.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(cameraPos.position.y / cellMultiplicity) * cellMultiplicity, zOffsetOfCamera);
        
        // Переносим сетку по этой позиции
        gridTransform.position = pos;
    }
}
