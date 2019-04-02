using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour, IPointerClickHandler
{
    float lastClick = 0f;
    float interval = 0.4f;
    public string mapPath;

    public void OnPointerClick(PointerEventData eventData)
    {
        if ((lastClick + interval) > Time.time)
        {
            //doubleclick
            GameManager.mapPath = mapPath;
 
            Application.LoadLevel("MainScene");
            //SceneManager.LoadScene("MainScene");
        }
        else
        {
            lastClick = Time.time;
        }
    }
}
