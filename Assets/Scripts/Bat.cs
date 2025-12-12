using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
    [SerializeField] private GameObject alertPrefab;

    float orbitAngle = 0f;
    bool isOrbiting = false; // 回転初期化用フラグ
    float orbitSpeed = 6.0f; // 回転速度（大きくすると速く回る）

    public float life = 10;
    public int set = 0;

    public static BlackBoard squadBoard;

    public bool moveOK = false;

    // トークン関連
    private TokenSource tokenSource;
    private Token myAttackToken = null;

    bool isInvincible = false;
    bool isHitted = false;

    Animator animator;
    public EnemyComp tool;
    private Coroutine hitCoroutine;

    public bool facingLeft = true;
    public Vector2 speed = Vector2.zero;

    float randomOffset;

    enum State
    {
        SEARCH,
        CHASE,
        ATTACK_RESPONSE,
        DEAD,
    };
    State state = State.SEARCH;

    // 指令処理用
    int currentCommand = 0;
    float commandTimer = 0f;

    float noiseTimer = 0f;
    Vector2 noiseOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        tool = new EnemyComp(this.gameObject);
        randomOffset = Random.Range(0f, 100f);

        // ■■■ BoardMからボードを取得 ■■■
        if (BoardM.instance != null)
        {
            squadBoard = BoardM.instance.sharedBoard;
        }
        else
        {
            // エラーが出ないように仮作成（本来はManager必須）
            Debug.LogError("BoardMが見つかりません");
            squadBoard = new BlackBoard();
        }
    }

    void Update()
    {
        if (state == State.DEAD) return;
        if (life <= 0)
        {
            state = State.DEAD;
            animator.SetTrigger("Dead");
            Destroy(gameObject, 0.5f);
            return;
        }

        // ■■■ ブラックボードを用いた連携処理 ■■■
        if (squadBoard != null)
        {
            int command;
            if (squadBoard.GetValue(BlackBoardKey.BatCommand, out command))
            {
                // 新しい指令が来た時
                if (command != 0 && currentCommand != command)
                {
                    SpawnAlertObject();
                    currentCommand = command;
                    state = State.ATTACK_RESPONSE;

                    if (command == 1) // 突進
                    {
                        commandTimer = 1.5f;
                    }
                    else if (command == 2) // 回転
                    {
                        // 3回転するのに必要な時間を計算
                        float cycleCount = 6.0f;
                        commandTimer = (cycleCount * 2.0f * Mathf.PI) / orbitSpeed;
                    }
                }
            }

            // 発見・見失う処理
            float dist = tool.DistanceToPlayer();
            if (dist < 8.0f)
            {
                if (state == State.SEARCH)
                {
                    state = State.CHASE;
                    squadBoard.SetValue(BlackBoardKey.SquadState, 1);
                }
            }
            else if (dist > 15.0f && state == State.CHASE)
            {
                state = State.SEARCH;
                squadBoard.SetValue(BlackBoardKey.SquadState, 0);
            }
        }

        // ■■■ ステートごとの行動実行 ■■■
        switch (state)
        {
            case State.SEARCH:
                SearchAction();
                Movement(); // 通常移動を実行
                break;
            case State.CHASE:
                ChaseAction();
                Movement(); // 通常移動を実行
                break;
            case State.ATTACK_RESPONSE:
                ResponseAction();
                // ★重要：ここでは Movement() を呼ばない！
                // 突進のときは ResponseAction 内で個別に呼び、回転のときは呼ばないことで制御する
                break;
        }

        // ★重要：ここに書いてあった Movement() は削除しました！
    }

    // --- 各ステートの処理 ---


    void SpawnAlertObject()
    {
        if (alertPrefab != null)
        {
            // Batの頭上（Y軸に+1.5fくらい）の位置を計算
            Vector3 spawnPos = transform.position + new Vector3(0f, 1.5f, 0f);

            // 1. まず親を指定せずに生成します
            // （これでプレハブの設定通りのサイズで生成されます）
            GameObject obj = Instantiate(alertPrefab, spawnPos, Quaternion.identity);

            // 2. その後、ワールドでの見た目（サイズ・位置）を維持したまま親を設定します
            // 第二引数の true が「今のサイズをキープする」という設定です
            obj.transform.SetParent(transform, true);
        }
    }
    void SearchAction()
    {
        noiseTimer += Time.deltaTime;
        float x = Mathf.Sin(noiseTimer + randomOffset) * 0.5f;
        float y = Mathf.Cos(noiseTimer * 0.8f + randomOffset) * 0.5f;
        speed = Vector2.Lerp(speed, new Vector2(x, y), Time.deltaTime * 2.0f);
    }

    void ChaseAction()
    {
        Vector2 targetPos = tool.PlayerPosition();
        Vector2 myPos = transform.position;
        targetPos.y += 1.5f;
        Vector2 dir = (targetPos - myPos).normalized;
        speed = Vector2.Lerp(speed, dir * 3.0f, Time.deltaTime * 5.0f);
    }

    void ResponseAction()
    {
        commandTimer -= Time.deltaTime;

        int checkCommand;
        if (squadBoard == null || !squadBoard.GetValue(BlackBoardKey.BatCommand, out checkCommand))
        {
            checkCommand = 0;
        }

        // ■■■ 修正箇所：checkCommand == 0 を削除しました ■■■
        // これにより、Soldierが指令を解除しても、回転が終わる(timerが0になる)まで止まらなくなります
        if (commandTimer <= 0)
        {
            state = State.CHASE;
            currentCommand = 0;
            isOrbiting = false;
            return;
        }

        if (currentCommand == 1) // 突進
        {
            Vector2 targetPos = tool.PlayerPosition();
            Vector2 myPos = transform.position;
            Vector2 direction = (targetPos - myPos).normalized;
            speed = Vector2.Lerp(speed, direction * 15.0f, Time.deltaTime * 10.0f);

            // 突進は物理移動が必要なので、ここで呼ぶ
            Movement();
        }
        else if (currentCommand == 2) // 回転
        {
            // 回転は Movement を呼ばず、直接座標を操作する
            Vector2 centerPos = tool.PlayerPosition() + new Vector2(0f, 1.2f);

            if (!isOrbiting)
            {
                Vector2 offset = (Vector2)transform.position - centerPos;
                orbitAngle = Mathf.Atan2(offset.y, offset.x);
                isOrbiting = true;
            }

            orbitAngle += orbitSpeed * Time.deltaTime;

            float radius = 2.5f;
            Vector2 nextPos;
            nextPos.x = centerPos.x + Mathf.Cos(orbitAngle) * radius;
            nextPos.y = centerPos.y + Mathf.Sin(orbitAngle) * radius;

            transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * 10f);

            // 向き制御（プレイヤーの方を向く）
            if (transform.position.x > centerPos.x) GetComponent<SpriteRenderer>().flipX = true;
            else GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    void Movement()
    {
        // 連携攻撃中以外だけ自動反転させる
        if (state != State.ATTACK_RESPONSE)
        {
            if (Mathf.Abs(speed.x) > 0.1f)
            {
                facingLeft = speed.x < 0;
            }
        }

        GetComponent<SpriteRenderer>().flipX = facingLeft;

        Vector3 pos = transform.position;
        pos.x += speed.x * Time.deltaTime;
        pos.y += speed.y * Time.deltaTime;

        if (pos.y < 0.8f) pos.y = 0.8f;

        transform.position = pos;
    }

    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            life -= damage;
            speed = new Vector2(direction * 5.0f, 2.0f);

            if (hitCoroutine != null) StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitTime());
        }
    }

    IEnumerator HitTime()
    {
        isHitted = true;
        isInvincible = true;
        animator.SetTrigger("Hit");
        yield return new WaitForSeconds(0.5f);
        isHitted = false;
        isInvincible = false;
    }

    void OnDestroy()
    {
        if (myAttackToken != null)
        {
            myAttackToken.Return();
        }
    }
}