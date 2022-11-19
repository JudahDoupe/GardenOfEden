using System.Diagnostics;

public static class DebugUtils
{
    public static Stopwatch StartTimer()
    {
        var timer = new Stopwatch();
        timer.Restart();
        return timer;
    }
}