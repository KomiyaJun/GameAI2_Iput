using UnityEngine;

public class SolidField : MonoBehaviour, IField
{
    [SerializeField] private GameObject fieldObject;
    [SerializeField] private Soldier targetSoldier;
    public void OnSetField()
    {
        Debug.Log("SolidフィールドのOnSetFieldが呼び出されました");
        fieldObject.SetActive(true);
        targetSoldier.StartProtect();
    }

    public void OnEndField()
    {
        Debug.Log("SolidフィールドのOnEndFieldが呼び出されました");
        fieldObject.SetActive(false);
        targetSoldier.isProtecting = false;

    }

    public void OnInitializeField()
    {
        Debug.Log("SolidフィールドのOnInitializeFieldが呼び出されました");
        targetSoldier.isProtecting = false;
        fieldObject.SetActive(false);
    }

}
