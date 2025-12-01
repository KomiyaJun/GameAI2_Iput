using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComp {
	
	GameObject me;
	GameObject player;
	
	// コンストラクタ
	public EnemyComp(GameObject enemyObject){
		me = enemyObject;
		player = GameObject.Find("Player");
	}
	
	// プレイヤーの位置を返す
	public Vector2 PlayerPosition(){
		return new Vector2(player.transform.position.x, player.transform.position.y);
	}
	
	// プレイヤーまでの距離を返す
	public float DistanceToPlayer(){
		Vector3 playerPosition = player.transform.position;
		Vector2 toPlayer = new Vector2(playerPosition.x - me.transform.position.x, playerPosition.y - me.transform.position.y);
		float distance = toPlayer.magnitude;
		return distance;
	}

	// 横方向のプレイヤーまでの距離を返す
	public float DistanceXToPlayer(){
		return Mathf.Abs(player.transform.position.x - me.transform.position.x);
	}
	
	// プレイヤーが左にいれば true を、右にいれば false を返す
	public bool IsPlayerLeftside(){
		bool facingLeft;
		Vector3 playerPosition = player.transform.position;
		if(playerPosition.x > me.transform.position.x){
			facingLeft = false;
		}else{
			facingLeft = true;
		}
		return facingLeft;
	}

    public bool IsPlayerFacingRight()
    {
        // プレイヤーオブジェクトから CharacterController2D コンポーネントを取得
        CharacterController2D playerController = player.GetComponent<CharacterController2D>();

        if (playerController != null)
        {
            // プレイヤーの FacingRight プロパティを参照
            return playerController.FacingRight;
        }
        return true; // コンポーネントがない場合のデフォルト値
    }
}
