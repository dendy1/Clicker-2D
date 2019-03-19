using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private DefaultAsset mapFile; 
    [SerializeField] private GameObject circle; //HitCircle Prefab
    [SerializeField] private float musicOffset; //Offset in ms
    [SerializeField] private float dieOffset; //HitCircle die offset in ms
    [SerializeField] private AudioClip hitFile;
    private AudioClip songFile;

    [Header("Text Fields")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverText;

    [Header("Audio Players")]
    private AudioSource hitPlayer;
    private AudioSource musicPlayer;
    public VideoPlayer vp;

    private static float approachRate;

    private List<Circle> osuObjects;
    private int currentObject = 0;
    
    [Header("Health Settings")]
    private float healthBar = 1f;
    [SerializeField] private float healthClick = 0.05f;
    [SerializeField] private float healthMiss = 0.05f;
    [SerializeField] private float healthRate = 0.005f;

    [Header("Score Settings")]
    private int score = 0;
    [SerializeField] private float baseClickPoints = 10.0f;
    [SerializeField] private float baseMissPoints = 10.0f;
    [SerializeField] private int scoreScale = 10;

    // Start is called before the first frame update
    void Start()
    {
        osuObjects = new List<Circle>();
        musicPlayer = gameObject.AddComponent<AudioSource>();
        hitPlayer = gameObject.AddComponent<AudioSource>();
        hitPlayer.clip = hitFile;
        vp.Play();
        StartGame();
    }

    private void FixedUpdate()
    {
        healthText.text = "Health: " + healthBar;
        scoreText.text = "Score: " + score;
        if (healthBar > 0)
            healthBar -= healthRate / 200;
    }

    private void SetHitCircles()
    {
        string path = AssetDatabase.GetAssetPath(mapFile);
        Parser parser = new Parser();
        
        osuObjects = parser.Parse(path, out songFile, out approachRate);
        musicPlayer.clip = songFile;
        
        circle.GetComponent<HitCircle>().Clicked += HitCircleClicked;
        circle.GetComponent<HitCircle>().Dead += HitCircleDead;
    }

    private void StartGame()
    {
        SetHitCircles();
        StartCoroutine("UpdateCoroutine");
        StartCoroutine("Checker");
    }

    private void RestartGame()
    {
        score = 0;
        healthBar = 1f;
        gameOverText.text = "";
        currentObject = 0;
        StartCoroutine("UpdateCoroutine");
        StartCoroutine("Checker");
    }

    private IEnumerator UpdateCoroutine()
    {
        float zbuffer = -0.000001f;
        float bufferS = 0.0001f;
        musicPlayer.PlayDelayed(musicOffset / 1000);
        while (currentObject != osuObjects.Count - 1 && healthBar > 0f)
        {
            Circle current = osuObjects[currentObject];
            double timer = musicPlayer.time;
            double delay = (current.Time - GetPreemt()) / 1000f;
        
            if (timer >= delay)
            {
                zbuffer += bufferS;
                GameObject temp = Instantiate(circle, new Vector3(current.X, current.Y, zbuffer), Quaternion.identity);
                var comp = temp.GetComponent<HitCircle>();
                comp.Clicked += HitCircleClicked;
                comp.Dead += HitCircleDead;
                comp.dieOffset = dieOffset;
                currentObject++;
            }
            yield return null;
        }

        while (musicPlayer.isPlaying && healthBar > 0)
            yield return null;
         
        musicPlayer.Stop();
        StartCoroutine("Restart");
    }

    private IEnumerator Restart()
    {
        if (healthBar <= 0)
            gameOverText.text = "ВЫ ПРОИГРАЛИ!!";
        else
            gameOverText.text = "ВЫ ПОБЕДИЛИ!!";
        yield return new WaitForSeconds(5f);
        RestartGame();
    }

    private IEnumerator Checker()
    {
        while (musicPlayer.isPlaying)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null)
            {
                if ((Input.GetMouseButtonDown(0) || 
                     Input.GetButtonDown("LeftClick") || 
                     Input.GetButtonDown("RightClick")))
                {
                    float scale = hit.collider.transform.GetComponent<HitCircle>().Scale;
                    HitCircleClicked(scale);
                    Destroy(hit.transform.gameObject);
                }
            }
            yield return null;
        }
    }

    private void HitCircleClicked(float ptsScale)
    {
        if (healthBar + healthClick < 1.0f)
            healthBar += healthClick;
        else
            healthBar = 1;

        score += (int)(baseClickPoints / ptsScale) * scoreScale;
        hitPlayer.Play();
    }
    
    private void HitCircleDead(float ptsScale)
    {
        if (healthBar - healthMiss > 0)
            healthBar -= healthMiss;
        else
            healthBar = 0;
        
        if (score > 0)
            score -= (int)(baseMissPoints / ptsScale) * scoreScale;
    }

    public static float GetPreemt()
    {
        if (approachRate < 5)
        {
            return 1200 + 600 * (5 - approachRate) / 5f;
        }

        if (approachRate == 5)
        {
            return 1200;
        }
        
        return 1200 - 750 * (approachRate - 5) / 5f;
    }
    
    public static float GetFadein()
    {
        if (approachRate < 5)
        {
            return 800 + 400 * (5 - approachRate) / 5f;
        }

        if (approachRate == 5)
        {
            return 800;
        }
        
        return 800 - 500 * (approachRate - 5) / 5f;
    }
}
