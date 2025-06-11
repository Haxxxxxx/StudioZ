using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Collections.Generic;

public class AudioOptions : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;

    [Header("Audio Sources")]
    [SerializeField] private List<AudioSource> musicSources;
    [SerializeField] private List<AudioSource> sfxSources;
    [SerializeField] private List<AudioSource> voiceSources;

    void Awake()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (voiceSlider != null)
            voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
    }

    public void SetMusicVolume(float value)
    {
        foreach (var src in musicSources)
            if (src != null)
                src.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        foreach (var src in sfxSources)
            if (src != null)
                src.volume = value;
    }

    public void SetVoiceVolume(float value)
    {
        foreach (var src in voiceSources)
            if (src != null)
                src.volume = value;
    }
}