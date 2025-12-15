using System.Collections;
using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    [Header("Input Settings")]
    public string guardButton = "Fire2";  // ガード用ボタン
    public string parryButton = "Fire3";  // パリィ用ボタン

    [Header("Parry Settings")]
    public float parryWindow = 1f;      // パリィ受付時間
    public float parryRecovery = 0.8f;    // パリィ失敗時の硬直時間
    public float failPenalty = 1.5f;      // パリィ失敗時の被ダメ倍率

    [Header("Guard Settings")]
    public float damageReduction = 0.5f;  // ガード時のダメージ倍率

    [Header("Visuals")]
    public GameObject shieldVisual;       // 円形オブジェクト
    public SpriteRenderer shieldRenderer; // 色変更用

    [Header("Status (Read Only)")]
    public bool isGuarding = false;
    public bool isParrying = false;
    public bool inRecovery = false;       // 硬直中
    public bool canCounter = false;       // カウンター待機中


    void Awake()
    {
        // 初期設定：シールドを非表示に
        if (shieldVisual != null) shieldVisual.SetActive(false);

        // SpriteRendererの取得
        if (shieldRenderer == null && shieldVisual != null)
            shieldRenderer = shieldVisual.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 硬直中やカウンター待機中は操作を受け付けない
        if (inRecovery || canCounter) return;

        // パリィ入力
        if (Input.GetButtonDown(parryButton))
        {
            StartCoroutine(PerformParry());
        }
        //ガード入力
        else if (Input.GetButton(guardButton) && !isParrying)
        {
            if (!isGuarding) StartGuarding();
        }
        //ガード解除
        else if (isGuarding)
        {
            StopGuarding();
        }
    }

    //ガード処理
    void StartGuarding()
    {
        isGuarding = true;

        if (shieldVisual)
        {
            shieldVisual.SetActive(true);
            if (shieldRenderer) shieldRenderer.color = new Color(0f, 0.5f, 1f, 0.5f);
        }
    }

    void StopGuarding()
    {
        isGuarding = false;

        if (shieldVisual) shieldVisual.SetActive(false);
    }

    //パリィ処理
    IEnumerator PerformParry()
    {
        if (isGuarding) StopGuarding();

        isParrying = true;

        if (shieldVisual)
        {
            shieldVisual.SetActive(true);
            if (shieldRenderer) shieldRenderer.color = Color.yellow;
        }

        // 受付時間待機
        yield return new WaitForSeconds(parryWindow);

        // 成功判定が呼ばれずに時間が過ぎたら失敗
        if (isParrying)
        {
            isParrying = false;
            StartCoroutine(ParryFailed());
        }
    }

    // パリィ失敗
    IEnumerator ParryFailed()
    {
        inRecovery = true; // 操作不能

        if (shieldVisual)
        {
            shieldVisual.SetActive(true);
            if (shieldRenderer) shieldRenderer.color = Color.red;
        }

        Debug.Log("Parry Failed!");
        yield return new WaitForSeconds(parryRecovery);

        // 復帰処理
        inRecovery = false;
        if (shieldVisual) shieldVisual.SetActive(false);
    }

    //成功時
    public void OnParrySuccess()
    {
        isParrying = false;
        canCounter = true;

        if (shieldRenderer) shieldRenderer.color = Color.white;
        Debug.Log("Parry Success! Counter Ready.");

        StartCoroutine(CounterWindow());
    }

    // カウンター受付猶予
    IEnumerator CounterWindow()
    {
        yield return new WaitForSeconds(0.5f); // 0.5秒以内に攻撃ボタン
        if (canCounter)
        {
            canCounter = false;
            // 受付終了したら非表示へ
            if (shieldVisual) shieldVisual.SetActive(false);
        }
    }

    // カウンター攻撃実行時にAttack.csから呼ぶ
    public void ResetAfterCounter()
    {
        canCounter = false;
        if (shieldVisual) shieldVisual.SetActive(false);
    }
}