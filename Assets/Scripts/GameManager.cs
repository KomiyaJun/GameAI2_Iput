using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;
	public GameObject batPrefab;
	public GameObject soldierPrefab;
	public BlackBoard blackBoard;

	// デバッグ用文字表示 DispStr用
	GameObject debugText;
	
	StringBuilder buffer = new StringBuilder();
	
	// ------------------------------------------------------
	// #4 メタAI
	
	// position の位置に Bat を生成する
	// groupParent の指定がある場合は、groupParent の子にする
	Bat CreateBat(Vector2 position, GameObject groupParent = null){
		Vector3 appearPos = new Vector3(position.x, position.y, 0f);
		GameObject g = Instantiate(batPrefab, appearPos, Quaternion.identity);
		Bat newBat = g.GetComponent<Bat>();
		if(groupParent != null){
			newBat.transform.SetParent(groupParent.transform);
		}
		return newBat;
	}

	// position の位置に Soldier を生成する
	// groupParent の指定がある場合は、groupParent の子にする
	Soldier CreateSoldier(Vector2 position, GameObject groupParent = null){
		Vector3 appearPos = new Vector3(position.x, position.y, 0f);
		GameObject g = Instantiate(soldierPrefab, appearPos, Quaternion.identity);
		Soldier newSoldier = g.GetComponent<Soldier>();
		if(groupParent != null){
			newSoldier.transform.SetParent(groupParent.transform);
		}
		return newSoldier;
	}
	
	// グループの親になる GameObject を作る
	GameObject CreateGroupParent()
	{
		GameObject g = new GameObject("group");
		g.AddComponent<EnemyGroup>();
		return g;
	}

	// 参考：出現テーブルを作って、それに合わせて敵を出す
	struct Appear {
		public Appear(float x, float y, int setValue){
			offset = new Vector2(x,y);
			set = setValue;
		}
		public Vector2 offset;	// 出現位置（のオフセット）
		public int set;	// こんな感じで他のパラメータ設定も設定できる、という例
	}
	Appear[] appearTable = new Appear[]{
		new Appear(0.0f,0.0f, 0),
		new Appear(1.0f,0.0f, 1),
		new Appear(2.0f,0.0f, 2),
	};
	
	// 参考：ゲーム内のObjectから参照できる値を設定する
	public int difficulty = 0;	// 数が多くなると難しくなるとか

	// 参考：トークンで管理してみる
	TokenSource soldierTokenSource = new TokenSource(5);
	
	void Awake(){
		instance = this;
		
		// ゲーム開始時に設定したいことがあれば、ここで行いましょう
		blackBoard = new BlackBoard();
	}
	
	// Start is called before the first frame update
	void Start()
	{
		debugText = GameObject.Find("DebugText");
	}

	// Update is called once per frame
	float default_x = 5.0f;
	Bat leader;
	void Update()
	{
		// ここからサンプル---------------------------------------------------------------------
		// #4 メタAI：オブジェクト生成の例
		
		// 一つずつ出現させる例：こうもり
		if(Input.GetKeyDown(KeyCode.Space)){
			// 出現位置
			Vector2 basePosition = new Vector2(default_x, 3f);
			// グループにする
			GameObject parent = CreateGroupParent();
			Bat bat;
			bat = CreateBat(basePosition, parent);
			bat.leader = true;	// 作った後にリーダーに設定とか
			leader = bat;	// リーダーを覚えておく（すぐ下のキャラに指示を出す例を動かすため）
			
			basePosition.x += 2f;
			bat = CreateBat(basePosition, parent);
			bat.set = 1;	// デフォルトの値から変える必要があれば他の項目も設定する

			// グループにしないやつも作る
			basePosition.x -= 4f;
			bat = CreateBat(basePosition);
			
			default_x += 0.5f;	// 出現位置をずらす
		}
		
		// ゲームキャラに指示を出す例
		// Blackboard に指示を書きこむ方法もあります
		if(leader != null && leader.gameObject != null){
			leader.SetMoveEnable(true);	// リーダーに指示を出す
			// ここで動かすとリーダーが Wait ステートにならないため、同じグループの Bat が動かない（メタAI側でへたに強制的に動かすと発生する問題の例）
		}
		
		// 出現テーブルに合わせて出現させる例：戦士
		if(Input.GetKeyDown(KeyCode.Tab)){
			// 出現位置
			Vector2 basePosition = new Vector2(default_x, 5f);
			for(int i=0; i<3; i++){
				Appear appear = appearTable[i];	// 出現設定を取得
				Vector2 p = basePosition + appear.offset;
				// トークンの数で出現数を制限してみる
				Token token = soldierTokenSource.GetToken();
				if(token != null){
					Soldier soldier = CreateSoldier(p);
					soldier.set = appear.set;	// 出現設定に合わせて設定
					soldier.life = 1;	// デモとしてすぐに倒せるように
					soldier.SetToken(token);	// トークンを渡す
				}
			}
			
			default_x += 0.5f;	// 出現位置をずらす
		}
		// ここまでサンプル---------------------------------------------------------------------
		
	}
	
	void LateUpdate()
	{
		if(debugText != null){
			debugText.GetComponent<TextMeshProUGUI>().text = buffer.ToString();
		}
		buffer.Clear();
	}
	
	public void DispStr(string str){
		buffer.Append(str + "\n");
	}
	
}
