using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
    public float life = 10;
    public int set = 0;

    // リーダーから渡されるブラックボード
    private BlackBoard squadBoard;

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

    // 個体差を作るためのランダム値
    float randomOffset;

    enum State
    {
        SEARCH, // 徘徊
        CHASE,  // 妨害
        ATTACK_RESPONSE, // 連携攻撃
        DEAD,
    };
    State state = State.SEARCH;

    float noiseTimer = 0f;
    Vector2 noiseOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        tool = new EnemyComp(this.gameObject);

        // 生成時に個体差を設定（動きのタイミングをずらす）
        randomOffset = Random.Range(0f, 100f);

        // 最初の目標地点もランダムに設定
        noiseOffset = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
    }

    //チーム全員で同じBlackBoardを共有するための設定
    public void SetSquadBoard(BlackBoard board)
    {
        this.squadBoard = board;
    }

    // トークンソースを受け取る
    public void SetTokenSource(TokenSource source)
    {
        this.tokenSource = source;
    }

    void Update()
    {
        if (life <= 0 && state != State.DEAD)
        {
            state = State.DEAD;
        }

        switch (state)
        {
            case State.SEARCH:
                SearchLogic();
                break;
            case State.CHASE:
                ChaseLogic();
                break;
            case State.ATTACK_RESPONSE:
                AttackResponseLogic();
                break;
            case State.DEAD:
                DeadAction();
                break;
        }
        ;

        // 攻撃ステート以外に遷移したらトークンを返す★★★★★★★★★★★★
        if (state != State.ATTACK_RESPONSE && myAttackToken != null)
        {
            myAttackToken.Return();
            myAttackToken = null;
        }

        if (!isHitted && state != State.DEAD)
        {
            Movement();
        }
    }

    // 1. 捜索
    void SearchLogic()
    {
        // 個体差を加味して上下左右に移動
        speed.x = Mathf.Cos((Time.time + randomOffset) * 1.0f) * 1.5f;
        speed.y = Mathf.Sin((Time.time + randomOffset) * 1.5f) * 1.5f;
        facingLeft = speed.x < 0;

        // 仲間と重ならないように反発力を加える
        speed += GetSeparationVector() * 1.0f;

        if (squadBoard == null) return;

        int squadState;
        squadBoard.GetValue(BlackBoardKey.SquadState, out squadState);

        // 自身で発見するか、チーム共有情報で追跡へ移行
        if (tool.DistanceToPlayer() < 5.0f && !IsPlayerHidden())
        {
            squadBoard.SetValue(BlackBoardKey.SquadState, 1);
            state = State.CHASE;
        }
        else if (squadState == 1)
        {
            state = State.CHASE;
        }
    }

    // 2. 追跡
    void ChaseLogic()
    {
        int squadState;
        squadBoard.GetValue(BlackBoardKey.SquadState, out squadState);
        // プレイヤーを見失うか、チームが捜索に戻ったら戻る
        if (IsPlayerHidden() || squadState == 0)
        {
            state = State.SEARCH;
            return;
        }

        // 攻撃指令が出ているか確認
        int command;
        squadBoard.GetValue(BlackBoardKey.BatCommand, out command);
        if (command != 0)
        {
            state = State.ATTACK_RESPONSE;
            return;
        }

        Vector2 targetPos = tool.PlayerPosition();
        noiseTimer -= Time.deltaTime;

        // 目標地点の更新タイミングを個体ごとにずらす
        if (noiseTimer <= 0)
        {
            // プレイヤーの周りをランダムに飛ぶ
            noiseOffset = new Vector2(Random.Range(-3f, 3f), Random.Range(0.5f, 3.5f));
            noiseTimer = 1.0f + Random.Range(0f, 0.5f);
        }

        Vector2 dest = targetPos + noiseOffset;
        Vector2 dir = (dest - (Vector2)transform.position).normalized;
        speed = dir * 3.0f;

        // 追跡中も仲間との距離を保つ
        speed += GetSeparationVector() * 2.0f;

        facingLeft = (targetPos.x < transform.position.x);
    }

    // 3. 連携攻撃
    void AttackResponseLogic()
    {
        int command;
        squadBoard.GetValue(BlackBoardKey.BatCommand, out command);

        // コマンド解除で追跡へ戻る
        if (command == 0)
        {
            state = State.CHASE;
            return;
        }

        // トークンを持っていなければ取得を試みる★★★★★★★★★★★★★
        if (myAttackToken == null && tokenSource != null)
        {
            myAttackToken = tokenSource.GetToken();
        }

        if (myAttackToken != null)
        {
            // 攻撃権あり：指令を実行
            Vector2 playerPos = tool.PlayerPosition();
            if (command == 1) // 突進
            {
                Vector2 dir = (playerPos - (Vector2)transform.position).normalized;
                speed = dir * 8.0f;
            }
            else if (command == 2) // 回転
            {
                float rotSpeed = 5.0f;
                float radius = 2.0f;
                // 回転の開始位置も個体差をつける
                Vector2 offset = (Vector2)transform.position - playerPos;
                float angle = Mathf.Atan2(offset.y, offset.x);
                angle += rotSpeed * Time.deltaTime;
                Vector2 nextPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                speed = (nextPos - (Vector2)transform.position) / Time.deltaTime;
            }
        }
        else
        {
            // 攻撃権なし：順番待ち待機
            Vector2 targetPos = tool.PlayerPosition();

            // 個体差を使って待機場所を円周上に分散させる
            float waitAngle = (Time.time * 0.5f) + randomOffset;
            Vector2 waitOffset = new Vector2(Mathf.Cos(waitAngle), Mathf.Sin(waitAngle)) * 3.0f;
            waitOffset.y = Mathf.Abs(waitOffset.y) + 1.5f; // プレイヤーより上を維持

            Vector2 dest = targetPos + waitOffset;
            Vector2 dir = (dest - (Vector2)transform.position).normalized;
            speed = dir * 2.5f;

            // 待機中も重ならないように調整
            speed += GetSeparationVector() * 1.5f;

            facingLeft = (targetPos.x < transform.position.x);
        }
    }

    // 仲間と近すぎたら離れるベクトルを計算する
    Vector2 GetSeparationVector()
    {
        Vector2 separateForce = Vector2.zero;
        GameObject[] siblings = tool.GetSinblings(); // 自分以外の仲間を取得

        if (siblings == null) return Vector2.zero;

        float separationDist = 1.0f; // この距離より近づいたら離れる

        foreach (var sibling in siblings)
        {
            if (sibling == null) continue;

            float dist = Vector2.Distance(transform.position, sibling.transform.position);

            // 近すぎる場合
            if (dist < separationDist && dist > 0.01f) // 0除算防止
            {
                // 相手と逆方向へのベクトルを作る
                Vector2 away = (Vector2)transform.position - (Vector2)sibling.transform.position;
                // 近ければ近いほど強い力で反発する
                separateForce += away.normalized / dist;
            }
        }
        return separateForce;
    }

    public void DeadAction()
    {
        // 死亡時にトークンを返却★★★★★★★★★★★★★★
        if (myAttackToken != null)
        {
            myAttackToken.Return();
            myAttackToken = null;
        }
        animator.SetBool("IsDead", true);
        speed = Vector2.zero;
        Destroy(gameObject, 1.0f);
    }

    public void Delete()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // 削除時にトークンを返却★★★★★★★★★★★★★★★★★
        if (myAttackToken != null)
        {
            myAttackToken.Return();
        }
    }

    void Movement()
    {
        GetComponent<SpriteRenderer>().flipX = facingLeft;
        Vector3 pos = transform.position;
        pos.x += speed.x * Time.deltaTime;
        pos.y += speed.y * Time.deltaTime;
        if (pos.y < 0.8f) pos.y = 0.8f;
        transform.position = pos;
    }

    bool IsPlayerHidden()
    {
        if (tool.DistanceToPlayer() > 15.0f) return true;
        return false;
    }

    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            float direction = damage / Mathf.Abs(damage);
            damage = Mathf.Abs(damage);
            life -= damage;
            if (hitCoroutine != null) StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitTime());
        }
    }

    IEnumerator HitTime()
    {
        isHitted = true;
        isInvincible = true;
        yield return new WaitForSeconds(0.5f);
        isHitted = false;
        isInvincible = false;
        hitCoroutine = null;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" && life > 0)
        {
            collider.gameObject.GetComponent<CharacterController2D>().ApplyDamage(1f, transform.position);
        }
    }
}