using UnityEngine;

public class DynamicField : MonoBehaviour, IField
{
    private BoomerangTargetMove boom;

    public void OnSetField()
    {
        if(boom == null)
        {
            BoomerangTargetMove animateBoom =  this.GetComponent<BoomerangTargetMove>();
            animateBoom.StartBoomerangMovement();
        }
        Debug.Log("DynamicフィールドのOnSetFieldが呼び出されました");
    }

    public void OnEndField()
    {
        Debug.Log("DynamicフィールドのOnEndFieldが呼び出されました");
    }

}
