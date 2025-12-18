using Unity.VisualScripting;
using UnityEngine;

public class ItemManager : MonoBehaviour 
{
    public string targetObjectTag = "Player";
    [SerializeField] private float BuffValue = 1;

    public enum Item
    {
        moveSpeed = 0,
        attackSpeed = 1,
        protectShield = 2
    };

    public Item whatItem = Item.moveSpeed;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(targetObjectTag))
        {
            OnBuffAssignmentToPlayer(whatItem);
            Destroy(this.gameObject);
        }
    }

    private void OnBuffAssignmentToPlayer(Item thisItem)
    {
        switch (thisItem)
        {
            case Item.moveSpeed:
                UnityEngine.Debug.Log(thisItem + "を取得！スピード増加！");
                SpeedUP();
                break;
            case Item.attackSpeed:
                AttackSpeedUP();
                UnityEngine.Debug.Log(thisItem + "を取得！攻撃速度増加！");
                break;

            case Item.protectShield:
                AttachProtectShield();
                UnityEngine.Debug.Log(thisItem + "を取得！シールド付与!");
                break;
        }
    }

    private void SpeedUP()
    {
        Debug.Log("速度増加: " + BuffValue );
    }

    private void AttackSpeedUP()
    {
        Debug.Log("攻撃速度増加: " + BuffValue);
    }

    private void AttachProtectShield()
    {
        Debug.Log("防御膜付与: " + BuffValue);
    }
}
