using UnityEngine;

public class AggresionField : MonoBehaviour, IField
{
    [SerializeField] private GameObject fieldObject;
    public void OnSetField()
    {
        Debug.Log("AggresionフィールドのOnSetFieldが呼び出されました");
        fieldObject.SetActive(true);
    }

    public void OnEndField()
    {
        Debug.Log("AggresionフィールドのOnEndFieldが呼び出されました");
        fieldObject.SetActive(false);
    }

    public void OnInitializeField()
    {
        Debug.Log("AggresionフィールドのOnInitializeFieldが呼び出されました");
        fieldObject.SetActive(false);

    }


}
