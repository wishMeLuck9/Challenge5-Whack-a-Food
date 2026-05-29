using System.Collections;
using UnityEngine;

public class TargetX : MonoBehaviour
{
    [SerializeField] private int pointValue = 5;
    [SerializeField] private bool badTarget;
    [SerializeField] private GameObject explosionFx;
    [SerializeField] private float timeOnScreen = 1.2f;

    private GameManagerX gameManagerX;
    private bool resolved;

    private void Start()
    {
        gameManagerX = FindFirstObjectByType<GameManagerX>();
        StartCoroutine(RemoveObjectRoutine());
    }

    private void OnMouseDown()
    {
        if (resolved || gameManagerX == null || !gameManagerX.isGameActive)
        {
            return;
        }

        resolved = true;
        Explode();

        if (badTarget)
        {
            gameManagerX.GameOver();
        }
        else
        {
            gameManagerX.UpdateScore(pointValue);
        }

        Destroy(gameObject);
    }

    private IEnumerator RemoveObjectRoutine()
    {
        yield return new WaitForSeconds(timeOnScreen);

        if (resolved)
        {
            yield break;
        }

        resolved = true;
        if (gameManagerX != null && gameManagerX.isGameActive && !badTarget)
        {
            gameManagerX.GameOver();
        }

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (explosionFx != null)
        {
            Instantiate(explosionFx, transform.position, explosionFx.transform.rotation);
        }
    }
}
