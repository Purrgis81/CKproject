using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 시 표시할 별 오브젝트")]
    public GameObject starObject;

    [Header("크기 효과")]
    public float hoverScale = 1.1f;
    public float scaleSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (starObject != null)
            starObject.SetActive(false);
    }

    void Update()
    {
        // 부드러운 크기 변화
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            scaleSpeed * Time.deltaTime
        );
    }

    // UI 마우스 진입
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (starObject != null)
            starObject.SetActive(true);

        targetScale = originalScale * hoverScale;
    }

    // UI 마우스 이탈
    public void OnPointerExit(PointerEventData eventData)
    {
        if (starObject != null)
            starObject.SetActive(false);

        targetScale = originalScale;
    }
}