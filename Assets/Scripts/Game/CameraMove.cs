using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Cinemachine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam; // Камера
    [SerializeField] private float zoomMin = 1f; // Минимальный зум
    [SerializeField] private float zoomMax = 8f; // Максимальный зум
    [SerializeField][Range(0f, 10f)] private float sensitivity = 1f; // Чувствительность зума
    [SerializeField][Range(0f, 10f)] private float vcamDamping = 1f; // Чувствительность сглаживания камеры

    private Vector3 touchStart; // Позиция начала зажатия ЛКМ (Vector3 из-за того, что камера использует их)


    private void Awake()
    {
        // Задаём чувствительность камеры
        vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = vcamDamping;
        vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = vcamDamping;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Первый кадр после нажатия ПКМ
        {
            // Задаём позицию начала зажатия
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        else if (Input.GetMouseButton(1)) // ПКМ продолжают держать
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition); // Вычисляем дельта позицию цели камеры
            gameObject.transform.position += direction; // Прибавляем дельта позицию к цели камеры
        }

        // Приравниваем масштаб камеры к значению колёсика мыши, без проверки для скорости обработки
        zoom(Input.GetAxis("Mouse ScrollWheel") * sensitivity);
    }

    private void zoom(float increment) // Изменения масштаба камеры
    {
        // Задаём масштаб камеры, при этом выставив ограниченте на мин и макс масштаб
        vcam.m_Lens.OrthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomMin, zoomMax);
    }
}
