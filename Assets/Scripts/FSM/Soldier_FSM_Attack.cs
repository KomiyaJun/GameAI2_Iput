using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier_FSM_Attack : Soldier_FSM_Base
{
    const float CHARGE_TIME = 0.7f;
    const float COOLDOWN_TIME = 0.3f;
    float timer;
	bool check = false;
	
	public Soldier_FSM_Attack(Soldier s) : base(s)
	{
		
	}
	
	public override void OnEnter()
	{
		soldier.WaitAction();
        soldier.speed.x = 0.0f;    // âEÇ™+ÅBç∂Ç™-Ç…Ç»ÇËÇ‹Ç∑
        soldier.speed.y = 0.0f;
        timer = CHARGE_TIME;
		check = false;

    }
	
	public override void OnUpdate()
	{
		timer -= Time.deltaTime;
		if(timer <= 0f&&check==false)
		{
            soldier.AttackAction();
            check = true;
            timer = COOLDOWN_TIME;
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
		if(timer < 0){
			check = false;
			return Soldier.State.Shout;
		}
		return Soldier.State.MELEE_ATTACK;
	}
	
}

