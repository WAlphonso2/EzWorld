using System.Collections;
using UnityEngine;

public class DayNightGenerator : Generator
{
    public Light sun;

    [Range(0, 24)]
    public float timeOfDay = 12;

    public override IEnumerator Generate(WorldInfo worldInfo)
    {
        LoadSettings(worldInfo.dayNightGeneratorData);

        SetTimeOfDay(timeOfDay);

        yield return null;
    }

    public override void Clear()
    {
        SetTimeOfDay(12); // reset to noon
    }

    private void SetTimeOfDay(float timeOfDay)
    {
        sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(-90, 270, timeOfDay / 24f), 0, 0);
    }

    private void OnValidate()
    {
        SetTimeOfDay(timeOfDay);
    }

    private void LoadSettings(DayNightGeneratorData data)
    {
        timeOfDay = data.timeOfDay;
    }
}
