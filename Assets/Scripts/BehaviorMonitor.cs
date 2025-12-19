using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class BehaviorMonitor : MonoBehaviour
{
    public enum PlayStyle
    {
        Aggressive, // 空中主体 (Air)
        Smart,      // 距離を取る (Far)
        Offensive   // 近接主体 (Close)
    }

    [Header("Thresholds")]
    public float closeDistance = 3.0f; // これより近いと「攻撃的」
    public float smartDistance = 6.0f; // これより遠いと「スマート」
    public float checkRadius = 15.0f;  // 敵を認識する範囲

    [Header("Time Counters (Read Only)")]
    public float airTime = 0f;
    public float farTime = 0f;
    public float closeTime = 0f;

    [Header("Settings")]
    public LayerMask enemyLayer;

    private CharacterController2D cc2d;

    void Awake()
    {
        cc2d = GetComponent<CharacterController2D>();
    }

    void Update()
    {
        // 滞空時間の計測
        if (cc2d != null && !cc2d.IsGrounded)
        {
            airTime += Time.deltaTime;
        }

        // 周囲の敵をチェック
        Collider2D closestEnemy = GetClosestEnemy();

        if (closestEnemy != null)
        {
            float distance = Vector2.Distance(transform.position, closestEnemy.transform.position);

            //近距離の計測
            if (distance <= closeDistance)
            {
                closeTime += Time.deltaTime;
            }
            //遠距離維持の計測
            else if (distance >= smartDistance)
            {
                farTime += Time.deltaTime;
            }
        }

        //GameManager.instance.DispStr("Dynamic: " + airTime + " Aggration: " + closeTime + " Solid; " + farTime);
    }

    // 最も近い敵を探す関数
    Collider2D GetClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, checkRadius, enemyLayer);

        Collider2D closest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    // 結果を取得する関数
    public PlayStyle GetResult()
    {


        // 3つの時間を比較して最大値を返す
        float maxTime = Mathf.Max(airTime, farTime, closeTime);

        // 最大値と一致するものを返す
        if (Mathf.Approximately(maxTime, airTime))
        {   
            ResetActionTimes();
            return PlayStyle.Aggressive;
        }

        if (Mathf.Approximately(maxTime, farTime))
        {
            ResetActionTimes();
            return PlayStyle.Smart;
        }

        // 残るはOffensive
        ResetActionTimes();
        return PlayStyle.Offensive;

    }

    private void ResetActionTimes()
    {
        airTime = 0f;
        farTime = 0f;
        closeTime = 0f;
    }

    // 範囲の可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius); // 敵検知範囲
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeDistance); // 近距離ライン
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, smartDistance); // 遠距離ライン
    }

}

/* 外部スクリプトからの呼び出し例
var monitor = playerObject.GetComponent<BehaviorMonitor>();
var style = monitor.GetResult();

switch (style)
{
case BehaviorMonitor.PlayStyle.Aggressive:
    Debug.Log("アグレッシブなプレイヤー！");
    break;
case BehaviorMonitor.PlayStyle.Smart:
    Debug.Log("堅実なプレイヤー！");
    break;
case BehaviorMonitor.PlayStyle.Offensive:
    Debug.Log("攻撃的なプレイヤー！");
    break;
}*/