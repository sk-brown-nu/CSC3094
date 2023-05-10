using UnityEngine;
using TMPro;

/// <summary>
/// A simple script that displays the current frame rate or frame time.
/// </summary>
/// <remarks>
/// This class is adapted from tutorial by CatLike Coding
/// found at https://catlikecoding.com/unity/tutorials/basics/measuring-performance/.
/// </remarks>
public class FrameRateCounter : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI display;

    public enum DisplayMode { FPS, MS }

    [SerializeField]
    DisplayMode displayMode = DisplayMode.FPS;

    [SerializeField, Range(0.1f, 2f)]
    float sampleDuration = 1f;

    int frames;

    float duration, bestDuration = float.MaxValue, worstDuration;

    void Update()
    {
        float frameDuration = Time.unscaledDeltaTime;
        frames += 1;
        duration += frameDuration;

        if (frameDuration < bestDuration)
        {
            bestDuration = frameDuration;
        }
        if (frameDuration > worstDuration)
        {
            worstDuration = frameDuration;
        }

        if (duration >= sampleDuration)
        {
            if (displayMode == DisplayMode.FPS)
            {
                display.SetText(
                    "FPS\nBest: {0:0}\nAvg: {1:0}\nWorst: {2:0}",
                    1f / bestDuration,
                    frames / duration,
                    1f / worstDuration
                );
            }
            else
            {
                display.SetText(
                    "MS\nBest: {0:1}\nAvg: {1:1}\nWorst: {2:1}",
                    1000f * bestDuration,
                    1000f * duration / frames,
                    1000f * worstDuration
                );
            }
            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
        }
    }
}