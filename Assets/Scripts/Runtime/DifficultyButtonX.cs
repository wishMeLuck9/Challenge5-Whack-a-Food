using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DifficultyButtonX : MonoBehaviour
{
    [SerializeField] private int difficulty = 1;

    private Button button;
    private GameManagerX gameManagerX;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        gameManagerX = FindFirstObjectByType<GameManagerX>();
        button.onClick.AddListener(SetDifficulty);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SetDifficulty);
        }
    }

    private void SetDifficulty()
    {
        if (gameManagerX != null)
        {
            gameManagerX.StartGame(difficulty);
        }
    }
}
