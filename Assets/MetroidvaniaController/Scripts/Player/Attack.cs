using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Settings")]
    public float dmgValue = 4;
    public GameObject throwableObject; 
    public Transform attackCheck;

    [Header("Input Settings")]
    public string shotButton = "Shot";

    public Animator animator;
    public bool canAttack = true;
    public bool isTimeToCheck = false;

    public GameObject cam;

    private Rigidbody2D m_Rigidbody2D;
    private PlayerDefense playerDefense;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        playerDefense = GetComponent<PlayerDefense>();
    }

    void Start()
    {

    }

    void Update()
    {
        // 通常攻撃（近接）
        if (Input.GetButtonDown("Attack") && canAttack)
        {
            if (CheckDefenseStatus())
            {
                PerformAttack();
            }
        }

        if (Input.GetButtonDown(shotButton) && canAttack)
        {
            if (CheckDefenseStatus())
            {
                PerformRangeAttack();
            }
        }
    }

    // 防御中でないかチェック
    bool CheckDefenseStatus()
    {
        // カウンター攻撃判定
        if (playerDefense != null && playerDefense.canCounter)
        {
            PerformCounterAttack();
            return false;
        }
        // ガード中・硬直中は攻撃不可
        else if (playerDefense != null && (playerDefense.isGuarding || playerDefense.inRecovery))
        {
            return false;
        }
        return true;
    }

    void PerformAttack()
    {
        canAttack = false;
        animator.SetBool("IsAttacking", true);
        StartCoroutine(AttackCooldown());
    }

    // 射撃実行処理
    void PerformRangeAttack()
    {
        canAttack = false;

        MakeShot();

        StartCoroutine(AttackCooldown());
    }

    void PerformCounterAttack()
    {
        Debug.Log("Counter Attack!!");
        playerDefense.ResetAfterCounter();
        animator.SetBool("IsAttacking", true);
        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    // 弾生成・発射処理
    void MakeShot()
    {
        // プレイヤーの向きを取得
        float facingDir = transform.localScale.x;

        float xOffset = 0.5f;
        float xSpeed = 10.0f; // 弾速

        // 左向きなら反転
        if (facingDir < 0)
        {
            xOffset = -xOffset;
            xSpeed = -xSpeed;
        }

        // 弾を生成
        GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(xOffset, -0.2f, 0), Quaternion.identity) as GameObject;

        // 方向をセット
        ThrowableWeapon weapon = throwableProj.GetComponent<ThrowableWeapon>();
        if (weapon != null)
        {
            weapon.direction = new Vector2(xSpeed, 0f);

            weapon.direction = new Vector2(Mathf.Sign(xSpeed), 0f);
        }
    }

    public void DoDashDamage()
    {
        dmgValue = Mathf.Abs(dmgValue);
        Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        for (int i = 0; i < collidersEnemies.Length; i++)
        {
            if (collidersEnemies[i].gameObject.tag == "Enemy")
            {
                if (collidersEnemies[i].transform.position.x - transform.position.x < 0)
                {
                    dmgValue = -dmgValue;
                }
                collidersEnemies[i].gameObject.SendMessage("ApplyDamage", dmgValue);
                cam.GetComponent<CameraFollow>().ShakeCamera();
            }
        }
    }
}