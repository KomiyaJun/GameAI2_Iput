using UnityEngine;

public class BoardM : MonoBehaviour
{
    // シングルトン（どこからでも GameManager.instance でアクセスできるようにする）
    public static BoardM instance;

    // みんなで使う唯一のブラックボード
    public BlackBoard sharedBoard;

    void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 1. ここでブラックボードを実体化（生成）する
        sharedBoard = new BlackBoard();
    }
}
