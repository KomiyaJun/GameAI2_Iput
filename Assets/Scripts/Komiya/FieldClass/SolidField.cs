using UnityEngine;

public class SolidField : MonoBehaviour, IField
{
    private BoomerangTargetMove boom;   //フィールド構築時のブロック移動用の参照
    [SerializeField] private GameObject fieldObject;
    [SerializeField] private Soldier targetSoldier;
    public void OnSetField()
    {
        Debug.Log("SolidフィールドのOnSetFieldが呼び出されました");
        fieldObject.SetActive(true);
        if (boom == null)
        {
            BoomerangTargetMove animateBoom = this.GetComponent<BoomerangTargetMove>();
            animateBoom.StartBoomerangMovement();
        }
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
