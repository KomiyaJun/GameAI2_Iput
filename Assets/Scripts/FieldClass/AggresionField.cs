using UnityEngine;

public class AggresionField : MonoBehaviour, IField
{
    public void OnSetField()
    {
        Debug.Log("AggresionフィールドのOnSetFieldが呼び出されました");
    }

    public void OnEndField()
    {
        Debug.Log("AggresionフィールドのOnEndFieldが呼び出されました");

    }

}
