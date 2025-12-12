using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier_FSM_RangeAttack : Soldier_FSM_Base
{
    float timer;

    public Soldier_FSM_RangeAttack(Soldier s) : base(s)
    {
    }

    public override void OnEnter()
    {
        soldier.RangeAttackAction();
        timer = 2.0f; // 攻撃アニメーション＋硬直時間（調整してください）
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

        // 攻撃動作が終わったら
        if (timer < 0)
        {
            // ■■■ 追加：当たってないなら追いかける ■■■
            // 5秒以上ヒットしていなければ、WaitせずRunへ移行して距離を詰めさせる
            if (soldier.timeSinceLastHit >= 5.0f)
            {
                return Soldier.State.RUN;
            }

            // 通常時は待機に戻る（Waitからまた距離判定でRangeかRunか決まる）
            return Soldier.State.WAIT;
        }

        return Soldier.State.RANGE_ATTACK;
    }
}