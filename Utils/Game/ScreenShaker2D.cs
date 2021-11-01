// ScreenShaker2D: shakes a 2D camera to a specified magnitude, for a specified duration
// Usage:
// ScreenShaker2D shaker = new ScreenShaker2D(my2DCamera, duration:0.2f, magnitude:4);
// shaker.Shake();

using Godot;
using System;

public class ScreenShaker2D
{

    private Camera2D _camera;
    private Random _rand = new Random();
    private Vector2 _initialOffset;
    // private float _duration;
    // private int _magnitude;

    public ScreenShaker2D(Camera2D camera)//, float duration = 0.18f, int magnitude = 3)
    {
        _camera = camera;
        // _duration = duration;
        // _magnitude = magnitude;
        _initialOffset = _camera.Offset;
    }

    // Call this externally to cause the shake effect
    public async void Shake(float duration, int magnitude)
    {
        // Upon a new shake, start time at 0
        float time = 0;

        // Until time elapsed passes the specified duration of the shake
        while (time < duration)
        {   
            // Increment _time but not beyond max duration
            time += _camera.GetProcessDeltaTime();
            time = Math.Min(time, duration);

            // Every frame set the offset to a random x and y value within the specified magnitude
            // So with a larger magnitude the offset will be greater and the screen will appear to shake more
            Vector2 offset = new Vector2();
            offset.x = (float) _rand.Next(-magnitude, magnitude + 1);
            offset.y = (float) _rand.Next(-magnitude, magnitude + 1);
            _camera.Offset = _initialOffset + offset;

            // Must be called otherwise the screen will freeze throughout the loop
            await _camera.ToSignal(_camera.GetTree(), "idle_frame");
        }

        // After the shake set time back to 0 so we can shake all over again when needed
        _camera.Offset = _initialOffset;
    }

}