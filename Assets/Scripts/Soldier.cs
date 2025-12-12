using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
	public float life = 10;
	public GameObject throwableObject;
	
	bool isInvincible = false;
	public bool isHitted = false;

	Transform attackCheck;
	Rigidbody2D rb;
	
	// 自身のanimatorの保持
	public Animator animator;
	// 便利関数つめあわせ
	public EnemyComp tool;
	private Coroutine hitCoroutine;
	
	// ------------------------------------------------------
	// #1 キャラクターの動き（身体）
	// 向き： true の時に左向き、false の時に右向き
	public bool facingLeft = true;
	// 移動速度
	public Vector2 speed = Vector2.zero;	// x:右が+。左が-  y:上が+、下が-
	// 位置：参照用
	public Vector2 position;	
	// タイマー
	float timer;
		
	// ------------------------------------------------------
	// #2 意思決定
	// ステート
	public enum State {
		WAIT,
		RUN,
		MELEE_ATTACK,
		RANGE_ATTACK,
		DAMAGE,
		DEAD,
	};
	State currentState = 0;
	
	private List<Soldier_FSM_Base> availableStates = new List<Soldier_FSM_Base>();
	
	// ------------------------------------------------------
	// #3 マルチエージェント
	public bool leader;
	public int set;
	Soldier[] teamMember;

    // ------------------------------------------------------


    //==================小宮=====================

    //トークン
    //近接攻撃が当たったことを通知するためのトークン
    TokenSource MeleeToken = new TokenSource(1);

	//遠距離攻撃が当たったことを通知するためのトークン
	TokenSource RangeToken = new TokenSource(1);

	[SerializeField]public BlackBoard squadBoard;

    public float timeSinceLastHit = 0f;

    // Start is called before the first frame update
    void Start()
    {
        tool = new EnemyComp(this.gameObject);
        animator = GetComponent<Animator>();
        attackCheck = transform.Find("AttackCheck").transform;
        rb = GetComponent<Rigidbody2D>();

        GameObject[] allies = tool.GetSinblings();
        teamMember = new Soldier[allies.Length];
        for (int i = 0; i < teamMember.Length; i++)
        {
            teamMember[i] = allies[i].GetComponent<Soldier>();
        }

        availableStates.Add(new Soldier_FSM_Wait(this));
        availableStates.Add(new Soldier_FSM_Run(this)); // Runステートの実装が必要
        availableStates.Add(new Soldier_FSM_Attack(this));
        availableStates.Add(new Soldier_FSM_RangeAttack(this));
        availableStates.Add(new Soldier_FSM_Damage(this));
        availableStates.Add(new Soldier_FSM_Dead(this));

        currentState = State.WAIT;
        availableStates[(int)currentState].OnEnter();

        if (Bat.squadBoard != null)
        {
            squadBoard = GameManager.instance.blackBoard;
        }

		if(BoardM.instance != null)
		{
			squadBoard = BoardM.instance.sharedBoard;
		}
		else
		{
			Debug.LogError("BoardManagerがシーンにありません。");
		}
			
    }
    void OnDestroy()
	{
		if(hitCoroutine != null){
			StopCoroutine(hitCoroutine);
			hitCoroutine = null;
		}
	}
	
	// Updateの最初に行う
	void FirstInUpdate()
	{
		position.x = transform.position.x;
		position.y = transform.position.y;
		speed = rb.linearVelocity;
	}

	// 待機モーション
	public void WaitAction()
	{
		animator.SetBool("Run", false);
	}
		
	// 走りモーション
	public void RunAction()
	{
		animator.SetBool("Run", true);
	}
	
	// 攻撃モーション
	public void AttackAction()
	{
		Debug.Log("Melee");
		animator.SetBool("Run", false);
		animator.SetTrigger("Attack");
		MakeAttackHit();
	}
	
	public void RangeAttackAction()
	{
		Debug.Log("Shot");
		animator.SetBool("Run", false);
		MakeShot(facingLeft);
	}
	
	// 死にモーション
	public void DeadAction()
	{
		animator.SetBool("IsDead", true);
		// アタリ判定を変形させる
		ChangeBodyHitToDead();
	}
	
	// このキャラクターを削除する
	public void Delete()
	{
		Destroy(gameObject);
	}

    // Update is called once per frame
    void Update()
    {
        FirstInUpdate();


        // ■■■ 追加：時間の計測 ■■■
        // 攻撃が当たらない間、どんどんカウントアップされていく
        timeSinceLastHit += Time.deltaTime;

        State nextState = availableStates[(int)currentState].CheckTransitions();
        if (life <= 0 && currentState != State.DEAD)
        {
            nextState = State.DEAD;
        }

        if (nextState != currentState)
        {
            availableStates[(int)currentState].OnExit();
            availableStates[(int)nextState].OnEnter();
            currentState = nextState;
        }
        availableStates[(int)currentState].OnUpdate();

        Movement();
    }

    // 自キャラの移動処理をまとめています
    void Movement(){
		// 向き処理
		GetComponent<SpriteRenderer>().flipX = !facingLeft;
		// 移動処理
		rb.linearVelocity = speed;
	}
	
	// ===============================================================
	// 以下、このシステムの処理
	void MakeAttackHit(){
		Collider2D[] collidersEnemies = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
		for (int i = 0; i < collidersEnemies.Length; i++){
			if (collidersEnemies[i].gameObject.tag == "Player"){
				collidersEnemies[i].gameObject.GetComponent<CharacterController2D>().ApplyDamage(2f, transform.position);
			}
		}

		OnMeleeAttackHit();	//近接攻撃トークンを発生
	}
	
	void ChangeBodyHitToDead(){
		CapsuleCollider2D capsule;
		capsule = GetComponent<CapsuleCollider2D>();
		capsule.size = new Vector2(1f, 0.25f);
		capsule.offset = new Vector2(0f, -0.8f);
		capsule.direction = CapsuleDirection2D.Horizontal;
	}
	
	public void ApplyDamage(float damage) {
		if (!isInvincible) 
		{
			float direction = damage / Mathf.Abs(damage);
			damage = Mathf.Abs(damage);
			animator.SetBool("Hit", true);
			life -= damage;
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(new Vector2(direction * 500f, 100f));
			if(hitCoroutine != null)
			{
				StopCoroutine(hitCoroutine);
			}
			hitCoroutine = StartCoroutine(HitTime());
		}
	}

	// 攻撃を受けた後、一定時間無敵になる
	IEnumerator HitTime()
	{
		isHitted = true;
		isInvincible = true;
		yield return new WaitForSeconds(0.3f);
		isHitted = false;
		isInvincible = false;
		hitCoroutine = null;
	}
	
	// 遠距離攻撃の弾を発射する
	// toLeft : 左側に出す
	void MakeShot(bool toLeft){
		float offset = 0.5f;
		float speed = 0.5f;
		if(toLeft){
			offset = -offset;
			speed = -speed;
		}
		GameObject throwableProj = Instantiate(throwableObject, transform.position + new Vector3(offset, -0.2f), Quaternion.identity) as GameObject;
		throwableProj.GetComponent<ThrowableProjectile>().owner = gameObject;
		Vector2 direction = new Vector2(speed, 0f);
		throwableProj.GetComponent<ThrowableProjectile>().direction = direction;
	}

    //遠距離攻撃トークンを起動
    public void OnRangeAttackHit()
    {
        // ■■■ 追加：ヒットしたのでタイマーをリセット（満足してまた遠距離攻撃するようになる） ■■■
        timeSinceLastHit = 0f;

        Debug.Log("遠距離攻撃が当たりました。Batへ突進指令(1)を出します");
        RangeToken.GetToken();

        if (squadBoard != null)
        {
            squadBoard.SetValue(BlackBoardKey.BatCommand, 1);
            StartCoroutine(ResetCommandTimer());
        }
    }

    // 近距離攻撃ヒット時の処理（既存）
    public void OnMeleeAttackHit()
    {
        // ■■■ 追加：近接でも当たればリセット ■■■
        timeSinceLastHit = 0f;

        Debug.Log("近距離攻撃が当たりました。Batへ回転指令(2)を出します");
        MeleeToken.GetToken();

        if (squadBoard != null)
        {
            squadBoard.SetValue(BlackBoardKey.BatCommand, 2);
            StartCoroutine(ResetCommandTimer());
        }
    }
    // 指令を一定時間後に解除するコルーチン
    IEnumerator ResetCommandTimer()
    {
        yield return new WaitForSeconds(1.0f); // 1秒間だけ指令を有効にする
        if (squadBoard != null)
        {
            squadBoard.SetValue(BlackBoardKey.BatCommand, 0);
        }
    }

}
