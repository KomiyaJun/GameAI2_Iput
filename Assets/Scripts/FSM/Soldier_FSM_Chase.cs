using UnityEngine;

public class Soldier_FSM_Chase : Soldier_FSM_Base
{
    float timer;

    public Soldier_FSM_Chase(Soldier s) : base(s)
    {

    }

    public override void OnEnter()
    {
        soldier.AttackAction();
        timer = 0.5f;
    }

    public override void OnUpdate()
    {
        timer -= Time.deltaTime;
    }

    public override void OnExit()
    {
    }

    public override Soldier.State CheckTransitions()
    {
        if (soldier.isHitted)
        {
            return Soldier.State.DAMAGE;
        }

        //プレイヤーが感知範囲に入ったら
        //return Soldier.State.Attack

        return Soldier.State.MELEE_ATTACK;
    }

}
