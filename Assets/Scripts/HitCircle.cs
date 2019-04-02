using System.Collections;
using DG.Tweening;
using UnityEngine;


public class HitCircle : MonoBehaviour
{
    public delegate void ClickAction();
    public event ClickAction Dead;
    public float dieOffset;// ms
    public float MinScale { get; } = 0.95f;
    
    private float _fadeInTime;
    private bool _dead;
    
    public float Scale
    {
        get { return _approachCircleTransform.localScale.x; }
    }
    
    private Transform _approachCircleTransform;
    
    void Start()
    {
        _fadeInTime = GameManager.Instance.FadeIn;
        _approachCircleTransform = transform.GetChild(0);
        
        SpriteRenderer approachSr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer overlaySr = transform.GetChild(1).GetComponent<SpriteRenderer>();
        SpriteRenderer circleSr = transform.GetChild(2).GetComponent<SpriteRenderer>();
        
        circleSr.DOFade(1.0f, _fadeInTime / 1000);
        approachSr.DOFade(1.0f, _fadeInTime / 1000);
        overlaySr.DOFade(1.0f, _fadeInTime / 1000);
    }

    void Update()
    {
        if (_approachCircleTransform.localScale.x < MinScale)
            StartCoroutine("Die");
    }
    
    private IEnumerator Die()
    {
        Debug.Log("test");
        yield return new WaitForSeconds(dieOffset/1000f);
        _dead = true;
        Dead?.Invoke();
        Destroy(gameObject);  
    }
    
    private void OnDestroy()
    {
        if (!_dead)
        {
            if (Mathf.Abs(Scale - MinScale) < 0.15f)
            {
                GameManager.Instance.PerfectHit();
            }
            else if (Mathf.Abs(Scale - MinScale) < 0.4f)
            {
                GameManager.Instance.NormalHit();
            }
            else
            {
                GameManager.Instance.BadHit();
            }
        }
    }
}