using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicAS;
    public AudioSource sfxAS;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlaySFX(AudioClip sfx)
    {
        sfxAS.PlayOneShot(sfx);
    }

    public void PlayRandomPitchSFX(AudioClip sfx)
    {
        sfxAS.pitch = Random.Range(0.5f, 1.5f);
        sfxAS.PlayOneShot(sfx);
    }
}
