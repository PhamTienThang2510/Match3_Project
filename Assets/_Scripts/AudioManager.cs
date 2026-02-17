using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip background; // background.ogg
    [SerializeField] private AudioClip swap;       // swap.ogg

    [Header("Playback Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private float musicFadeDuration = 0.5f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Coroutine musicFadeCoroutine;
    private bool muted;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playMusicOnStart && background != null)
            PlayMusic(background, true);
    }

    private void CreateAudioSources()
    {
        // music source (looping)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f; // 2D

        // sfx source (one-shot)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.spatialBlend = 0f; // 2D
    }

    // MUSIC API

    /// <summary>
    /// Play given clip as background music. If already playing same clip, can skip unless force true.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool force = false)
    {
        if (clip == null) return;

        if (musicSource == null) CreateAudioSources();

        if (!force && musicSource.isPlaying && musicSource.clip == clip) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        // start new music with fade
        musicFadeCoroutine = StartCoroutine(FadeToNewMusic(clip, musicFadeDuration));
    }

    public void StopMusic(bool immediate = false)
    {
        if (musicSource == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        if (immediate)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
        else
        {
            musicFadeCoroutine = StartCoroutine(FadeOutMusic(musicFadeDuration));
        }
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip, float duration)
    {
        // fade out current
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
                yield return null;
            }
            musicSource.Stop();
        }

        // switch clip
        musicSource.clip = newClip;
        musicSource.volume = 0f;
        if (!muted) musicSource.Play();

        // fade in to musicVolume
        float target = muted ? 0f : musicVolume;
        float tt = 0f;
        while (tt < duration)
        {
            tt += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, target, tt / duration);
            yield return null;
        }

        musicSource.volume = target;
        musicFadeCoroutine = null;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        if (!musicSource.isPlaying)
        {
            musicFadeCoroutine = null;
            yield break;
        }

        float startVol = musicSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicFadeCoroutine = null;
    }

    // SFX API

    public void PlaySwap()
    {
        if (swap == null) return;
        PlaySFX(swap);
    }

    /// <summary>
    /// Play a one-shot SFX. Volume is multiplied by sfxVolume.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        if (sfxSource == null) CreateAudioSources();

        float vol = Mathf.Clamp01(sfxVolume * volumeScale) * (muted ? 0f : 1f);
        sfxSource.PlayOneShot(clip, vol);
    }

    // SETTINGS / UTIL

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource != null && !muted) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null && !muted) sfxSource.volume = sfxVolume;
    }

    public void ToggleMute(bool? state = null)
    {
        muted = state ?? !muted;
        ApplyMuteState();
    }

    private void ApplyMuteState()
    {
        if (musicSource != null) musicSource.volume = muted ? 0f : musicVolume;
        if (sfxSource != null) sfxSource.volume = muted ? 0f : sfxVolume;
    }

    public bool IsMuted() => muted;

    // Helpers to quickly play default background / swap
    public void PlayBackground() => PlayMusic(background, false);
    public void StopBackground() => StopMusic(false);
}
