using UnityEngine;
using System.Collections;
public class ShieldScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    public int maxShieldHP = 20;
    public int shieldHP = 20;

    [Header("点滅設定")]
    public float blinkInterval = 0.1f; // 点滅の間隔（秒）
    public int blinkCount = 3;         // 点滅する回数

    [SerializeField] Soldier OnParentSoldier;
    public void OnInitializeShield()
    {
        shieldHP = maxShieldHP;
        spriteRenderer.enabled = false;
    }

    public void OnEndShield()
    {
        shieldHP = maxShieldHP;
        spriteRenderer.enabled = false;
    }

    public void OnStartShield()
    {
        shieldHP = maxShieldHP;
        spriteRenderer.enabled = true;
    }

    void Start()
    {
        // このスクリプトと同じオブジェクトについているSpriteRendererを取得
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("ShieldScript: SpriteRendererが見つかりません。点滅しません。");
        }

        OnInitializeShield();
    }

    // 防御成功時に呼ばれる
    public void OnProtectDamage()
    {
        shieldHP--;
        Debug.Log("シールドHP: "+ shieldHP);
        // SpriteRendererがない場合は何もしない
        if (spriteRenderer == null) return;

        // すでに点滅中なら、一度停止してリセットする
        // (連続で攻撃を防いだ時に、点滅が途切れないようにするため)
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            spriteRenderer.enabled = true; // 念のため表示状態に戻す
        }

        if(OnHPLost() == false){
            // 点滅コルーチンを開始する
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    // 点滅処理を行うコルーチン
    private IEnumerator BlinkRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            // 1. 非表示にする
            spriteRenderer.enabled = false;
            // 指定時間待つ
            yield return new WaitForSeconds(blinkInterval);

            // 2. 表示する
            spriteRenderer.enabled = true;
            // 指定時間待つ
            yield return new WaitForSeconds(blinkInterval);
        }

        // 点滅終了後、コルーチンの参照を空にする
        blinkCoroutine = null;
    }

    private bool OnHPLost()
    {
        if (shieldHP <= 0)
        {
            Debug.Log("シールドがなくなった！");
            OnParentSoldier.isProtecting = false;
            spriteRenderer.enabled = false;
            OnEndShield();

            return true;
        }
        return false;
    }
}