using UnityEngine;

public class MineTrap : MonoBehaviour
{
    public bool Attattayo = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Attattayo = true;
            Destroy(gameObject);
        }
    }
}
