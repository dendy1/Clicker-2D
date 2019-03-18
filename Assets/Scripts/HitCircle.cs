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
    
    public float MinScale { get; set; } = 0.5f;

    public float Scale
    {
        get { return approachCircleTransform.localScale.x; }
    }

    private Color circleColor, approachColor;
    private SpriteRenderer circleSR, approachSR;

    private Transform approachCircleTransform;
    
    // Start is called before the first frame update
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

    // Update is called once per frame
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
            if (Dead != null)
                Dead(approachCircleTransform.localScale.x);
            
            Destroy(gameObject);
        }  
    }
}
