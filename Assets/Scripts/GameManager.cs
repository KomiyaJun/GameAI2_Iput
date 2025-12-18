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
		
	}

	public void OnCreateBats()
	{

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
