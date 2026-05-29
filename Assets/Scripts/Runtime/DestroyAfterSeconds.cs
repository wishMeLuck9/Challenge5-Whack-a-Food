using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField] private float lifetime = 1.25f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
