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
        // ... (移動処理は変更なし) ...
        Vector2 targetPos = soldier.tool.PlayerPosition();
        Vector2 myPos = soldier.transform.position;
        float xDiff = targetPos.x - myPos.x;
        if (xDiff > 0) soldier.facingLeft = false;
        else soldier.facingLeft = true;

        float moveSpeed = 2.0f;
        if (soldier.facingLeft) soldier.speed.x = -moveSpeed;
        else soldier.speed.x = moveSpeed;
    }

    public override void OnExit()
    {
        soldier.speed = Vector2.zero;
    }

    public override Soldier.State CheckTransitions()
    {
        if (soldier.isHitted) return Soldier.State.DAMAGE;

        float dist = soldier.tool.DistanceToPlayer();

        // 1. 近距離範囲なら、問答無用で殴る
        if (dist < 1.5f)
        {
            return Soldier.State.MELEE_ATTACK;
        }

        // 2. 遠距離範囲の場合の判断
        // 「距離が適切」かつ「最近攻撃が当たっている(5秒以内)」場合のみ遠距離攻撃をする
        // つまり、5秒以上当たっていないと、ここを無視して走り続ける（＝距離を詰める）
        else if (dist < 6.0f && dist >= 3.0f)
        {
            // ■■■ 修正：痺れを切らしていないかチェック ■■■
            if (soldier.timeSinceLastHit < 5.0f)
            {
                return Soldier.State.RANGE_ATTACK;
            }
        }

        // 3. 追跡終了判定
        if (soldier.squadBoard != null)
        {
            int squadState;
            soldier.squadBoard.GetValue(BlackBoardKey.SquadState, out squadState);
            if (squadState == 0) return Soldier.State.WAIT;
        }

        if (dist > 15.0f)
        {
            if (soldier.squadBoard != null)
                soldier.squadBoard.SetValue(BlackBoardKey.SquadState, 0);
            return Soldier.State.WAIT;
        }

        return Soldier.State.RUN;
    }
}