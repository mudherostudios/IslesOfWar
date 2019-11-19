using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsElements : MonoBehaviour
{
    public Settings settings;

    [Header("Volume GUI")]
    public Slider guiSlider;
    public Slider environmentSlider;
    public Slider musicSlider;

    [Header("Graphics GUI")]
    public Slider shadowSlider;
    public Slider textureSlider;
    public Slider fpsSlider;
    public Dropdown resolutionMenu;
    public Toggle fullscreenToggle;
    public Toggle aaToggle;
    public Toggle ppToggle;
    public Toggle blurToggle;
    public Toggle grainToggle;
    public Toggle aoToggle;

    public void SetGUIVolume()
    {
        settings.SetGUIVolume();
    }

    public void SetEnvironmentVolume()
    {
        settings.SetEnvironmentVolume();
    }

    public void SetMusicVolume()
    {
        settings.SetMusicVolume();
    }

    public void SetShadowQuality()
    {
        settings.SetShadowQuality();
    }

    public void SetTextureQuality()
    {
        settings.SetTextureQuality();
    }

    public void SetFPS()
    {
        settings.SetFPS();
    }

    public void SetResolution()
    {
        settings.SetScreenResolution();
    }

    public void SetFullscreen()
    {
        settings.SetFullscreen();
    }

    public void SetPostProcessing()
    {
        settings.SetPostProcessing();
    }

    public void SetAntiAliasing()
    {
        settings.SetAntiAliasing();
    }

    public void SetBlur()
    {
        settings.SetBlur();
    }

    public void SetGrain()
    {
        settings.SetGrain();
    }

    public void SetAmbientOcclusion()
    {
        settings.SetAmbientOcclusion();
    }

    public void SaveSettings()
    {
        settings.SaveSettings();
    }

    public void LoadSettings()
    {
        guiSlider.value = settings.settings.guiVolume;
        environmentSlider.value = settings.settings.environmentVolume;
        musicSlider.value = settings.settings.musicVolume;

        shadowSlider.value = settings.settings.shadowQuality;
        textureSlider.value = settings.settings.textureQuality;
        fpsSlider.value = settings.settings.fps;
        fullscreenToggle.isOn = settings.settings.fullscreen;
        aaToggle.isOn = settings.settings.antiAliasing;
        ppToggle.isOn = settings.settings.postProcessing;
        blurToggle.isOn = settings.settings.blur;
        grainToggle.isOn = settings.settings.grain;
        aoToggle.isOn = settings.settings.ambientOcclusion;
    }

    public void ClearCache()
    {
        settings.ClearCache();
        guiSlider.value = 1;
        environmentSlider.value = 1;
        musicSlider.value = 1;
        
        shadowSlider.value = shadowSlider.maxValue;
        textureSlider.value = textureSlider.maxValue;
        fpsSlider.value = textureSlider.maxValue;
        resolutionMenu.value = 0;
        fullscreenToggle.isOn = true;
        aaToggle.isOn = true;
        ppToggle.isOn = true;
        blurToggle.isOn = true;
        grainToggle.isOn = true;
        aoToggle.isOn = true;
    }
}
