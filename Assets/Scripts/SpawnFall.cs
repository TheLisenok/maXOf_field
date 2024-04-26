using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


// Никто не должен увидеть этот скрипт...
public class SpawnFall : MonoBehaviour
{
    [SerializeField] private GameObject[] figures;
    int i = 0;

    int kadr = 0;

    private void Start()
    {
    }

    void Update()
    {
        if (kadr % 90 == 0)
        {
            Instantiate(figures[i % figures.Length], new Vector3(Random.Range(-9f, 9f), gameObject.transform.position.y), Quaternion.Euler(0, 0, Random.Range(-90f, 90f)));
            i++;
        }

        kadr++;
    }

}
