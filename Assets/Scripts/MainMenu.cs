using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("MainMenu")]
    [SerializeField] private string gameSceneName = "Play";

    [Header("Nút")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeButton; // nút Close nằm trong How to Play Panel

    [Header("Panel hướng dẫn")]
    [SerializeField] private GameObject howToPlayPanel;

    private void Start()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OpenHowToPlay);
        if (closeButton != null) closeButton.onClick.AddListener(CloseHowToPlay);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);

        // Panel luôn đóng khi vào menu
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    private void Update()
    {
        // Esc đóng panel hướng dẫn nếu đang mở
        if (Input.GetKeyDown(KeyCode.Escape) && howToPlayPanel != null && howToPlayPanel.activeSelf)
        {
            CloseHowToPlay();
        }
    }

    private void OnDestroy()
    {
        if (playButton != null) playButton.onClick.RemoveListener(Play);
        if (howToPlayButton != null) howToPlayButton.onClick.RemoveListener(OpenHowToPlay);
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseHowToPlay);
        if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
    }

    public void Play()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"Không tải được scene '{gameSceneName}'. " +
                           "Kiểm tra lại tên scene và thêm scene vào Build Profiles > Scene List.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenHowToPlay()
    {
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        howToPlayPanel.SetActive(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}