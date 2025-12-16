using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableWeapon : MonoBehaviour
{
    public Vector2 direction;
    public bool hasHit = false;
    public float speed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        // 5秒後に自動消滅
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!hasHit)
            GetComponent<Rigidbody2D>().linearVelocity = direction * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        HitProcess(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HitProcess(collision.gameObject);
    }

    // 共通のヒット処理
    void HitProcess(GameObject target)
    {
        // すでに当たっていたら無視
        if (hasHit) return;

        if (target.tag == "Enemy")
        {
            hasHit = true; // ヒット済みにする

            float damageDir = (direction.x >= 0) ? 1f : -1f;
            target.SendMessage("ApplyDamage", damageDir * 2f);

            Destroy(gameObject); // 弾を消す
        }
        else if (target.tag != "Player" && target.tag != "Bullet")
        {
            // プレイヤーと他の弾以外（壁など）に当たったら消える
            hasHit = true;
            Destroy(gameObject);
        }
    }
}