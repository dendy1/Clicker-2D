using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private DefaultAsset mapFile; 
    [SerializeField] private AudioClip songFile;
    [SerializeField] private GameObject circle; //HitCircle Prefab
    [SerializeField] private float musicOffset; //Offset in ms
    [SerializeField] private AudioClip hitFile;

    [Header("Text Fields")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text gameOverText;

    [Header("Audio Players")]
    private AudioSource hitPlayer;
    private AudioSource musicPlayer;

    public static float ApproachRate { get; set; } = GetApproachRateMs(4); //ApproachRate in ms

    private List<Circle> circlesRaw;
    private int currentObject = 0;
    
    [Header("Health Settings")]
    private float healthBar = 1f;
    [SerializeField] private float healthClick = 0.2f;
    [SerializeField] private float healthMiss = 0.2f;
    [SerializeField] private float healthRate = 0.005f;

    [Header("Score Settings")]
    private int score = 0;
    [SerializeField] private float baseClickPoints = 10.0f;
    [SerializeField] private float baseMissPoints = 10.0f;
    [SerializeField] private int scoreScale = 10;

    // Start is called before the first frame update
    void Start()  
    {
        circlesRaw = new List<Circle>();
        musicPlayer = gameObject.AddComponent<AudioSource>();
        hitPlayer = gameObject.AddComponent<AudioSource>();
        musicPlayer.clip = songFile;
        hitPlayer.clip = hitFile;
        StartGame();
    }

    private void FixedUpdate()
    {
        healthText.text = "Health: " + healthBar;
        scoreText.text = "Score: " + score;
        if (healthBar > 0)
            healthBar -= healthRate / 200;
    }

    private List<Circle> Parser(string path)
    {
        StreamReader reader = new StreamReader(path);
        List<string> hitObjects = new List<string>();

        //Skip to [HitObjects] section
        while (reader.ReadLine() != "[HitObjects]"){ }

        while (true)
        {
            string line = reader.ReadLine();
            
            if (line == null)
                break;
            
            hitObjects.Add(line);
        }
        
        //Removing sliders
        for (int i = 0; i < hitObjects.Count; i++)
        {
            if (hitObjects[i].Contains("|"))
                hitObjects.RemoveAt(i);
        }
        
        List<Circle> hitCircles = new List<Circle>();
        for (int i = 0; i < hitObjects.Count - 1; i++)
        {
            string[] circleParams = hitObjects[i].Split(',');
            
            Vector3 screenPos = new Vector3(
                int.Parse(circleParams[0]),  
                384 - int.Parse(circleParams[1]), 
                0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            
            Circle newCircle = new Circle(
                worldPos.x, 
                worldPos.y, 
                int.Parse(circleParams[2])
            );
            
            hitCircles.Add(newCircle);
        }
       
        reader.Close();
        return hitCircles;
    }

    private void SetHitCircles()
    {
        string path = AssetDatabase.GetAssetPath(mapFile);
        circlesRaw = Parser(path);
    }

    private void StartGame()
    {
        SetHitCircles();
        circle.GetComponent<HitCircle>().Clicked += HitCircleClicked;
        circle.GetComponent<HitCircle>().Dead += HitCircleDead;
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
        while (currentObject != circlesRaw.Count - 1 && healthBar > 0f)
        {
            Circle current = circlesRaw[currentObject];
            double timer = musicPlayer.time;
            double delay = (current.Time - ApproachRate) / 1000f;
            
            if (timer >= delay)
            {
                zbuffer += bufferS;
                GameObject temp = Instantiate(circle, new Vector3(current.X, current.Y, zbuffer), Quaternion.identity);
                temp.GetComponent<HitCircle>().Clicked += HitCircleClicked;
                temp.GetComponent<HitCircle>().Dead += HitCircleDead;
                currentObject++;
            }

            yield return null;
        }    
        
        yield return new WaitForSeconds(9f);
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
    
    private static float GetApproachRateMs(int ar)
    {
        switch (ar)
        {
            case 0:
                return 1800;
            case 1:
                return 1680;
            case 2:
                return 1560;
            case 3:
                return 1440;
            case 4:
                return 1320;
            case 5:
                return 1200;
            case 6:
                return 1050;
            case 7:
                return 900;
            case 8:
                return 750;
            case 9:
                return 600;
            case 10:
                return 450;
        }
        throw new Exception();
    }
}
