using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Audio;
using Newtonsoft.Json;

public class RawSettings
{
    public float guiVolume;
    public float environmentVolume;
    public float musicVolume;
    public int shadowQuality;
    public int textureQuality;
    public int screenWidth;
    public int screenHeight;
    public int fps;
    public bool fullscreen;
    public bool antiAliasing;
    public bool postProcessing;
    public bool blur;
    public bool grain;
    public bool ambientOcclusion;

    public RawSettings()
    {
        guiVolume = 1.0f;
        environmentVolume = 1.0f;
        musicVolume = 1.0f;
        shadowQuality = 6;
        textureQuality = 4;
        fps = 3;
        screenWidth = 1024;
        screenHeight = 768;
        fullscreen = true;
        antiAliasing = true;
        postProcessing = true;
        blur = true;
        grain = true;
        ambientOcclusion = true;
    }
}

public class Settings : MonoBehaviour
{
    public RawSettings settings;
    public GameObject mainCam;
    public SettingsElements elements;
    public AudioMixer mixer;

    private void Start()
    {
        SaveLoad.LoadPreferences();
        LoadSettings();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "IslandMenu")
        {
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            elements = GameObject.FindGameObjectWithTag("SettingsElements").GetComponent<SettingsElements>();

            if (elements != null)
            {
                elements.settings = this;
                elements.LoadSettings();
                LoadInGameGraphics();
            }
        }
    }

    void LoadSettings()
    {
        settings = SaveLoad.state.settings;
        LoadVolume();
        LoadGraphics();
        if (SceneManager.GetActiveScene().name == "IslandMenu")
            LoadInGameGraphics();
    }

    void LoadVolume()
    {
        if (settings.guiVolume >= 0.001)
            mixer.SetFloat("guiVolume", Mathf.Log(settings.guiVolume) * 20);
        else
            mixer.SetFloat("guiVolume", -80);

        if (settings.environmentVolume >= 0.001)
            mixer.SetFloat("environmentVolume", Mathf.Log(settings.environmentVolume) * 20);
        else
            mixer.SetFloat("environmentVolume", -80);

        if (settings.musicVolume >= 0.001)
            mixer.SetFloat("musicVolume", Mathf.Log(settings.musicVolume) * 20);
        else
            mixer.SetFloat("musicVolume", -80);
    }

    void LoadGraphics()
    {
        LoadShadows();
        QualitySettings.masterTextureLimit = 4 - settings.textureQuality;
        LoadFPS();
        Screen.SetResolution(settings.screenWidth, settings.screenHeight, settings.fullscreen);
    }

    void LoadInGameGraphics()
    {
        if (mainCam != null)
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");

        if (mainCam != null)
        {
            PostProcessVolume volume = mainCam.GetComponent<PostProcessVolume>();
            volume.enabled = settings.postProcessing;

            if (volume.enabled)
            {
                DepthOfField dof = null;
                Grain grain = null;
                AmbientOcclusion ao = null;

                volume.profile.TryGetSettings(out dof);
                volume.profile.TryGetSettings(out grain);
                volume.profile.TryGetSettings(out ao);

                dof.enabled.value = settings.blur;
                grain.enabled.value = settings.grain;
                ao.enabled.value = settings.ambientOcclusion;
            }
        }
    }

    void LoadShadows()
    {
        switch (settings.shadowQuality)
        {
            case 0:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                break;
            case 1:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                break;
            case 2:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                break;
            case 3:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                break;
            case 4:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                break;
            case 5:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                break;
            default:
                break;
        }
    }

    void LoadFPS()
    {
        switch (settings.fps)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 1:
                Application.targetFrameRate = 45;
                break;
            case 2:
                Application.targetFrameRate = 60;
                break;
            default:
                break;
        }
    }

    public void SetGUIVolume()
    {
        settings.guiVolume = elements.guiSlider.value;
        LoadVolume();
    }

    public void SetEnvironmentVolume()
    {
        settings.environmentVolume = elements.environmentSlider.value;
        LoadVolume();
    }

    public void SetMusicVolume()
    {
        settings.musicVolume = elements.musicSlider.value;
        LoadVolume();
    }

    public void SetShadowQuality()
    {
        settings.shadowQuality = (int)elements.shadowSlider.value;
        LoadShadows();
    }

    public void SetTextureQuality()
    {
        settings.textureQuality = (int)elements.textureSlider.value;
        QualitySettings.masterTextureLimit = 4 - settings.textureQuality;
    }

    public void SetFPS()
    {
        settings.fps = (int)elements.fpsSlider.value;
        SetFPS();
    }

    public void SetScreenResolution()
    {
        int[] resolution = GetResolutionFromDropDown();
        settings.screenWidth = resolution[0];
        settings.screenHeight = resolution[1];
        Screen.SetResolution(settings.screenWidth, settings.screenHeight, settings.fullscreen);
    }

    public void SetFullscreen()
    {
        settings.fullscreen = elements.fullscreenToggle.isOn;
        Screen.SetResolution(settings.screenWidth, settings.screenHeight, settings.fullscreen);
    }

    int[] GetResolutionFromDropDown()
    {
        switch (elements.resolutionMenu.value)
        {
            case 0:
                return new int[] { 1024, 768 };
            default:
                return new int[] { 1024, 768 };
        }
    }

    public void SetPostProcessing()
    {
        settings.postProcessing = elements.ppToggle.isOn;
        LoadInGameGraphics();
    }

    public void SetBlur()
    {
        settings.blur = elements.blurToggle.isOn;
        LoadInGameGraphics();
    }

    public void SetGrain()
    {
        settings.grain = elements.grainToggle.isOn;
        LoadInGameGraphics();
    }

    public void SetAmbientOcclusion()
    {
        settings.ambientOcclusion = elements.aoToggle.isOn;
        LoadInGameGraphics();
    }

    public void SetAntiAliasing()
    {
        settings.antiAliasing = elements.aaToggle.isOn;
        LoadInGameGraphics();
    }


    public void SaveSettings()
    {
        SaveLoad.state.settings = settings;
        SaveLoad.SavePreferences();
    }

    public void ClearCache()
    {
        SaveLoad.ResetSaveState(true);
        LoadSettings();
    }
}
