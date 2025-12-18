using Unity.VisualScripting;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public string targetObjectTag = "Player";

    [Header("Buff Settings")]
    [SerializeField] private float BuffValue = 5f; // ★推奨: 1だと変化が小さいので5くらいに増やす
    [SerializeField] private float duration = 3.0f; // ★追加: 効果時間（秒）

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
            // ★変更: 衝突した相手（プレイヤー）のGameObjectを渡す
            OnBuffAssignmentToPlayer(whatItem, collision.gameObject);
            Destroy(this.gameObject);
        }
    }

    // ★変更: 引数に player を追加
    private void OnBuffAssignmentToPlayer(Item thisItem, GameObject player)
    {
        switch (thisItem)
        {
            case Item.moveSpeed:
                UnityEngine.Debug.Log(thisItem + "を取得！スピード増加！");
                // ★変更: playerを渡す
                SpeedUP(player);
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

    // ★変更: 実際にプレイヤーのスクリプトを操作する
    private void SpeedUP(GameObject player)
    {
        // プレイヤーから CharacterController2D を取得
        CharacterController2D controller = player.GetComponent<CharacterController2D>();

        if (controller != null)
        {
            Debug.Log("速度増加量: " + BuffValue + " / 時間: " + duration + "秒");
            // CharacterController2Dに追加したメソッドを実行
            controller.BoostSpeed(BuffValue, duration);
        }
    }

    private void AttackSpeedUP()
    {
        Debug.Log("攻撃速度増加: " + BuffValue);
        // 必要に応じて同様の実装を行ってください
    }

    private void AttachProtectShield()
    {
        Debug.Log("防御膜付与: " + BuffValue);
        // 必要に応じて同様の実装を行ってください
    }
}