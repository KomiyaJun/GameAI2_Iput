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
		soldier.facingLeft = soldier.tool.IsPlayerLeftside();
		if(soldier.facingLeft){
			soldier.speed.x = -2.5f;
		}else{
			soldier.speed.x = 2.5f;
		}
	}
	
	public override void OnExit()
	{
		
	}
	
	public override Soldier.State CheckTransitions()
	{
		if(soldier.isHitted){
			return Soldier.State.DAMAGE;
		}
		float d = soldier.tool.DistanceToPlayer();

		if(d < 1.5f){
			return Soldier.State.MELEE_ATTACK;
		}
		/*else if(d < 8f){
			return Soldier.State.RANGE_ATTACK;
		}*/
        if (d>=10)
        {
			return Soldier.State.WAIT;
        }
        return Soldier.State.RUN;
	}
	
}

