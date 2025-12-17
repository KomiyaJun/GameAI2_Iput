using UnityEngine;

public class Soldier_FSM_shout : Soldier_FSM_Base
{
    float timer;
    public Soldier_FSM_shout(Soldier s) : base(s)
    {

    }
    public override void OnEnter()
    {

        soldier.speed.x = 0.0f;    // 右が+。左が-になります
        soldier.speed.y = 0.0f;    // 上が+、下が-になります
        soldier.Shout();
        timer = 3f;
    }

    // このステートで毎回実行する
    public override void OnUpdate()
    {
        timer -= Time.deltaTime;
    }

    // このステートを終了する時に1度実行する
    public override void OnExit()
    {

    }

    // 遷移先のステートを返す
    // 今のステートを継続する時は、現在のステートを返す
    public override Soldier.State CheckTransitions()
    {
        if (soldier.isHitted)
        {
            return Soldier.State.DAMAGE;
        }
        if (timer <= 0)
        {
            return Soldier.State.WAIT;
        }
        return Soldier.State.Shout;
    }
}
