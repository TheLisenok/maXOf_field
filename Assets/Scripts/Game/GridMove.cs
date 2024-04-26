using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMove : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float zOffsetOfCamera = 10f;
    [SerializeField] private int cellMultiplicity = 2;
    
    private Transform gridTransform;
    void Awake()
    {
        gridTransform = gameObject.transform;
        
        if (cameraPos == null)
        {
            cameraPos = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var pos = new Vector3(Mathf.Round(cameraPos.position.x / cellMultiplicity) * cellMultiplicity, Mathf.Round(cameraPos.position.y / cellMultiplicity) * cellMultiplicity, zOffsetOfCamera);
        
        gridTransform.position = pos;
    }
}
