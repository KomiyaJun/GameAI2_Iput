using UnityEngine;
using System.Collections;

public class BoomerangTargetMove : MonoBehaviour
{
    [Header("動かす対象のオブジェクト")]
    [Tooltip("ここに動かしたいオブジェクト（Transform）をセットしてください")]
    [SerializeField]
    private Transform targetObject;

    [Header("移動量の設定")]
    [Tooltip("対象オブジェクトの現在座標からどれだけ動くか")]
    public Vector3 moveOffset = new Vector3(0, 3, 0);

    [Header("戻る動きの設定")]
    [Tooltip("何秒かけて元の位置に戻るか")]
    public float returnDuration = 1.5f;
    [Tooltip("戻る際の行き過ぎる量（大きいほどビヨーンと伸びます）")]
    public float overshootAmount = 1.7f;

    private Vector3 _originalPosition;
    private Coroutine _moveCoroutine;

    public void StartBoomerangMovement()
    {
        // 念のため対象がセットされているか確認
        if (targetObject == null)
        {
            Debug.LogWarning("Target Object が設定されていません！Inspectorでセットしてください。");
            return;
        }

        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveProcess());
    }

    private IEnumerator MoveProcess()
    {
        // 1. 指定されたオブジェクトの位置を記録
        _originalPosition = targetObject.position;

        // 移動先を計算
        Vector3 destination = _originalPosition + moveOffset;

        // 2. 指定されたオブジェクトを瞬間移動
        targetObject.position = destination;

        // --- ここから戻る処理 ---

        float elapsedTime = 0f;
        Vector3 startPos = targetObject.position; // 戻り始めの場所

        while (elapsedTime < returnDuration)
        {
            float t = elapsedTime / returnDuration;

            // イージング関数（行き過ぎて戻る動き）
            float easedT = EaseOutBack(t);

            // 指定されたオブジェクトの位置を更新
            // LerpUnclamped でオーバーシュートを許容
            targetObject.position = Vector3.LerpUnclamped(startPos, _originalPosition, easedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最後はきっちり元の位置へ
        targetObject.position = _originalPosition;
        _moveCoroutine = null;
    }

    /// <summary>
    /// EaseOutBack 計算関数
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = overshootAmount;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }
}