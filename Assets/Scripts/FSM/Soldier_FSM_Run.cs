using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier_FSM_Run : Soldier_FSM_Base
{
	public Soldier_FSM_Run(Soldier s) : base(s)
	{
		
	}
	
	public override void OnEnter()
	{
		soldier.RunAction();
	}
	
	public override void OnUpdate()
	{
        MineTrap trap = GameObject.FindObjectOfType<MineTrap>();
        if (trap.Attattayo)
        {
            soldier.speed.x = 0;
        }
        else
        {

        }

    }
	
	public override void OnExit()
	{
		
	}

    private void PerformTeleportAttack()
    {
        const float TELEPORT_DISTANCE = 0.5f;

      
        Vector2 playerPos = soldier.tool.PlayerPosition();

        // プレイヤーの向きを取得
        bool playerFacingRight = soldier.tool.IsPlayerFacingRight();
        Vector3 targetPosition;

        if (playerFacingRight)
        {
            // プレイヤーが右向き なら、背後である左側へワープ
            targetPosition = new Vector3(
                playerPos.x - TELEPORT_DISTANCE,
                playerPos.y,
                soldier.transform.position.z);

            // ワープ後、ソルジャーはプレイヤーの方（右）を向く
            soldier.facingLeft = false;
        }
        else
        {
            // プレイヤーが左向きなら、背後である右側へワープ
            targetPosition = new Vector3(
                playerPos.x + TELEPORT_DISTANCE,
                playerPos.y,
                soldier.transform.position.z);

            // ワープ後、ソルジャーはプレイヤーの方（左）を向く
            soldier.facingLeft = true;
        }

        // 瞬間移動を実行
        soldier.transform.position = targetPosition;

        // 物理挙動をリセット
        if (soldier.GetComponent<Rigidbody2D>() != null)
        {
            soldier.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }

    public override Soldier.State CheckTransitions()
	{

        if (soldier.isHitted)
        {
            return Soldier.State.DAMAGE;
        }
        if (soldier.tool.DistanceToPlayer() <= 10f) 
        {
            PerformTeleportAttack();
            return Soldier.State.MELEE_ATTACK;
        }
        if (soldier.tool.DistanceToPlayer() > 5f){
			return Soldier.State.WAIT;
		}
		return Soldier.State.RUN;
	}
	
}

