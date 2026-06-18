using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TargetX : MonoBehaviour
{
    [SerializeField] private int pointValue = 5;
    [SerializeField] private bool badTarget;
    [SerializeField] private GameObject explosionFx;
    [SerializeField] private float timeOnScreen = 1.2f;
    [SerializeField] private float minSpeed = 12f;
    [SerializeField] private float maxSpeed = 16f;
    [SerializeField] private float maxTorque = 10f;
    [SerializeField] private float xRange = 4f;
    [SerializeField] private float ySpawnPos = -4.5f;

    private GameManagerX gameManagerX;
    private Rigidbody targetRb;
    private bool resolved;

    private void Start()
    {
        targetRb = GetComponent<Rigidbody>();
        gameManagerX = FindFirstObjectByType<GameManagerX>();

        transform.position = RandomSpawnPosition();
        targetRb.linearVelocity = Vector3.zero;
        targetRb.angularVelocity = Vector3.zero;
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
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

    private void OnTriggerEnter(Collider other)
    {
        if (resolved || !other.CompareTag("Sensor"))
        {
            return;
        }

        resolved = true;
        if (gameManagerX != null && gameManagerX.isGameActive && !badTarget)
        {
            gameManagerX.GameOver();
        }

        Destroy(gameObject);
    }

    private Vector3 RandomForce()
    {
        return Vector3.up * Random.Range(minSpeed, maxSpeed);
    }

    private float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }

    private Vector3 RandomSpawnPosition()
    {
        return new Vector3(Random.Range(-xRange, xRange), ySpawnPos, 0f);
    }

    private void Explode()
    {
        if (explosionFx != null)
        {
            Instantiate(explosionFx, transform.position, explosionFx.transform.rotation);
        }
    }
}
