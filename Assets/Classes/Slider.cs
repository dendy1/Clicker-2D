using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

public enum SliderTypes
{
    Linear, Perfect, Bezier, Catmull
}

public class Slider
{
    public SliderTypes Type { get; set; }
    public int Time { get; set; }
    public List<Point> CurvePoints { get; set; }
    public int Repeat { get; set; }

    public Slider(int time, SliderTypes type, List<Point> pts, int repeat)
    {
        Time = time;
        Type = type;
        CurvePoints = pts;
        Repeat = repeat;
    }

    public static SliderTypes GetType(char l)
    {
        switch (l)
        {
            case 'L':
                return SliderTypes.Linear;
            case 'P':
                return SliderTypes.Perfect;
            case 'B':
                return SliderTypes.Bezier;
            case 'C':
                return SliderTypes.Catmull;
            default:
                throw new Exception("Неверный тип slider");
        }
    }

    private string ss()
    {
        string res = "";
        for (int i = 0; i < CurvePoints.Count; i++)
        {
            res += CurvePoints[i].X + ";" + CurvePoints[i].Y + "|";
        }

        return res;
    }
    public override string ToString()
    {
        return ss() + Time + "|" + Type + "|" + Repeat;
    }
}
