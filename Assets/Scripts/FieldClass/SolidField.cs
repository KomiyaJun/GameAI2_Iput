using UnityEngine;

public class SolidField : MonoBehaviour, IField
{
    public void OnSetField()
    {
        Debug.Log("SolidフィールドのOnSetFieldが呼び出されました");
    }

    public void OnEndField()
    {
        Debug.Log("SolidフィールドのOnEndFieldが呼び出されました");
    }

}
