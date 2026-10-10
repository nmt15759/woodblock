using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private const string VolumeKey = "BlockPuzzle_Volume";
    private const string BrightnessKey = "BlockPuzzle_Brightness";

    // Độ sáng thấp nhất cho phép (1 = sáng nhất). Không cho về 0 để màn hình không bị đen hẳn.
    private const float MinBrightness = 0.2f;

    // true khi panel đang mở. Block dùng cờ này để không cho kéo block phía sau panel.
    public static bool IsOpen { get; private set; }

    [Header("Scene menu (tên phải giống chính xác tên file scene)")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Panel")]
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button closeButton;
    [FormerlySerializedAs("quitButton")]
    [SerializeField] private Button menuButton; // nút "Quit" cũ, nay quay về menu

    [Header("Âm lượng / độ sáng")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Image brightnessOverlay; // Image đen phủ full màn hình

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

        // Áp dụng giá trị đã lưu ngay khi vào game
        SetVolume(volume);
        SetBrightness(brightness);
    }

    private void Update()
    {
        // Phím Esc mở/đóng panel (hữu ích khi test trên máy tính)
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
        // Ảnh hưởng toàn bộ âm thanh trong game (nhạc nền lẫn hiệu ứng)
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void SetBrightness(float value)
    {
        // Unity không chỉnh được độ sáng màn hình thật,
        // nên làm tối bằng cách phủ một lớp đen trong suốt lên trên game
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