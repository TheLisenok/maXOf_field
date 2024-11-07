using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBackgroundParticle : MonoBehaviour
{
    // Список с эффектами заднего фона
    [SerializeField] private List<GameObject> particles = new List<GameObject>();


    void Awake()
    {
        // На старте отключаем все эффекты
        foreach(var part in particles)
        {
            part.SetActive(false);
        }
        
        // И включаем один рандомный
        particles[Random.Range(0, particles.Count)].SetActive(true);
    }
}
