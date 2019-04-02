using UnityEngine;

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
        step = delta / (GameManager.Instance.Preemt / 20);
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
