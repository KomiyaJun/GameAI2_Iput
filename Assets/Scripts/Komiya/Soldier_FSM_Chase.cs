using UnityEngine;

public class Soldier_FSM_Chase : Soldier_FSM_Base
{
    public Soldier_FSM_Chase(Soldier s) : base(s)
    {

    }

    public override void OnEnter()
    {
        soldier.RunAction();
        soldier.OnInvisible();  //開始時、透明に
    }

    public override void OnUpdate()
    {
        soldier.facingLeft = soldier.tool.IsPlayerLeftside();
        if (soldier.facingLeft)
        {
            soldier.speed.x = -2.5f;
        }
        else
        {
            soldier.speed.x = 2.5f;
        }

        if (soldier.isInvisible && soldier.tool.DistanceToPlayer() < 3f)
        {
            soldier.OffInvisible();
        }

    }

    public override void OnExit()
    {
        soldier.OffInvisible();
    }

    public override Soldier.State CheckTransitions()
    {
        if (soldier.isHitted)
        {
            return Soldier.State.DAMAGE;
        }
        if(soldier.tool.DistanceToPlayer() < 1f)
        {
            return Soldier.State.MELEE_ATTACK;  //一定距離近づいた場合、近接攻撃
        }
        if(soldier.tool.DistanceToPlayer() > 20f)
        {
            return Soldier.State.WAIT;
        }

        return Soldier.State.CHASE;
    }
}
