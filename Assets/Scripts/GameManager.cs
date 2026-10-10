using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const string HighScoreKey = "BlockPuzzle_HighScore";

    // Điểm thưởng khi xóa hàng/cột: lines * lines * LineBonus (xóa nhiều hàng cùng lúc thì thưởng cao hơn)
    private const int LineBonus = 10;

    // Số lượt liên tiếp không xóa được hàng thì combo bị reset
    private const int MaxMovesWithoutClear = 3;

    // Thời gian hiệu ứng chữ khen (giây)
    private const float PopTime = 0.3f;
    private const float HoldTime = 0.7f;
    private const float FadeTime = 0.3f;

    // Chữ khen theo mức độ, mức càng cao càng "đã"
    private static readonly string[] Praises =
    {
        "Good!", "Great!", "Excellent!", "Amazing!", "Unbelievable!"
    };

    [SerializeField] private board board;
    [SerializeField] private Blocks blocks;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text highScoreLabel;
    [SerializeField] private Button replayButton;
    [SerializeField] private GameObject gameOverPanel; // không bắt buộc, để trống cũng được
    [SerializeField] private TMP_Text praiseLabel;     // chữ khen khi xóa hàng, không bắt buộc

    private int score;
    private int highScore;
    private int combo;             // số lần xóa hàng liên tiếp
    private int movesWithoutClear; // số lượt liên tiếp không xóa được hàng
    private Coroutine praiseRoutine;

    private void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(Replay);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (praiseLabel != null)
        {
            praiseLabel.gameObject.SetActive(false);
        }

        UpdateLabels();
    }

    private void OnDestroy()
    {
        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(Replay);
        }
    }

    // Board gọi hàm này mỗi lần đặt block thành công
    public void AddScore(int placedCells, int clearedLines)
    {
        var points = placedCells;

        if (clearedLines > 0)
        {
            combo++;
            movesWithoutClear = 0;
            points += clearedLines * clearedLines * LineBonus * combo; // combo càng cao điểm càng lớn
        }
        else if (++movesWithoutClear >= MaxMovesWithoutClear)
        {
            combo = 0;
        }

        score += points;

        if (clearedLines > 0)
        {
            ShowPraise(clearedLines);
        }

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt(HighScoreKey, highScore);
        }

        UpdateLabels();
    }

    // Blocks gọi hàm này khi không còn block nào đặt được
    public void GameOver()
    {
        PlayerPrefs.Save();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void Replay()
    {
        score = 0;
        combo = 0;
        movesWithoutClear = 0;
        HidePraise();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        board.ResetBoard();
        blocks.Restart();
        UpdateLabels();
    }

    private void ShowPraise(int clearedLines)
    {
        if (praiseLabel == null) return;

        // Xóa nhiều hàng một lúc hoặc combo cao thì chữ khen lên mức cao hơn
        var level = Mathf.Clamp(clearedLines - 1 + combo / 2, 0, Praises.Length - 1);
        var message = Praises[level];
        if (combo >= 2)
        {
            message += "\nCombo x" + combo;
        }

        if (praiseRoutine != null)
        {
            StopCoroutine(praiseRoutine);
        }
        praiseRoutine = StartCoroutine(PraiseRoutine(message));
    }

    private void HidePraise()
    {
        if (praiseRoutine != null)
        {
            StopCoroutine(praiseRoutine);
            praiseRoutine = null;
        }
        if (praiseLabel != null)
        {
            praiseLabel.gameObject.SetActive(false);
        }
    }

    private IEnumerator PraiseRoutine(string message)
    {
        var rect = praiseLabel.rectTransform;
        var color = praiseLabel.color;

        praiseLabel.text = message;
        color.a = 1f;
        praiseLabel.color = color;
        praiseLabel.gameObject.SetActive(true);

        // Phóng to có độ nảy
        for (var time = 0f; time < PopTime; time += Time.deltaTime)
        {
            rect.localScale = Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, EaseOutBack(time / PopTime));
            yield return null;
        }
        rect.localScale = Vector3.one;

        yield return new WaitForSeconds(HoldTime);

        // Mờ dần rồi ẩn
        for (var time = 0f; time < FadeTime; time += Time.deltaTime)
        {
            color.a = 1f - time / FadeTime;
            praiseLabel.color = color;
            yield return null;
        }

        praiseLabel.gameObject.SetActive(false);
        praiseRoutine = null;
    }

    private static float EaseOutBack(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
    }

    private void UpdateLabels()
    {
        if (scoreLabel != null) scoreLabel.text = score.ToString("D4");
        if (highScoreLabel != null) highScoreLabel.text = highScore.ToString("D4");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}