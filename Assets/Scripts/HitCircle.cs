using System.Collections;
using UnityEngine;

public class HitCircle : MonoBehaviour
{
    public delegate void ClickAction(float ptsScale);
    public event ClickAction Clicked;
    public event ClickAction Dead;
    public float dieOffset = 100f;// ms
    
    public float MinScale { get; set; } = 0.86f;

    private float fadein;
    public float Scale
    {
        get { return approachCircleTransform.localScale.x; }
    }

    private Color circleColor, approachColor, overlayColor;
    private SpriteRenderer circleSR, approachSR, overlaySR;
    private Transform approachCircleTransform;
    
    void Start()
    {
        approachCircleTransform = transform.GetChild(0);
        
        circleSR = GetComponent<SpriteRenderer>();
        approachSR = transform.GetChild(0).GetComponent<SpriteRenderer>();
        overlaySR = transform.GetChild(1).GetComponent<SpriteRenderer>();
        
        circleColor = circleSR.material.color;
        approachColor = approachSR.material.color;
        overlayColor = overlaySR.material.color;
        
        circleColor.a = 0f;
        approachColor.a = 0f;
        overlayColor.a = 0f;
        
        circleSR.material.color = circleColor;
        approachSR.material.color = approachColor;
        overlaySR.material.color = overlayColor;

        fadein = GameManager.GetFadein();
        Debug.Log(GameManager.GetPreemt() + "|||" + GameManager.GetFadein());
        StartCoroutine("FadeIn");
    }

    void Update()
    {
        if (approachCircleTransform.localScale.x < MinScale)
            StartCoroutine("Die");
    }

    private IEnumerator FadeIn()
    {
        while (circleColor.a < 1f)
        {
            circleColor.a += 1f / (fadein / 60);
            approachColor.a += 1f / (fadein / 60);
            overlayColor.a += 1f / (fadein / 60);
            
            circleSR.material.color = circleColor;
            approachSR.material.color = approachColor;
            overlaySR.material.color = overlayColor;
            
            yield return new WaitForSeconds(60 / 1000f);
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