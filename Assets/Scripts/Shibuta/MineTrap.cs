using UnityEngine;
using System.Collections; // Coroutine‚ðŽg‚¤‚½‚ß‚É•K—v

public class MineTrap : MonoBehaviour
{
    public bool Attattayo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Attattayo = true;
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {

        yield return null;

        Destroy(gameObject);
    }
}
