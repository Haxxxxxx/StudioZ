using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;
    
    [Header("Settings")]
    [SerializeField] private bool allowMusic = true;
    [SerializeField] private bool allowSFX = true;
    [SerializeField] private bool allowVoice = true;

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!allowMusic || clip == null) return;
        
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (!allowSFX || clip == null) return;
        
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float delay)
    {
        if (!allowSFX || clip == null) return;

        StartCoroutine(DelayedSFX(clip, delay));
    }

    private System.Collections.IEnumerator DelayedSFX(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (!allowVoice || clip == null) return;
        
        voiceSource.PlayOneShot(clip);
    }
}