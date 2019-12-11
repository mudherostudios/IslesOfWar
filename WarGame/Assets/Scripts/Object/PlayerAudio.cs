using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource environmentSource;
    public AudioSource environmentEntranceSource;
    public AudioSource guiSource;
    public AudioSource musicSource;
    public float fadeTimer;

    private AudioSource queuedSource;
    private AudioClip queuedClip;
    private float queuedTime;
    private bool isDoneTiming;

    [Header("Submission Sounds")]
    public AudioClip genericSuccess;
    public AudioClip genericFailure;
    public AudioClip nationSuccess;
    public AudioClip collectorSuccess;
    public AudioClip collectorFailure;
    public AudioClip defenseSuccess;
    public AudioClip defenseFailure;
    public AudioClip unitPurchaseSuccess;
    public AudioClip unitPurchaseFailure;
    public AudioClip searchSuccess;
    public AudioClip searchFailure;
    public AudioClip poolSuccess;
    public AudioClip poolFailure;
    public AudioClip warbucksSuccess;
    public AudioClip warbucksFailure;
    public AudioClip submitSuccess;
    public AudioClip submitFailure;
    public AudioClip resourcePackSuccess;
    public AudioClip resourcePackFailure;

    [Header("Cancel Sounds")]
    public AudioClip genericCancel;
    public AudioClip troopCancel;
    public AudioClip tankCancel;
    public AudioClip aircraftCancel;
    public AudioClip oilCancel;
    public AudioClip metalCancel;
    public AudioClip concreteCancel;
    public AudioClip warbucksCancel;
    public AudioClip searchCancel;
    public AudioClip developmentCancel;
    public AudioClip nationCancel;
    public AudioClip attackCancel;
    public AudioClip defendCancel;
    public AudioClip resourcePackCancel;

    public void Start()
    {
        queuedTime = Time.time;
        isDoneTiming = true;
        queuedSource = gameObject.AddComponent<AudioSource>();
    }

    public void Update()
    {
        if (!isDoneTiming)
        {
            CrossFadeEnvironmentAudio();

            if (Time.time - queuedTime >= fadeTimer)
            {
                SwapEnvironmentClips();
                isDoneTiming = true;
            }
        }
    }

    void CrossFadeEnvironmentAudio()
    {
        float inVolume = (Time.time - queuedTime) / fadeTimer;
        float outVolume = 1.0f - inVolume;

        queuedSource.volume = inVolume;
        environmentSource.volume = outVolume;
    }

    void SwapEnvironmentClips()
    {
        environmentSource.Stop();
        environmentSource.clip = queuedClip;
        environmentSource.volume = 1.0f;
        environmentSource.time = queuedSource.time;

        queuedSource.Stop();
        environmentSource.Play();

        queuedSource.clip = null;
        queuedSource.volume = 0.0f;
    }

    public void QueueAmbientEnvironmentSound(AudioClip environmentClip)
    {
        if (environmentClip != queuedClip)
        {
            queuedClip = environmentClip;
            queuedSource.Stop();
            queuedSource.clip = queuedClip;
            queuedSource.volume = 0.0f;
            queuedSource.Play();

            queuedTime = Time.time;
            isDoneTiming = false;
        }
    }

    public void PlayEnvironmentEntranceSound(AudioClip environmentClip)
    {
        if (environmentClip != null)
        {
            environmentEntranceSource.Stop();
            environmentEntranceSource.PlayOneShot(environmentClip);
        }
    }

    public void PlayGUISound(int type, string clipName=null)
    {
        AudioClip tempClip = genericFailure;
        AudioClip tempGeneric = genericFailure;

        if (type == 0)
        {
            tempClip = GetSuccessAudioClip(clipName);
            tempGeneric = genericSuccess;
        }
        else if (type == 1)
        {
            tempClip = GetFailureAudioClip(clipName);
            tempGeneric = genericFailure;
        }
        else if (type == 2)
        {
            tempClip = GetCancelAudioClip(clipName);
            tempGeneric = genericCancel;
        }
        else
        {
            tempClip = null;
            tempGeneric = null;
        }

        PlayGUISound(tempGeneric, tempClip);
    }

    public AudioClip GetSuccessAudioClip(string audioClipVariableName)
    {
        switch (audioClipVariableName)
        {
            case null:
                return null;
            case "nationSuccess":
                return nationSuccess;
            case "collectorSuccess":
                return collectorSuccess;
            case "defenseSuccess":
                return defenseSuccess;
            case "unitPurchaseSuccess":
                return unitPurchaseSuccess;
            case "searchSuccess":
                return searchSuccess;
            case "poolSuccess":
                return poolSuccess;
            case "warbucksSuccess":
                return warbucksSuccess;
            case "submitSuccess":
                return submitSuccess;
            case "resourcePackSuccess":
                return resourcePackSuccess;
            default:
                return genericSuccess;
        }
    }

    public AudioClip GetFailureAudioClip(string audioClipByVariableName)
    {
        switch (audioClipByVariableName)
        {
            case null:
                return null;
            case "collectorFailure":
                return collectorFailure;
            case "defenseFailure":
                return defenseFailure;
            case "unitPurchaseFailure":
                return unitPurchaseFailure;
            case "searchFailure":
                return searchFailure;
            case "poolFailure":
                return poolFailure;
            case "warbucksFailure":
                return warbucksFailure;
            case "submitFailure":
                return submitFailure;
            case "resourcePackFailure":
                return resourcePackFailure;
            default:
                return genericFailure;
        }
    }

    public AudioClip GetCancelAudioClip(string audioClipByVariableName)
    {
        switch (audioClipByVariableName)
        {
            case null:
                return null;
            case "nationCancel":
                return nationCancel;
            case "troopCancel":
                return troopCancel;
            case "tankCancel":
                return tankCancel;
            case "aircraftCancel":
                return aircraftCancel;
            case "oilCancel":
                return oilCancel;
            case "metalCancel":
                return metalCancel;
            case "concreteCancel":
                return concreteCancel;
            case "searchCancel":
                return searchCancel;
            case "warbucksCancel":
                return warbucksCancel;
            case "attackCancel":
                return attackCancel;
            case "defendCancel":
                return defendCancel;
            case "resourcePackCancel":
                return resourcePackCancel;
            default:
                return genericCancel;
        }
    }

    void PlayGUISound(AudioClip genericClip, AudioClip guiClip)
    {
        if (guiClip != null)
            PlayGUISound(guiClip);
        else
            PlayGUISound(genericClip);
    }

    public void PlayGUISound(AudioClip guiClip)
    {
        if (guiClip != null)
        {
            guiSource.Stop();
            guiSource.PlayOneShot(guiClip);
        }
    }
}
