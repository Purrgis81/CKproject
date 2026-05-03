using UnityEngine;

public class Overlay : MonoBehaviour
{
    [Header("검은 오버레이")]
    [SerializeField] private GameObject overlay;

    private void Start()
    {
        if (overlay != null)
        {
            overlay.SetActive(false);
        }
    }
    private void OnMouseEnter()
    {
        if (overlay != null)
        {
            overlay.SetActive(true);
        }
    }
    private void OnMouseExit()
    {
        if(overlay != null)
        {
            overlay.SetActive(false);
        }
    }
}
