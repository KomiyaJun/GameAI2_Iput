using UnityEngine;

public class Soldier_FSM_Escape : Soldier_FSM_Base
{
    public Soldier_FSM_Escape(Soldier s) : base(s)
    {

    }

    public override void OnEnter()
    {
        soldier.RunAction();

    }


    public override void OnUpdate()
    {
        soldier.facingLeft = !soldier.tool.IsPlayerLeftside();

        //ƒvƒŒƒCƒ„[‚Æ‚Í‹t•ûŒü‚É“¦‚°‚é
        if (soldier.facingLeft)
        {
            soldier.speed.x = -10.0f;
        }
        else
        {
            soldier.speed.x = 10.0f;
        }

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

        if(soldier.tool.DistanceToPlayer() > 12f)
        {
            return Soldier.State.CHASE;
        }

        return Soldier.State.ESCAPE;
    }


}
