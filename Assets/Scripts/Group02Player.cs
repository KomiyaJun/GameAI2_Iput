using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Group02
{
    public class Group02Player : Pawn
    {
        enum State
        {
            Planning,    // 経路計画中
            Moving,      // 移動中
            Finished     // 担当分完了（または待機）
        }

        State state = State.Planning;

        // 自分の担当するターゲットのリスト（順番通り）
        Queue<int> myRouteQueue = new Queue<int>();

        // 現在目指しているターゲット
        int currentTargetIndex = -1;

        NavMeshTool navi;

        // --- 全インスタンス共有の計画データ ---
        // 計算済みフラグ
        static bool isPlanCompleted = false;
        // 2体分のルートリスト [PlayerID, List<TargetIndex>]
        static Dictionary<int, List<int>> plannedRoutes = new Dictionary<int, List<int>>();
        // 計算中のロック
        static bool isCalculating = false;

        void Start()
        {
            navi = GetNavMeshTool();

            // 静的変数の初期化（シーン遷移時などを考慮してリセットが必要なら適宜行う）
            // ※ここでは簡易的に、最初の1回だけ初期化するロジックにします
            if (!isPlanCompleted && !isCalculating)
            {
                StartCoroutine(CalculateOptimalRoute());
            }
        }

        void Update()
        {
            navi.UpdateCurrentPosition();

            // 計画完了待ち
            if (!isPlanCompleted)
            {
                // まだ計算中なら待機
                return;
            }

            // 初回のルート受取
            if (state == State.Planning)
            {
                int myID = this.GetInstanceID();
                if (plannedRoutes.ContainsKey(myID))
                {
                    // 自分の担当分をQueueに変換
                    foreach (var t in plannedRoutes[myID])
                    {
                        // まだ有効なターゲットだけ追加
                        if (GameManager.instance.IsTargetActive(t))
                        {
                            myRouteQueue.Enqueue(t);
                        }
                    }
                }
                state = State.Moving;
                GetNextTarget();
            }

            // 移動ロジック
            if (state == State.Moving)
            {
                // ターゲットが既に他（他チーム等）に取られていないかチェック
                if (currentTargetIndex != -1 && !GameManager.instance.IsTargetActive(currentTargetIndex))
                {
                    // ターゲット消滅 -> 次へ
                    GetNextTarget();
                    return;
                }

                if (currentTargetIndex != -1)
                {
                    // 移動制御
                    Vector3 dir = navi.MoveDirection();
                    SetDirection(dir);
                    SetMoveSpeed(10f); // 最大速度を設定（Pawn側でキャップされるが大きめに指定）

                    // 到着判定
                    if (navi.IsArrived())
                    {
                        // NavMeshToolのIsArrivedだけでなく、実際の距離も念のため見る
                        float dist = Vector3.Distance(GetPosition(), GameManager.instance.TargetPosition(currentTargetIndex));
                        if (dist < 1.0f) // 接触判定
                        {
                            // Pawn.csのOnTriggerEnter等で処理されるはずだが、念の為次へ
                            // ここでは明示的に処理せず、TargetHitが呼ばれるのを期待するか、
                            // 次のフレームでActiveチェックに引っかかるのを待つ
                        }
                    }
                }
                else
                {
                    // ターゲットがないなら停止
                    SetMoveSpeed(0f);
                }
            }
        }

        // 次のターゲットをキューから取り出す
        void GetNextTarget()
        {
            currentTargetIndex = -1;

            // 有効なターゲットが見つかるまでキューを回す
            while (myRouteQueue.Count > 0)
            {
                int next = myRouteQueue.Dequeue();
                if (GameManager.instance.IsTargetActive(next))
                {
                    currentTargetIndex = next;
                    navi.SetDestination(GameManager.instance.TargetPosition(next));
                    return;
                }
            }

            // 自分の担当が終わった場合
            // 暇なら、まだ残っている有効なターゲットの中で一番近いものを手伝いに行く
            FindSupportTarget();
        }

        // 担当分が終わった後のヘルプ行動
        void FindSupportTarget()
        {
            int bestIdx = -1;
            float minCost = float.MaxValue;
            Vector3 myPos = GetPosition();

            int num = GameManager.instance.NumTargets();
            for (int i = 0; i < num; i++)
            {
                if (GameManager.instance.IsTargetActive(i))
                {
                    // ここでのコスト計算は簡易的に直線距離でも良いが、
                    // 余裕があればNavMesh距離を使う
                    float d = Vector3.Distance(myPos, GameManager.instance.TargetPosition(i));
                    if (d < minCost)
                    {
                        minCost = d;
                        bestIdx = i;
                    }
                }
            }

            if (bestIdx != -1)
            {
                currentTargetIndex = bestIdx;
                navi.SetDestination(GameManager.instance.TargetPosition(bestIdx));
            }
            else
            {
                // 完全終了
                state = State.Finished;
                SetMoveSpeed(0f);
            }
        }

        public override void TargetHit(int index)
        {
            // ターゲットを取った瞬間の処理
            if (index == currentTargetIndex)
            {
                GetNextTarget();
            }
        }

        // ---------------------------------------------------------
        // ■ 経路最適化ロジック (Min-Max mTSP / Greedy Balance)
        // ---------------------------------------------------------
        IEnumerator CalculateOptimalRoute()
        {
            isCalculating = true;
            isPlanCompleted = false;
            plannedRoutes.Clear();

            // 1. プレイヤーとターゲットの情報を収集
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            // 自分たちのチーム(Group02Playerが付いているもの)だけ抽出
            List<Group02Player> ourTeam = new List<Group02Player>();
            foreach (var p in players)
            {
                var comp = p.GetComponent<Group02Player>();
                if (comp != null) ourTeam.Add(comp);
            }

            // ターゲットリスト取得
            int numTargets = GameManager.instance.NumTargets();
            List<int> availableTargets = new List<int>();
            for (int i = 0; i < numTargets; i++)
            {
                if (GameManager.instance.IsTargetActive(i)) availableTargets.Add(i);
            }

            // 各エージェントのシミュレーション用状態
            int agentCount = ourTeam.Count;
            float[] agentTotalDistance = new float[agentCount];
            Vector3[] agentCurrentPos = new Vector3[agentCount];
            List<int>[] agentRoutes = new List<int>[agentCount];

            for (int i = 0; i < agentCount; i++)
            {
                agentTotalDistance[i] = 0f;
                agentCurrentPos[i] = ourTeam[i].transform.position;
                agentRoutes[i] = new List<int>();
            }

            // 2. シミュレーションループ (Greedy Balancing)
            // 全ターゲットが割り当てられるまで繰り返す
            while (availableTargets.Count > 0)
            {
                // 最も「累積移動距離が短い(暇な)」エージェントを探す
                // これにより完了時間を均等化する
                int bestAgentIdx = -1;
                float minTotalDist = float.MaxValue;

                for (int i = 0; i < agentCount; i++)
                {
                    if (agentTotalDistance[i] < minTotalDist)
                    {
                        minTotalDist = agentTotalDistance[i];
                        bestAgentIdx = i;
                    }
                }

                // そのエージェントにとって、NavMesh距離で最も近いターゲットを探す
                int bestTargetIdx = -1;
                float minPathDist = float.MaxValue;
                Vector3 startPos = agentCurrentPos[bestAgentIdx];

                // 候補の中から探索
                // ※全探索は少し重いが、ターゲット数が数十個なら数msで終わる
                // ※重すぎる場合はここを分割して yield return null を挟む
                foreach (int tIdx in availableTargets)
                {
                    Vector3 tPos = GameManager.instance.TargetPosition(tIdx);

                    // NavMeshToolの距離計算（壁考慮）を使用
                    float dist = NavMeshTool.CalculatePathLength(startPos, tPos);

                    if (dist < minPathDist)
                    {
                        minPathDist = dist;
                        bestTargetIdx = tIdx;
                    }
                }

                // ターゲット決定
                if (bestTargetIdx != -1)
                {
                    // 割り当て
                    agentRoutes[bestAgentIdx].Add(bestTargetIdx);
                    agentTotalDistance[bestAgentIdx] += minPathDist;
                    agentCurrentPos[bestAgentIdx] = GameManager.instance.TargetPosition(bestTargetIdx);

                    // リストから削除
                    availableTargets.Remove(bestTargetIdx);
                }
                else
                {
                    // 到達不能なターゲットが残った場合などの安全策
                    break;
                }

                // 計算負荷分散（ターゲット数が多い場合のみ）
                if (availableTargets.Count % 5 == 0) yield return null;
            }

            // 3. 結果を辞書に格納
            for (int i = 0; i < agentCount; i++)
            {
                int id = ourTeam[i].GetInstanceID();
                plannedRoutes.Add(id, agentRoutes[i]);

                // デバッグ表示
                string routeStr = string.Join("->", agentRoutes[i]);
                GameManager.instance.DispStr($"Agent{i} Dist:{agentTotalDistance[i]:F1} Route:{routeStr}");
            }

            isPlanCompleted = true;
            isCalculating = false;
        }
    }
}