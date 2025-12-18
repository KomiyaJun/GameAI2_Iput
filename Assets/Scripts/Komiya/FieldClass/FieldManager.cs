using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> FieldClassList = new List<GameObject>();　　//各フィールドのクラス
    private GameObject nowFieldClass;   //利用するフィールドのクラス
    private IField iField;
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
        //SetField(nowField);
        InitializedFields();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.U)){
            SetField();
        }
    }

    private void InitializedFields()
    {
        foreach (GameObject fieldObj in FieldClassList)
        {
            // nullチェックをしつつ取得を試みる（安全かつ高速）
            if (fieldObj.TryGetComponent<IField>(out IField fieldComponent))
            {
                fieldComponent.OnInitializeField();
            }
            else
            {
                // 必要であれば、IFieldが見つからなかった場合のログを出す
                Debug.LogWarning($"{fieldObj.name} に IField がアタッチされていません");
            }
        }
    }

    public void PlayerSelectAction(int selectAction)
    {
        switch(selectAction)
        {
            case 0:
                nowField = Field.Dynamic;
                break;
            case 1:
                nowField = Field.Aggression;
                break;
            case 2:
                nowField = Field.Solid;
                break;


        }
    }

    /// <summary>
    /// フィールドをセッティング
    /// </summary>
    public void SetField()
    {
        if(iField != null) { 
            iField.OnEndField();
        }
        switch (nowField)
        {
            case Field.Dynamic:
                nowFieldClass = FieldClassList[(int)Field.Dynamic];

                iField = nowFieldClass.GetComponent<IField>();
                iField.OnSetField();
                break;

            case Field.Aggression:
                nowFieldClass = FieldClassList[(int)Field.Aggression];

                iField = nowFieldClass.GetComponent<IField>();
                iField.OnSetField();
                break;

            case Field.Solid:
                nowFieldClass = FieldClassList[(int)Field.Solid];

                iField = nowFieldClass.GetComponent<IField>();
                iField.OnSetField();
                break;

            default:
                nowField = Field.Err;
                nowFieldClass = null;
                Debug.Log("フィールドがエラー判定を出しています。");
                break;
        }
    }

}
