using UnityEngine;

public class BikkuriMark : MonoBehaviour
{
    // 表示しておきたい時間（秒）
    float lifeTime = 2.0f;

    void Start()
    {
        // 生成されてから lifeTime 秒後に自分自身を破壊する
        Destroy(gameObject, lifeTime);
    }
}