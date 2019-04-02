using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{       
    public static GameManager Instance { get; set; }
    
    [Header("Map Settings")]
    //public static DefaultAsset mapFile;

    public static string mapPath;
    [SerializeField] private GameObject circle; //HitCircle Prefab
    [SerializeField] private float musicOffset; //Offset in ms
    [SerializeField] private float dieOffset; //HitCircle die offset in ms
    
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip missSound;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button restartButton;

    public Slider sliderBar;
    public Text scoreText;
    public Text percentText;
    public Texture2D cursorTexture;
    
    [Header("Audio Players")]
    private AudioSource _notePlayer;
    private AudioSource _musicPlayer;

    private float _approachRate;

    private List<CircleClass> _osuObjects;
    private int _currentObject = 0;
    
    [Header("Health Settings")]
    private float _currentHealth = 1f;
    [SerializeField] private float healthClick = 0.05f;
    [SerializeField] private float healthMiss = 0.05f;
    [SerializeField] private float healthDecreasingRate = 0.005f;

    [Header("Score Settings")]
    private int _currentScore = 0;

    private int _badHitScore = 50;
    private int _normalHitScore = 100;
    private int _perfectHitScore = 300;

    // Start is called before the first frame update
    void Start()
    {
        _osuObjects = new List<CircleClass>();
        _musicPlayer = gameObject.AddComponent<AudioSource>();
        _notePlayer = gameObject.AddComponent<AudioSource>();
        _notePlayer.clip = hitSound;
        
        Cursor.SetCursor(cursorTexture,  new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.Auto);
        StartGame();
    }
    
    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        if (_currentHealth > 0 && _currentObject < _osuObjects.Count)
        {
            _currentHealth -= healthDecreasingRate / 200;
            sliderBar.value = _currentHealth;
        }
    }

    private void SetHitCircles()
    {
        //string path = AssetDatabase.GetAssetPath(mapFile);
        Parser parser = new Parser();

        AudioClip temp;
        Debug.Log(mapPath);
        _osuObjects = parser.Parse(mapPath, out temp, out _approachRate);
        _musicPlayer.clip = temp;
    }

    private void StartGame()
    {
        SetHitCircles();
        StartCoroutine("UpdateCoroutine");  
        StartCoroutine("MouseChecker");
    }

    public void RestartGame()
    {
        _currentScore = 0;
        _currentHealth = 1f;
        _currentObject = 0;
        returnButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        StartCoroutine("UpdateCoroutine");
    }

    private IEnumerator UpdateCoroutine()
    {
        float zbuffer = -0.000001f;
        float bufferS = 0.0001f;
        _musicPlayer.PlayDelayed(musicOffset / 1000);
        while (_currentObject != _osuObjects.Count - 1 && _currentHealth > 0f)
        {
            CircleClass current = _osuObjects[_currentObject];
            double timer = _musicPlayer.time;
            double delay = (current.Time - Preemt) / 1000f;
        
            if (timer >= delay)
            {
                zbuffer += bufferS;
                GameObject temp = Instantiate(circle, new Vector3(current.X, current.Y, zbuffer), Quaternion.identity);
                var comp = temp.GetComponent<HitCircle>();
                comp.Dead += NoteMiss;
                comp.dieOffset = dieOffset;
                _currentObject++;
            }
            yield return null;
        }

        while (_musicPlayer.isPlaying && _currentHealth > 0)
        {
            yield return null;
        }

        returnButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    private IEnumerator MouseChecker()
    {
        while (_musicPlayer.isPlaying)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction, Mathf.Infinity);

            if (hit.collider != null)
            {
                if ((Input.GetMouseButtonDown(0) || 
                     Input.GetButtonDown("LeftClick") || 
                     Input.GetButtonDown("RightClick")))
                {
                    Destroy(hit.transform.gameObject);
                }
            }
            yield return null;
        }
    }

    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(5f);
        RestartGame();
    }

    public void ReturnToMenu()
    {
        Application.LoadLevel("SongSelection");
    }

    public void BadHit()
    {
        _currentScore += _badHitScore;
        NoteHit();
    }

    public void NormalHit()
    {
        _currentScore += _normalHitScore;
        NoteHit();
    }

    public void PerfectHit()
    {
        _currentScore += _perfectHitScore;

        NoteHit();
    }

    private void NoteMiss()
    {
        float health = _currentHealth - healthMiss;
        
        if (health > 0)
            _currentHealth = health;
        else
            _currentHealth = 0;
        
        sliderBar.value = _currentHealth;
        percentText.text = Percentage.ToString("P", CultureInfo.InvariantCulture);
        _notePlayer.clip = missSound;
        _notePlayer.Play();
    }

    private void NoteHit()
    {
        float health = _currentHealth + healthClick;
        
        if (health < 1.0f)
            _currentHealth = health;
        else
            _currentHealth = 1;

        sliderBar.value = _currentHealth;
        scoreText.text = $"{_currentScore:00000000}";
        percentText.text = Percentage.ToString("P", CultureInfo.InvariantCulture);
        _notePlayer.clip = hitSound;
        _notePlayer.Play();
    }

    public float Preemt
    {
        get
        {
            if (_approachRate < 5)
            {
                return 1200 + 600 * (5 - _approachRate) / 5f;
            }

            if (_approachRate == 5)
            {
                return 1200;
            }
        
            return 1200 - 750 * (_approachRate - 5) / 5f;
        }
    }

    public float FadeIn
    {
        get
        {
            if (_approachRate < 5)
            {
                return 800 + 400 * (5 - _approachRate) / 5f;
            }

            if (_approachRate == 5)
            {
                return 800;
            }
        
            return 800 - 500 * (_approachRate - 5) / 5f;
        }
    }

    private float Percentage
    {
        get { return (float)_currentScore / (_currentObject * _perfectHitScore); }
    }
}
