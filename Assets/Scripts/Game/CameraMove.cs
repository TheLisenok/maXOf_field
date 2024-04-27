using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Cinemachine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float zoomMin = 1f;
    [SerializeField] private float zoomMax = 8f;
    [SerializeField][Range(0f, 10f)] private float sensitivity = 1f;
    [SerializeField][Range(0f, 10f)] private float vcamDamping = 1f;

    private Vector3 touchStart;


    private void Awake()
    {
        vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_XDamping = vcamDamping;
        vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_YDamping = vcamDamping;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // прав кнопка мыши
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        else if (Input.GetMouseButton(1))
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gameObject.transform.position += direction;
        }

        zoom(Input.GetAxis("Mouse ScrollWheel") * sensitivity);
    }

    private void zoom(float increment)
    {
        vcam.m_Lens.OrthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomMin, zoomMax);
    }
}
