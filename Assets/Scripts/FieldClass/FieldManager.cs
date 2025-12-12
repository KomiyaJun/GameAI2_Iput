using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> FieldClassList = new List<GameObject>();　　//各フィールドのクラス
    private GameObject nowFieldClass;   //利用するフィールドのクラス

    public enum Field
    {
        Dynamic = 0,    //機動性
        Aggression = 1, //攻撃性
        Solid = 2,      //堅実性

        //例外、エラー
        Err = 99,
    };

    public Field nowField = Field.Err;

    private void Start()
    {
        SetField(nowField);
    }

    private void PlayerActionCount()
    {
        /*
         プレイヤー側でアクション数をカウントしているはず
        そのカウントに応じてnowFieldを変化
        ジャンプが多かったらDynamicなど...
         ボス側の方向によってイベントなりなんなりが出されるはずなのでそこで呼び出す
        処理は後程追記するものとする

        SetField(nowField) とかは多分必須
        設置するフィールドにOnSetFieldとかのメソッドを用意し、開始処理を入れるのもあり
         */
    }

    /// <summary>
    /// フィールドをセッティング
    /// </summary>
    /// <param name="field"></param>
    public void SetField(Field field)
    {
        switch(field)
        {
            case Field.Dynamic:
                SetDynamicField();
                nowFieldClass = FieldClassList[(int)Field.Dynamic];

                IField iField = nowFieldClass.GetComponent<IField>();
                iField.OnSetField();
                break;

            case Field.Aggression:
                SetAggressionField();
                nowFieldClass = FieldClassList[(int)Field.Aggression];

                break;

            case Field.Solid:
                SetSolidField();
                nowFieldClass = FieldClassList[(int)Field.Solid];
                break;

            default:
                field = Field.Err;
                nowFieldClass = null;
                Debug.Log("フィールドがエラー判定を出しています。");
                break;
        }
    }

    private void SetDynamicField()
    {
        Debug.Log("機動性フィールドを構築");
        //アスレチック風フィールドを構築
        //スピードアイテムを設置
    }

    private void SetAggressionField()
    {
        Debug.Log("攻撃性フィールドを構築");
        //雑魚召喚メソッドを呼び出し(全部倒したらプレイヤーにバフ)
        //攻撃速度アイテムを設置
    }

    private void SetSolidField()
    {
        Debug.Log("堅実性フィールドを構築");
        //ボスにshieldを付与(なんらかの攻撃を複数回与えたら壊れ、ボスにデバフ & プレイヤーにバフ)
        //シールドアイテムを設置
    }
}
