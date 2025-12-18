using UnityEngine;

public class DynamicField : MonoBehaviour, IField
{
    private BoomerangTargetMove boom;   //フィールド構築時のブロック移動用の参照
    [SerializeField] private GameObject fieldObject;  
    public void OnSetField()
    {
        fieldObject.SetActive(true);
        if (boom == null)
        {
            BoomerangTargetMove animateBoom =  this.GetComponent<BoomerangTargetMove>();
            animateBoom.StartBoomerangMovement();
        }
        Debug.Log("DynamicフィールドのOnSetFieldが呼び出されました");
    }

    public void OnEndField()
    {
        Debug.Log("DynamicフィールドのOnEndFieldが呼び出されました");
        fieldObject.SetActive(false);
    }

    public void OnInitializeField()
    {
        Debug.Log("DynamicフィールドのOnInitializeFieldが呼び出されました");
        fieldObject.SetActive(false);

    }
}
