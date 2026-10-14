using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

   
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string gameSceneName = "Play";


    [SerializeField] private AudioSource menuMusic;   
    [SerializeField] private AudioSource playMusic;   
    [SerializeField] private AudioSource touchSfx;      
    [SerializeField] private AudioSource stoneBreakSfx; 

    private void Awake()
    {
     
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (sceneName == menuSceneName)
        {
            SwitchMusic(menuMusic, playMusic);
        }
        else if (sceneName == gameSceneName)
        {
            SwitchMusic(playMusic, menuMusic);
        }
    }

    private static void SwitchMusic(AudioSource toPlay, AudioSource toStop)
    {
        if (toStop != null && toStop.isPlaying) toStop.Stop();
        if (toPlay != null && !toPlay.isPlaying) toPlay.Play();
    }

    private static void SetupSource(AudioSource source, bool loop)
    {
        if (source == null) return;
        source.playOnAwake = false;
        source.loop = loop;
    }

    private AudioSource FindChildSource(string childName)
    {
        var child = transform.Find(childName);
        return child != null ? child.GetComponent<AudioSource>() : null;
    }
    public static void PlayTouch()
    {
        if (Instance != null) Instance.Play(Instance.touchSfx);
    }
    public static void PlayStoneBreak()
    {
        if (Instance != null) Instance.Play(Instance.stoneBreakSfx);
    }

    private void Play(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }
    public static void StopGameMusic()
    {
        if (Instance != null && Instance.playMusic != null)
        {
            Instance.playMusic.Stop();
        }
    }
    public static void PlayGameMusic()
    {
        if (Instance != null && Instance.playMusic != null && !Instance.playMusic.isPlaying)
        {
            Instance.playMusic.Play();
        }
    }
}