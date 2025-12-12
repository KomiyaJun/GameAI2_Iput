using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier_FSM_Wait : Soldier_FSM_Base
{
    public Soldier_FSM_Wait(Soldier s) : base(s)
    {

    }

    public override void OnEnter()
    {
        soldier.WaitAction();
        soldier.speed.x = 0.0f;
        soldier.speed.y = 0.0f;
    }

    public override void OnUpdate()
    {
        // 既存のコードにあるGameManager経由の処理は、もしGameManagerを使わないなら削除してOKです
        // ここではサンプルとして残しておきますが、遷移チェックは CheckTransitions で行います
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

        // ■■■ 修正：ブラックボードの SquadState を監視 ■■■
        if (soldier.squadBoard != null)
        {
            int squadState;
            soldier.squadBoard.GetValue(BlackBoardKey.SquadState, out squadState);

            // Batが発見モード(1)に書き換えていたら、Soldierも追跡(RUN)へ
            if (squadState == 1)
            {
                return Soldier.State.RUN;
            }
        }

        // ソルジャー自身がプレイヤーを近くで見つけた場合も追跡へ（オプション）
        if (soldier.tool.DistanceToPlayer() < 5.0f)
        {
            // 自身で見つけた場合、チーム全体に通知するためにブラックボードを書き換える
            if (soldier.squadBoard != null)
                soldier.squadBoard.SetValue(BlackBoardKey.SquadState, 1);

            return Soldier.State.RUN;
        }

        return Soldier.State.WAIT;
    }
}