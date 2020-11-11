using System;

public interface IDailyProcess
{
    void ProcessDay(Action callback);
}
