using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// クラス名を変更しました。ファイル名も必ず SwarmBat.cs にしてください。
public class SwarmBat : MonoBehaviour
{
    [Header("ステータス")]
    public float life = 10;
    public float moveSpeed = 3.0f;

    // 個体差用
    private float speedNoise;
    private Vector2 offsetNoise;

    // 内部フラグ
    bool isInvincible = false;
    bool isHitted = false;
    bool hasReportedDeath = false;

    Animator animator;
    private Coroutine hitCoroutine;

    // 参照系
    private BatSwarmManager myManager;
    private Transform targetPlayer;

    // 動き用変数
    public bool facingLeft = true;
    public Vector2 speed = Vector2.zero;

    // ステート
    enum State
    {
        WAIT, // 生成直後
        MOVE, // 追跡中
        DEAD, // 死亡中
    };
    State state = State.WAIT;
    float timer;
    int step;

    // ------------------------------------------------------
    // 初期化（マネージャーから呼ばれる）
    public void Setup(BatSwarmManager manager, Transform player)
    {
        myManager = manager;
        targetPlayer = player;

        // 個体差をつける（全員が重ならないように）
        speedNoise = Random.Range(0.8f, 1.2f);
        offsetNoise = Random.insideUnitCircle * 0.5f;

        // 移動開始
        state = State.MOVE;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnDestroy()
    {
        if (hitCoroutine != null) StopCoroutine(hitCoroutine);
    }

    void Update()
    {
        // 死亡判定
        if (life <= 0 && state != State.DEAD)
        {
            state = State.DEAD;
            step = 0;
        }

        switch (state)
        {
            case State.WAIT:
                // Setupされるまで待機
                speed = Vector2.zero;
                break;

            case State.MOVE:
                Move();
                break;

            case State.DEAD:
                DeadProcess();
                break;
        }
        ;

        // ヒットストップ中でなければ移動反映
        if (!isHitted)
        {
            ApplyMovement();
        }
    }

    // プレイヤーに向かう移動ロジック
    void Move()
    {
        if (targetPlayer == null) return;

        // ターゲット座標 + ノイズ
        Vector2 targetPos = (Vector2)targetPlayer.position + offsetNoise;
        Vector2 currentPos = transform.position;

        // 方向と速度
        Vector2 direction = (targetPos - currentPos).normalized;
        float currentSpeed = moveSpeed * speedNoise;

        // ふわふわ感を出す
        float waveY = Mathf.Sin(Time.time * 5.0f + this.GetInstanceID()) * 1.0f;

        speed = direction * currentSpeed;
        speed.y += waveY;

        // 向き判定
        if (speed.x > 0) facingLeft = false;
        else if (speed.x < 0) facingLeft = true;
    }

    // 移動の反映
    void ApplyMovement()
    {
        // 絵の反転
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = facingLeft;

        // 座標更新
        Vector3 pos = transform.position;
        pos.x += speed.x * Time.deltaTime;
        pos.y += speed.y * Time.deltaTime;

        // 簡易地形判定（地面より下に行かない）
        if (pos.y < 0.8f && state != State.DEAD)
        {
            pos.y = 0.8f;
        }
        transform.position = pos;
    }

    // 死亡時の演出処理
    void DeadProcess()
    {
        // 最初の1回だけマネージャーに報告
        if (!hasReportedDeath)
        {
            if (myManager != null) myManager.ReportDeath();
            hasReportedDeath = true;

            if (animator != null) animator.SetBool("IsDead", true);
            speed = Vector2.zero;
            timer = 1.0f; // 演出時間
        }

        // 段階的な演出
        switch (step)
        {
            case 0: // 停止中
                timer -= Time.deltaTime;
                // 少しずつ落下させる
                speed.y -= 5f * Time.deltaTime;

                if (timer < 0)
                {
                    step++;
                }
                break;
            case 1: // 削除
                Destroy(gameObject);
                break;
        }
    }

    // ダメージ処理（外部から呼ばれる想定）
    public void ApplyDamage(float damage)
    {
        if (!isInvincible && life > 0)
        {
            life -= Mathf.Abs(damage);
            if (hitCoroutine != null) StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitTime());
        }
    }

    IEnumerator HitTime()
    {
        isHitted = true;
        isInvincible = true;
        yield return new WaitForSeconds(0.2f);
        isHitted = false;
        isInvincible = false;
        hitCoroutine = null;
    }

    // 衝突判定
    void OnTriggerEnter2D(Collider2D collider)
    {
        // ここにプレイヤーへの攻撃処理などを記述
        // if (collider.gameObject.CompareTag("Player")) { ... }
    }
}