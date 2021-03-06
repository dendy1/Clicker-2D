﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;
using NAudio.Wave;

public class Parser
{
    public List<CircleClass> Parse(string path, out AudioClip music, out float approachRate)
    {
        music = null;
        approachRate = 4;
        
        StreamReader reader = new StreamReader(path);
        List<string> hitObjects = new List<string>();
        
        // Getting song file
        // Skip to [General] section
        while (reader.ReadLine() != "[General]"){ }
        string filename = reader.ReadLine();
        filename = filename.Substring(15, filename.Length - 15);

        string songpath = path.Replace('\\', '/');
        Debug.Log(songpath);
        for (int i = songpath.Length - 1; i > 0; --i)
        {
            if (songpath[i] == '/')
                break;
            songpath = songpath.Remove(i);
        }

        string[] files = Directory.GetFiles(songpath, "*.mp3");
        
        foreach (var song in files)
        {
            if (song.Contains(filename))
            {
                music = LoadSong(song);
            }
        }
        
        
        // Skip to [Difficulty] section
        while (reader.ReadLine() != "[Difficulty]"){ }
        string approachRateString = "4";
        while (true)
        {
            if (approachRateString.Contains("ApproachRate")) 
                break;

            approachRateString = reader.ReadLine();
        }

        approachRateString = approachRateString.Substring(13, approachRateString.Length - 13);
        approachRate = float.Parse(approachRateString.Replace('.', ','));
           
        // Skip to [HitObjects] section
        while (reader.ReadLine() != "[HitObjects]"){ }

        while (true)
        {
            string line = reader.ReadLine();
            
            if (line == null)
                break;
            
            hitObjects.Add(line);
        }
        
        List<CircleClass> objects = new List<CircleClass>();
        
        for (int i = 0; i < hitObjects.Count - 1; i++)
        {
            string[] circleParams = hitObjects[i].Split(',');
            
            int desiredX = Mathf.RoundToInt(int.Parse(circleParams[0]) / 512f * Screen.width);
            int desiredY = Mathf.RoundToInt((384 - int.Parse(circleParams[1])) / 384f * Screen.height);
        
            Vector3 screenPos = new Vector3(desiredX, desiredY, 0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        
            CircleClass newCircleClass = new CircleClass(
                worldPos.x, 
                worldPos.y, 
                int.Parse(circleParams[2])
            );
        
            objects.Add(newCircleClass);
        }
       
        reader.Close();
        return objects;
    }

    AudioClip LoadSong(string path)
    {
        string filename = Path.GetFileNameWithoutExtension(path);
        
        AudioFileReader afr = new AudioFileReader(path);
        int lenght = (int)afr.Length;
        float[] audioData = new float[lenght];
        afr.Read(audioData, 0, lenght);
        
        AudioClip song = AudioClip.Create(filename, lenght, afr.WaveFormat.Channels, afr.WaveFormat.SampleRate, false);
        song.SetData(audioData, 0);

        return song;
    }
}
