using System;

namespace SteamworkUploader;

public class ProgressClass(Action<float> callback) : IProgress<float>
{
    private float lastValue = 0;
    public void Report(float value)
    {
        if (lastValue >= value) return;
        lastValue = value;
        callback(value);
    }
}