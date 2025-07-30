using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager s_instance;

    [Header("-----Audio Source-----")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _SFXSource;

    [Header("-----Audio clip-----")]
    [SerializeField] private AudioClip _background;
    public static AudioClip Background { get => s_instance._background; }
    [SerializeField] private AudioClip _fire;
    public static AudioClip Fire { get => s_instance._fire; }

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Debug.LogWarning("AudioManager has already been instantiated. Deleting duplicate AudioManager.");
            Destroy(this.gameObject);
            return;
        }
        else
        {
            s_instance = this;
        }

        if (_musicSource == null) throw new NullReferenceException("Music Source is null.");
        if (_SFXSource == null) throw new NullReferenceException("SFX Source is null.");
    }

	private void Start()
    {
        _musicSource.clip = _background;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public static void PlaySFX(AudioClip clip)
    {
        s_instance._SFXSource.PlayOneShot(clip);
    }

}
