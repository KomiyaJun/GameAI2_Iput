using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Basic Attack Settings")]
    public float dmgValue = 4;              // 近接攻撃の基本ダメージ
    public float baseAttackCooldown = 0.25f; // 【変更】攻撃の基本クールダウン時間（変数化）
    public GameObject throwableObject;      // 射撃用の弾プレハブ
    public Transform attackCheck;           // 攻撃判定の中心点となるTransform

    [Header("Input Settings")]
    public string shotButton = "Shot";      // 射撃ボタンの入力名

    [Header("Counter Settings")]
    public float counterDamageMult = 3.0f;  // カウンター時のダメージ倍率
    public float counterRangeMult = 2.0f;   // カウンター時の攻撃範囲倍率
    private bool isCountering = false;      // 現在カウンター攻撃を実行中かどうかのフラグ
    private float defaultRange = 0.9f;      // 通常時の基本攻撃範囲半径

    [Header("Buff Status")] // 【追加】バフの状態確認用
    [SerializeField] private float currentDamageMultiplier = 1.0f; // 現在の攻撃力倍率（1.0が通常）
    [SerializeField] private float currentSpeedMultiplier = 1.0f;  // 現在の攻撃速度倍率（1.0が通常）
    private Coroutine speedBuffCoroutine;
    private Coroutine damageBuffCoroutine;

    [Header("References")]
    public Animator animator;
    public GameObject cam;                  // ヒット時のカメラシェイク用参照

    // 内部ステータス
    public bool canAttack = true;           // 攻撃可能状態か
    public bool isTimeToCheck = false;      // 判定タイミング管理

    private Rigidbody2D m_Rigidbody2D;
    private PlayerDefense playerDefense;    // 防御・パリィ状態確認用

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playerDefense = GetComponent<PlayerDefense>();
    }

    void Update()
    {
        // 近接攻撃入力
        if (Input.GetButtonDown("Attack") && canAttack)
        {
            if (CheckDefenseStatus())
            {
                PerformAttack();
            }
        }

        // 射撃入力
        if (Input.GetButtonDown(shotButton) && canAttack)
        {
            if (CheckDefenseStatus())
            {
                PerformRangeAttack();
            }
        }
    }

    // ---------------------------------------------------------
    // 【追加】バフ・デバフ用メソッド
    // ---------------------------------------------------------

    /// <summary>
    /// 一定時間、攻撃速度を上昇させる
    /// </summary>
    /// <param name="multiplier">速度倍率（2.0なら2倍速＝クールダウン半減）</param>
    /// <param name="duration">効果時間（秒）</param>
    public void ApplySpeedBuff(float multiplier, float duration)
    {
        // 既にバフがかかっている場合は一度リセットして上書き
        if (speedBuffCoroutine != null) StopCoroutine(speedBuffCoroutine);
        speedBuffCoroutine = StartCoroutine(SpeedBuffRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        currentSpeedMultiplier = multiplier;

        // Animatorの速度パラメータも変更する（もしAnimatorにSpeedパラメータがあれば）
        // animator.SetFloat("AttackSpeed", multiplier); 

        // または、単純にAnimator全体の速度を変える場合（歩きなども速くなるので注意）
        animator.speed = multiplier;

        Debug.Log($"Attack Speed Up! x{multiplier}");

        yield return new WaitForSeconds(duration);

        currentSpeedMultiplier = 1.0f;
        animator.speed = 1.0f; // 元に戻す
        Debug.Log("Attack Speed Normal.");
        speedBuffCoroutine = null;
    }
    /// <summary>
    /// 一定時間、攻撃力を上昇させる
    /// </summary>
    /// <param name="multiplier">攻撃力倍率（1.5なら1.5倍）</param>
    /// <param name="duration">効果時間（秒）</param>
    public void ApplyDamageBuff(float multiplier, float duration)
    {
        if (damageBuffCoroutine != null) StopCoroutine(damageBuffCoroutine);
        damageBuffCoroutine = StartCoroutine(DamageBuffRoutine(multiplier, duration));
    }

    private IEnumerator DamageBuffRoutine(float multiplier, float duration)
    {
        currentDamageMultiplier = multiplier;
        Debug.Log($"Attack Damage Up! x{multiplier}");

        yield return new WaitForSeconds(duration);

        currentDamageMultiplier = 1.0f; // 元に戻す
        Debug.Log("Attack Damage Normal.");
        damageBuffCoroutine = null;
    }
    // ---------------------------------------------------------


    bool CheckDefenseStatus()
    {
        if (playerDefense != null && playerDefense.canCounter)
        {
            PerformCounterAttack();
            return false;
        }
        else if (playerDefense != null && (playerDefense.isGuarding || playerDefense.inRecovery))
        {
            return false;
        }
        return true;
    }

    void PerformAttack()
    {
        isCountering = false;
        canAttack = false;
        animator.SetBool("IsAttacking", true);
        StartCoroutine(AttackCooldown());
    }

    void PerformRangeAttack()
    {
        isCountering = false;
        canAttack = false;
        MakeShot();
        StartCoroutine(AttackCooldown());
    }

    void PerformCounterAttack()
    {
        Debug.Log("Counter Attack Execute");
        isCountering = true;
        playerDefense.ResetAfterCounter();
        animator.SetBool("IsAttacking", true);
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        // 【変更】基本クールダウンを速度倍率で割る（倍率が高いほど待ち時間が減る）
        float waitTime = baseAttackCooldown / currentSpeedMultiplier;
        yield return new WaitForSeconds(waitTime);
        canAttack = true;
    }

    void MakeShot()
    {
        float facingDir = transform.localScale.x;
        float xOffset = 0.5f;
        float xSpeed = 10.0f;

        if (facingDir < 0)
        {
            xOffset = -xOffset;
            xSpeed = -xSpeed;
        }

        GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(xOffset, -0.2f, 0), Quaternion.identity) as GameObject;

        ThrowableWeapon weapon = throwableProj.GetComponent<ThrowableWeapon>();
        if (weapon != null)
        {
            weapon.direction = new Vector2(Mathf.Sign(xSpeed), 0f);

            // （オプション）飛び道具にも攻撃バフを乗せたい場合はここで倍率を渡す処理が必要
            // weapon.damage *= currentDamageMultiplier; など
        }
    }

    public void DoDashDamage()
    {
        float finalDamage = Mathf.Abs(dmgValue);
        float finalRange = defaultRange;

        // 【追加】ここでバフ倍率を計算に含める
        finalDamage *= currentDamageMultiplier;

        if (isCountering)
        {
            finalDamage *= counterDamageMult;
            finalRange *= counterRangeMult;
            if (cam != null) cam.GetComponent<CameraFollow>().shakeAmount = 1.0f;
        }
        else
        {
            if (cam != null) cam.GetComponent<CameraFollow>().shakeAmount = 0.1f;
        }

        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, finalRange);

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            float time = 0.2f;
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                float appliedDamage = finalDamage;
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    appliedDamage = -appliedDamage;
                }

                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", appliedDamage);

                if (cam != null) cam.GetComponent<CameraFollow>().ShakeCamera(time);
            }
        }
    }
}