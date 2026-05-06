using UnityEngine;

public class ShakerLid : MonoBehaviour
{
    [Header("¶Ñ²± À§Ä¡")]
    public Vector3 closedPosition;     // ´ÝÇûÀ» ¶§ À§Ä¡ (¸öÃ¼ À§)
    public float closeSpeed = 8f;      // µ¤´Â ¼Óµµ

    private bool isClosed = false;

    void OnMouseDown()
    {
        // Àç·á ÅõÀÔ ´Ü°è°¡ ¾Æ´Ï¸é ¹«½Ã
        if (ShakerManager.Instance.currentState != ShakerState.AddingIngredients) return;
        if (isClosed) return;

        isClosed = true;
        StartCoroutine(CloseLid());
    }

    System.Collections.IEnumerator CloseLid()
    {
        while (Vector3.Distance(transform.position, closedPosition) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                closedPosition,
                closeSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = closedPosition;

        // »óÅÂ¸¦ "¶Ñ²± µ¤À½"À¸·Î º¯°æ
        ShakerManager.Instance.ChangeState(ShakerState.LidClosed);
    }
}