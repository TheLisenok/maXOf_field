using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBackgroundParticle : MonoBehaviour
{
    // ������ � ��������� ������� ����
    [SerializeField] private List<GameObject> particles = new List<GameObject>();


    void Awake()
    {
        // �� ������ ��������� ��� �������
        foreach(var part in particles)
        {
            part.SetActive(false);
        }
        
        // � �������� ���� ���������
        particles[Random.Range(0, particles.Count)].SetActive(true);
    }
}
