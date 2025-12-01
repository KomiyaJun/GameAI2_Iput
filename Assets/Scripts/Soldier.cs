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
		DAMAGE,
		DEAD,
		CHASE,
		ESCAPE,
	};
	State currentState = 0;
	
	private List<Soldier_FSM_Base> availableStates = new List<Soldier_FSM_Base>();

    public bool isTrapTouched = false;	//プレイヤーが罠を踏んだか
	public SpriteRenderer spriteRenderer;	//表示のオンオフ
	public bool isInvisible = false;    //透明になっているか
	public bool isAnimating = false;    //アニメーションを行っているか
	public Transform targetTransform = null;	//アニメーションに利用するTransform

    // ------------------------------------------------------
    // Start is called before the first frame update
    void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		tool = new EnemyComp(this.gameObject);
		animator = GetComponent<Animator>();
		attackCheck = transform.Find("AttackCheck").transform;
		rb = GetComponent<Rigidbody2D>();

		availableStates.Add(new Soldier_FSM_Wait(this));
		availableStates.Add(new Soldier_FSM_Run(this));
		availableStates.Add(new Soldier_FSM_Attack(this));
		availableStates.Add(new Soldier_FSM_Damage(this));
		availableStates.Add(new Soldier_FSM_Dead(this));
		availableStates.Add(new Soldier_FSM_Chase(this));
		availableStates.Add(new Soldier_FSM_Escape(this));

		currentState = State.WAIT;
		availableStates[(int)currentState].OnEnter();
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
		animator.SetBool("Run", false);
		animator.SetTrigger("Attack");
		MakeAttackHit();
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
	
		GameManager.instance.DispStr("Soldier" + ":" + currentState + ":" + new Vector2(position.x, position.y));
	
		// ===================================================
		// AIを作りましょう
		State nextState = availableStates[(int)currentState].CheckTransitions();
		// 死亡は強制的に変更
		if (life <= 0 && currentState != State.DEAD) {
			nextState = State.DEAD;
		}
		
		if (nextState != currentState)
		{
			//今のステートを終了
			availableStates[(int)currentState].OnExit();
			//次のステートの前処理
			availableStates[(int)nextState].OnEnter();
			//新しいステートを記録
			currentState = nextState;
		}
		//ステート実行
		availableStates[(int)currentState].OnUpdate();
		// ===================================================
		
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
	
	//透明化をオフ
	public void OffInvisible()
	{
		isInvisible = false;
		spriteRenderer.enabled = true;
		Debug.Log("可視");
		spriteRenderer.color = new Color(0, 0, 0, 255);
	}
	

	//透明化をオン
	public void OnInvisible()
	{
		isInvisible = true;
		spriteRenderer.color = new Color(0, 0, 0, 0);

		//StartCoroutine(StartFadeAnimation(spriteRenderer, 1f));
		//for(int i = 0; i < 255; i++)
		//{
		//	spriteRenderer.color -= new Color(0,0,0,1);
		//}
		//Debug.Log("不可視");
	}

	//トラップが踏まれた
    public void OnTrapTouched()
    {
		isTrapTouched = true;
    }

	//徐々に透明にしていく
    IEnumerator StartFadeAnimation(SpriteRenderer sr, float duration)
    {
        // 開始時の透明度
        float startAlpha = sr.color.a;
        // 経過時間
        float elapsedTime = 0f;

        // 透明度 (a) が 0.0 になるまでループを続ける
        while (elapsedTime < duration)
        {
            // 時間の経過を記録
            elapsedTime += Time.deltaTime;

            // 経過時間に基づき、新しい透明度を計算
            // 0 から duration の間で、alphaを startAlpha から 0 へ線形補間
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);

            // スプライトの色を更新
            Color newColor = sr.color;
            newColor.a = newAlpha;
            sr.color = newColor;

            // 次のフレームまで待機
            yield return null;
        }

        // 念のため、最後に完全に透明に設定
        Color finalColor = sr.color;
        finalColor.a = 0f;
        sr.color = finalColor;
    }

    public IEnumerator PlayChaseStartAnimation(float targetYScale, float duration)
    {
        if (targetTransform == null) yield break;

        isAnimating = true;

        // ★重要: 現在のXとZのスケールを保存しておく
        float preservedX = targetTransform.localScale.x;
        float preservedZ = targetTransform.localScale.z;

        // Y軸だけ0の状態 (XとZは元のまま)
        Vector3 startScaleVec = new Vector3(preservedX, 0f, preservedZ);

        // Y軸だけ目標値の状態 (XとZは元のまま)
        Vector3 targetScaleVec = new Vector3(preservedX, targetYScale, preservedZ);

        float halfDuration = duration / 2f;

        // --- 1. 伸びる (Y: 0 -> target) ---
        // スタート地点をセット
        targetTransform.localScale = startScaleVec;

        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;

            // 保存したX,Zを維持したまま、Yだけ変化させる
            targetTransform.localScale = Vector3.Lerp(startScaleVec, targetScaleVec, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        targetTransform.localScale = targetScaleVec;

        // --- 2. 透明化 & 移動開始 ---
        OnInvisible();
        isAnimating = false; // 移動ロック解除

        // ※注意: 移動開始後に向き(Xスケール)が変わる可能性がある場合、
        // ここで最新のXを取得しなおす必要が出るかもしれませんが、
        // 透明中なので基本的には元のpreservedXを使って戻して問題ありません。

        // --- 3. 縮む (Y: target -> 0) ---
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;

            targetTransform.localScale = Vector3.Lerp(targetScaleVec, startScaleVec, t);
            yield return null;
        }
        targetTransform.localScale = startScaleVec;
    }
}
