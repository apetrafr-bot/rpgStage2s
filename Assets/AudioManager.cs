using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("AudioSources")]
    public AudioSource musicSource;
    public AudioSource talkSource;

    [Header("Musiques")]
    public AudioClip mainSceneMusic;
    public AudioClip dungeonMusic;
    public AudioClip bossMusic;
    public AudioClip gameOverMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneMusic(scene.name);
    }

    public void PlaySceneMusic(string sceneName)
    {
        AudioClip clip = null;

        if (sceneName.Contains("Donjon") || sceneName.Contains("dungeon"))
            clip = dungeonMusic;
        else if (sceneName.Contains("Boss") || sceneName.Contains("boss"))
            clip = bossMusic;
        else
            clip = mainSceneMusic;

        PlayMusic(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying && musicSource.clip != null)
            musicSource.UnPause();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayTalkSound(AudioClip clip)
    {
        if (clip == null) return;
        if (talkSource == null) talkSource = gameObject.AddComponent<AudioSource>();

        talkSource.Stop();
        talkSource.clip = clip;
        talkSource.loop = true;
        talkSource.Play();
    }

    public void ResumeTalkSound()
    {
        if (talkSource != null && talkSource.clip != null && !talkSource.isPlaying)
            talkSource.UnPause();
    }

    public void PauseTalkSound()
    {
        if (talkSource != null && talkSource.isPlaying)
            talkSource.Pause();
    }

    public void StopTalkSound()
    {
        if (talkSource != null)
            talkSource.Stop();
    }
}
