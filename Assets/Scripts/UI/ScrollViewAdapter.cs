using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

public class ScrollViewAdapter : MonoBehaviour
{
    public RectTransform prefab;
    public RectTransform content;

    private List<SongItemModel> _songs;
    
    public class SongItemModel
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }

        public SongItemModel(string artist, string title, string version, string path)
        {
            Artist = artist;
            Title = title;
            Version = version;
            Path = path;
        }
    }
    
    public class SongItemView
    {
        public Text SongName { get; set; }
        public Text Version { get; set; }
        public string Path { get; set; }

        public SongItemView(Transform rootView)
        {
            SongName = rootView.Find("SongNameText").GetComponent<Text>();
            Version = rootView.Find("VersionText").GetComponent<Text>();
        }
    }

    private void Start()
    {
        _songs = new List<SongItemModel>();
        List<string> files = GetOsuFiles(Application.dataPath + "/Songs");

        foreach (var file in files)
        {
            _songs.Add(GetSongItemFromOsuFile(file));
        }

        foreach (var song in _songs)
        {
            var instance = Instantiate(prefab.gameObject, content, false);
            instance.GetComponent<Button>().GetComponent<ButtonController>().mapPath = song.Path;
            InitializeItemView(instance, song);
        }
    }

    private void InitializeItemView(GameObject viewGameObject, SongItemModel songItemModel)
    {
        SongItemView view = new SongItemView(viewGameObject.transform);
        view.SongName.text = songItemModel.Artist + " - " + songItemModel.Title;
        view.Version.text = songItemModel.Version;
        view.Path = songItemModel.Path;
    }
    
    private List<string> GetOsuFiles(string rootDirectory)
    {
        List<string> files = new List<string>();
        
        string[] directories = Directory.GetDirectories(rootDirectory);

        foreach (var directory in directories)
        {
            files.AddRange(Directory.GetFiles(directory, "*.osu"));
        }

        return files;
    }

    private SongItemModel GetSongItemFromOsuFile(string path)
    {
        StreamReader sr = new StreamReader(path);
        
        while (sr.ReadLine() != "[Metadata]") { }

        string temp = "";

        while (!temp.Contains("Title"))
        {
            temp = sr.ReadLine();
        }
        string title = temp.Substring(6, temp.Length - 6);
        
        while (!temp.Contains("Artist"))
        {
            temp = sr.ReadLine();
        }
        string artist = temp.Substring(7, temp.Length - 7);
        
        while (!temp.Contains("Version"))
        {
            temp = sr.ReadLine();
        }
        string version = temp.Substring(8, temp.Length - 8);
        
        return new SongItemModel(artist, title, version, path);
    }
}
