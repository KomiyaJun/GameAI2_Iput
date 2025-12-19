using UnityEngine;

public class AggresionField : MonoBehaviour, IField
{
    private BoomerangTargetMove boom;   //フィールド構築時のブロック移動用の参照
    [SerializeField] private GameObject fieldObject;
    [SerializeField] private BatSwarmManager batSwarmManager;
    public void OnSetField()
    {
        Debug.Log("AggresionフィールドのOnSetFieldが呼び出されました");
        fieldObject.SetActive(true);
        if (boom == null)
        {
            BoomerangTargetMove animateBoom = this.GetComponent<BoomerangTargetMove>();
            animateBoom.StartBoomerangMovement();
        }
        batSwarmManager.CallSwarm();
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
