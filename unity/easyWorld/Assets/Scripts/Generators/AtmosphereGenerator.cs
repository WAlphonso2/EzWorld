using System.Collections;
using UnityEngine;
public class AtmosphereGenerator : Generator
{
    public Light sun;
    public Light moon;
    public Material skyboxMaterial;

    [Range(0, 24)]
    public float timeOfDay = 12;
    [Range(0, 1)]
    public float sunSize = .05f;
    [Range(0, 5)]
    public float atmosphericThickness = 1;
    [Range(0, 8)]
    public float exposure = 1.3f;
    public Color skyTint = Color.gray;
    [Range(0, 1)]
    public float fogIntensity = 0;
    public Color fogColor = Color.gray;

    public override IEnumerator Generate(WorldInfo worldInfo, int terrainIndex = 0)
    {
        LoadSettings(worldInfo.atmosphereGeneratorData);

        ApplySettings();

        yield return null;
    }

    public override void Clear()
    {
        SetTimeOfDay(12);
        SetSkyTintColor(Color.gray);
        SetSunSize(.05f);
        SetAtmosphericThickness(1);
        SetExposure(1.3f);
        SetFog(0, Color.gray);
    }

    private void ApplySettings()
    {
        SetTimeOfDay(timeOfDay);
        SetSkyTintColor(skyTint);
        SetSunSize(sunSize);
        SetAtmosphericThickness(atmosphericThickness);
        SetExposure(exposure);
        SetFog(fogIntensity, fogColor);
    }

    private void SetTimeOfDay(float timeOfDay)
    {
        sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(-90, 270, timeOfDay / 24f), 0, 0);
        var sunRotation = sun.transform.rotation;
        moon.transform.rotation = new Quaternion(sunRotation.x, sunRotation.y, sunRotation.z, 0);
    }

    private void SetSunSize(float size)
    {
        skyboxMaterial.SetFloat("_SunSize", size);
    }

    private void SetSkyTintColor(Color color)
    {
        skyboxMaterial.SetColor("_SkyTint", color);
    }

    private void SetAtmosphericThickness(float thickness)
    {
        skyboxMaterial.SetFloat("_AtmosphereThickness", thickness);
    }

    private void SetExposure(float thickness)
    {
        skyboxMaterial.SetFloat("_Exposure", thickness);
    }

    private void SetFog(float intensity, Color color)
    {
        if (RenderSettings.fog = intensity >= 0)
        {
            RenderSettings.fogDensity = intensity;
            RenderSettings.fogColor = color;
        }
    }

    private void OnValidate()
    {
        ApplySettings();
    }

    private void LoadSettings(AtmosphereGeneratorData data)
    {
        timeOfDay = data.timeOfDay;
        skyTint = data.skyTint;
        sunSize = data.sunSize;
        atmosphericThickness = data.atmosphericThickness;
        exposure = data.exposure;
        fogIntensity = data.fogIntensity;
        fogColor = data.fogColor;
    }
}
