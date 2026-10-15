using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private const string VolumeKey = "BlockPuzzle_Volume";
    private const string BrightnessKey = "BlockPuzzle_Brightness";

    
    private const float MinBrightness = 0.2f;

  
    public static bool IsOpen { get; private set; }

    [Header("Scene menu (tên phải giống chính xác tên file scene)")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Panel")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button closeButton;
    [FormerlySerializedAs("quitButton")]
    [SerializeField] private Button menuButton;

    [Header("Âm lượng / độ sáng")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Image brightnessOverlay; 

    private void Start()
    {
        IsOpen = false;
        settingPanel.SetActive(false);

        var volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        var brightness = Mathf.Clamp(PlayerPrefs.GetFloat(BrightnessKey, 1f), MinBrightness, 1f);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(volume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = MinBrightness;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.SetValueWithoutNotify(brightness);
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        if (settingButton != null) settingButton.onClick.AddListener(Toggle);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (menuButton != null) menuButton.onClick.AddListener(BackToMenu);

       
        SetVolume(volume);
        SetBrightness(brightness);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    private void OnDestroy()
    {
        IsOpen = false;

        if (volumeSlider != null) volumeSlider.onValueChanged.RemoveListener(SetVolume);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
        if (settingButton != null) settingButton.onClick.RemoveListener(Toggle);
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (menuButton != null) menuButton.onClick.RemoveListener(BackToMenu);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        settingPanel.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        settingPanel.SetActive(false);
        IsOpen = false;
        PlayerPrefs.Save();
    }

    public void BackToMenu()
    {
        PlayerPrefs.Save();

        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError($"Không tải được scene '{menuSceneName}'. " +
                           "Kiểm tra lại tên scene và thêm scene vào Build Profiles > Scene List.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    private void SetVolume(float value)
    {
       
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void SetBrightness(float value)
    {
        
        if (brightnessOverlay != null)
        {
            brightnessOverlay.color = new Color(0f, 0f, 0f, 1f - value);
        }
        PlayerPrefs.SetFloat(BrightnessKey, value);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}