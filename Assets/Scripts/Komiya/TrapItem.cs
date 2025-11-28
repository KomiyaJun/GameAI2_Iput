using Unity.VisualScripting;
using UnityEngine;

public class TrapItem : MonoBehaviour
{
    [SerializeField] private Soldier soldier;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            soldier.OnTrapTouched();
            Destroy(gameObject);
            Debug.Log("トラップアイテムが取られました");
            Debug.Log("isTrapTouched" + soldier.isTrapTouched);
        }
    }
}
