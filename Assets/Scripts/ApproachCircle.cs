using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ApproachCircle : MonoBehaviour
{
    private Vector3 currentScale;
    private float step;
    private float minScale;
    
    void Start()
    {
        minScale = transform.GetComponentInParent<HitCircle>().MinScale;
        currentScale = transform.localScale;
        
        float delta = currentScale.x - minScale;
        step = delta / (GameManager.ApproachRate / 20);
    }

    void FixedUpdate()
    {
        if (currentScale.x > minScale)
        {
            currentScale -= Vector3.one * step;
            transform.localScale = currentScale;
        }
    }
}
