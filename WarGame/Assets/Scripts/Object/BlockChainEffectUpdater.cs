using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockChainEffectUpdater : MonoBehaviour
{
    public GameObject outgoingEffect;
    public GameObject incomingEffect;
    private ParticleSystem outEffect, inEffect;

    public void Awake()
    {
        outEffect = outgoingEffect.GetComponent<ParticleSystem>();
        inEffect = incomingEffect.GetComponent<ParticleSystem>();
    }

    public void Start()
    {
        StopEffects();
    }

    public void StopEffects()
    {
        outEffect.Stop();
        inEffect.Stop();
    }

    public void TransmissionEffect()
    {
        outEffect.Play();
    }

    public void ReceiverEffect()
    {
        inEffect.Play();
    }

}
