using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Serialization;

public class HitCircle : MonoBehaviour
{
    public delegate void ClickAction(float ptsScale);
    public event ClickAction Clicked;
    public event ClickAction Dead;
    public float dieOffset = 100f;// ms
    
    public float MinScale { get; set; } = 0.86f;

    public float Scale
    {
        get { return approachCircleTransform.localScale.x; }
    }

    private Color circleColor, approachColor;
    private SpriteRenderer circleSR, approachSR;
    private Transform approachCircleTransform;
    
    void Start()
    {
        approachCircleTransform = transform.GetChild(0);
        
        circleSR = GetComponent<SpriteRenderer>();
        approachSR = transform.GetChild(0).GetComponent<SpriteRenderer>();

        circleColor = circleSR.material.color;
        approachColor = approachSR.material.color;
        
        circleColor.a = 0f;
        approachColor.a = 0f;
        
        circleSR.material.color = circleColor;
        approachSR.material.color = approachColor;
    }

    void Update()
    {
        if (approachCircleTransform.localScale.x > MinScale)
        {
            circleColor.a += 4f * Time.deltaTime;
            approachColor.a += 4f * Time.deltaTime;
            circleSR.material.color = circleColor;
            approachSR.material.color = approachColor;
        }
        else
        {
            StartCoroutine("Die");
        }  
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(dieOffset/1000f);
        if (Dead != null)
            Dead(approachCircleTransform.localScale.x);
        Destroy(gameObject);  
    }
}