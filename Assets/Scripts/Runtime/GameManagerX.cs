using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerX : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private List<GameObject> targetPrefabs = new List<GameObject>();

    [SerializeField] private int roundDuration = 60;
    [SerializeField] private float baseSpawnRate = 1.5f;

    public bool isGameActive { get; private set; }

    private int score;
    private float spawnRate;
    private float remainingTime;
    private Coroutine spawnRoutine;

    private void Start()
    {
        score = 0;
        remainingTime = roundDuration;
        UpdateScore(0);
        UpdateTimeLabel();

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimeLabel();
            GameOver();
            return;
        }

        UpdateTimeLabel();
    }

    public void StartGame(int difficulty)
    {
        score = 0;
        remainingTime = roundDuration;
        spawnRate = baseSpawnRate / Mathf.Max(1, difficulty);
        isGameActive = true;

        UpdateScore(0);
        UpdateTimeLabel();

        if (titleScreen != null)
        {
            titleScreen.SetActive(false);
        }

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
        }

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnTarget());
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void GameOver()
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);

            if (!isGameActive || targetPrefabs.Count == 0)
            {
                continue;
            }

            int index = Random.Range(0, targetPrefabs.Count);
            Instantiate(targetPrefabs[index], targetPrefabs[index].transform.position, targetPrefabs[index].transform.rotation);
        }
    }

    private void UpdateTimeLabel()
    {
        if (timeText != null)
        {
            timeText.text = "Time: " + Mathf.CeilToInt(remainingTime);
        }
    }
}
