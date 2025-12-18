using UnityEngine;
using UnityEngine.Events;

public class BatSwarmManager : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("SwarmBatスクリプトがついたPrefabをセットしてください")]
    public GameObject batPrefab;

    [Tooltip("群れの数")]
    public int swarmCount = 10;

    [Tooltip("出現範囲")]
    public float spawnRadius = 2.0f;

    [Header("クリア報酬（バフ）")]
    [Tooltip("全滅時にプレイヤーを強化するか")]
    public bool applyBuffOnClear = true;
    [Tooltip("効果時間（秒）")]
    public float rewardDuration = 10.0f;
    [Tooltip("攻撃速度倍率（2.0で2倍速）")]
    public float rewardSpeedMult = 2.0f;
    [Tooltip("攻撃力倍率（1.5で1.5倍）")]
    public float rewardDamageMult = 1.5f;

    [Header("イベント")]
    [Tooltip("全員倒した時に呼ばれる（UI表示などに使ってください）")]
    public UnityEvent OnSwarmDefeated;

    private int deadCount = 0;
    private int spawnedCount = 0;
    private Transform playerTransform;

    public void CallSwarm()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Playerタグのついたオブジェクトが見つかりません。");
            return;
        }

        if (batPrefab == null)
        {
            Debug.LogError("Bat Prefabが設定されていません。");
            return;
        }

        deadCount = 0;
        spawnedCount = 0;

        for (int i = 0; i < swarmCount; i++)
        {
            SpawnBat();
        }
    }

    void SpawnBat()
    {
        Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        GameObject obj = Instantiate(batPrefab, randomPos, Quaternion.identity);

        SwarmBat batScript = obj.GetComponent<SwarmBat>();
        if (batScript != null)
        {
            batScript.Setup(this, playerTransform);
            spawnedCount++;
        }
    }

    public void ReportDeath()
    {
        deadCount++;

        // 全員倒したかチェック
        if (deadCount >= spawnedCount)
        {
            AllBatsDefeated();
        }
    }

    void AllBatsDefeated()
    {
        Debug.Log("群れ全滅！報酬を付与します。");

        // 1. バフ報酬の適用
        if (applyBuffOnClear && playerTransform != null)
        {
            // プレイヤーからAttackコンポーネントを探す
            Attack playerAttack = playerTransform.GetComponent<Attack>();

            if (playerAttack != null)
            {
                // 両方のバフを付与
                playerAttack.ApplySpeedBuff(rewardSpeedMult, rewardDuration);
                playerAttack.ApplyDamageBuff(rewardDamageMult, rewardDuration);
            }
            else
            {
                Debug.LogWarning("プレイヤーにAttackスクリプトがついていないため、バフを付与できませんでした。");
            }
        }

        // 2. その他のイベント実行（UI表示やSEなど）
        OnSwarmDefeated?.Invoke();
    }
}