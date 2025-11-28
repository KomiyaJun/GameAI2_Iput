using UnityEngine;

public class SearchPlayer : MonoBehaviour
{

    [Header("移動設定")]
    public Transform player;       // 追いかけるプレイヤー
    public float moveSpeed = 3f;   // 移動速度

    [Header("センサー設置設定")]
    public GameObject sensorPrefab;    // 足元に置くセンサーのプレハブ
    public float dropInterval = 0.5f;  // 何秒ごとにセンサーを置くか
    public float sensorOffsetY = -0.2f; // 足元に少しだけ下げて設置

    private float dropTimer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);

        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            DropSensor();
            dropTimer = 0f;
        }
    }

    void DropSensor()
    {
        if (sensorPrefab == null) return;

        Vector3 spawnPos = transform.position + new Vector3(0f, sensorOffsetY, 0f);
        Instantiate(sensorPrefab, spawnPos, Quaternion.identity);
    }
}
