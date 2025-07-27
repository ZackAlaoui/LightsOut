using UnityEngine;

public class audiomanager : MonoBehaviour
{
    [Header("-----Audio Source-----")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-----Audio clip-----")]
    public AudioClip background;
    public AudioClip fire;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

}
