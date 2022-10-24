using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource Source;

    public Audio[] Audios;

    [System.Serializable]
    public class Audio
    {
        public string id;
        public AudioClip[] AudioClip;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }

    public void PlayAudio(string id)
    {
        var audio = GetAudio(id);

        Source.PlayOneShot(audio.AudioClip[Random.Range(0, audio.AudioClip.Length - 1)]);
    }
    Audio GetAudio(string id)
    {
        foreach (var audio in Audios)
        {
            if (audio.id == id)
                return audio;
        }

        Debug.LogError($"No {id} found");
        return null;
    }
}
