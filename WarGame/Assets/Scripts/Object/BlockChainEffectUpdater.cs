using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockChainEffectUpdater : MonoBehaviour
{
    public GameObject outgoingEffect;
    public GameObject incomingEffect;
    private ParticleSystem outEffect, inEffect;

    public void Start()
    {
        outEffect = outgoingEffect.GetComponent<ParticleSystem>();
        inEffect = incomingEffect.GetComponent<ParticleSystem>();
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
