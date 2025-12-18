using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Basic Attack Settings")]
    public float dmgValue = 4;              // 近接攻撃の基本ダメージ
    public GameObject throwableObject;      // 射撃用の弾プレハブ
    public Transform attackCheck;           // 攻撃判定の中心点となるTransform

    [Header("Input Settings")]
    public string shotButton = "Shot";      // 射撃ボタンの入力名（Input Managerに対応）

    [Header("Counter Settings")]
    public float counterDamageMult = 3.0f;  // カウンター時のダメージ倍率
    public float counterRangeMult = 2.0f;   // カウンター時の攻撃範囲倍率
    private bool isCountering = false;      // 現在カウンター攻撃を実行中かどうかのフラグ
    private float defaultRange = 0.9f;      // 通常時の基本攻撃範囲半径

    [Header("References")]
    public Animator animator;
    public GameObject cam;                  // ヒット時のカメラシェイク用参照

    // 内部ステータス
    public bool canAttack = true;           // 攻撃可能状態か
    public bool isTimeToCheck = false;      // 判定タイミング管理（アニメーション連携用）

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

    bool CheckDefenseStatus()
    {
        // カウンター受付中の場合、カウンター攻撃を実行
        if (playerDefense != null && playerDefense.canCounter)
        {
            PerformCounterAttack();
            return false; // 通常の攻撃処理はスキップ
        }
        // ガード中やパリィ失敗時の硬直中は攻撃不可
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

        isCountering = true; // ダメージ計算時に強化倍率を適用するためのフラグをON

        playerDefense.ResetAfterCounter(); // 防御・カウンター待機状態を解除
        animator.SetBool("IsAttacking", true);
        StartCoroutine(AttackCooldown());
    }


    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    void MakeShot()
    {
        float facingDir = transform.localScale.x;
        float xOffset = 0.5f;
        float xSpeed = 10.0f;

        // 左を向いている場合は、発射位置と速度ベクトルを反転
        if (facingDir < 0)
        {
            xOffset = -xOffset;
            xSpeed = -xSpeed;
        }

        // 弾の生成
        GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(xOffset, -0.2f, 0), Quaternion.identity) as GameObject;

        // 弾に進行方向を設定
        ThrowableWeapon weapon = throwableProj.GetComponent<ThrowableWeapon>();
        if (weapon != null)
        {
            weapon.direction = new Vector2(Mathf.Sign(xSpeed), 0f);
        }
    }

    public void DoDashDamage()
    {
        float finalDamage = Mathf.Abs(dmgValue);
        float finalRange = defaultRange;

        // カウンター実行中は設定された倍率で強化
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

        // 攻撃範囲内の敵を検出
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, finalRange);

        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            float time = 0.2f;
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                // 敵とプレイヤーの位置関係からノックバック方向（ダメージの符号）を決定
                float appliedDamage = finalDamage;
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    appliedDamage = -appliedDamage;
                }

                // ダメージ適用
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", appliedDamage);

                // ヒット時のカメラシェイク実行
                if (cam != null) cam.GetComponent<CameraFollow>().ShakeCamera(time);
            }
        }
    }
}