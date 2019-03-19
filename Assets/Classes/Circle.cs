using UnityEngine;

public class Circle
{
    /// <summary>
    /// Position of the center of the hit
    /// X ranges from 0 to 512 pixels, inclusive.
    /// Y ranges from 0 to 384 pixels, inclusive.
    /// The origin, (0, 0) is at the top left of the screen.
    /// </summary>
    public Vector2 Position { get; set; }
    
    /// <summary>
    /// Time is an integral number of milliseconds from the beginning of the song, and specifies when the hit begins.
    /// </summary>
    public int Time { get; set; }
    
    public float X
    {
        get { return Position.x; }
    }
    
    public float Y
    {
        get { return Position.y; }
    }

    public Circle(float x, float y, int time)
    {
        Position = new Vector2(x, y);
        Time = time;
    }

    public override string ToString()
    {
        return Position.x + ";" + Position.y + ";" + Time;
    }
}
