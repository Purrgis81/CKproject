using UnityEngine;
using System.Collections;

public class ShakerBody : MonoBehaviour
{
    [Header("흔들기 설정")]
    public float shakeDuration = 1f;      // 흔드는 시간
    public float shakeAmount = 0.1f;      // 흔드는 강도
    public float shakeFrequency = 30f;    // 흔드는 빈도

    [Header("따르기 설정")]
    public Transform glassPosition;        // 잔 위치
    public GameObject finalDrinkObject;    // 잔에 나타날 음료 오브젝트
    public float pourMoveSpeed = 3f;       // 잔으로 이동하는 속도
    public float pourTiltAngle = -45f;     // 따를 때 기울임 각도

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // 시작할 때 음료는 숨김
        if (finalDrinkObject != null)
            finalDrinkObject.SetActive(false);
    }

    void OnMouseDown()
    {
        // 뚜껑 덮인 상태에서만 클릭 반응
        if (ShakerManager.Instance.currentState != ShakerState.LidClosed) return;

        StartCoroutine(ShakeAndPour());
    }

    IEnumerator ShakeAndPour()
    {
        // === 1단계: 흔들기 ===
        ShakerManager.Instance.ChangeState(ShakerState.Shaking);

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Sin 곡선으로 부드럽게 좌우 흔들기
            float offsetX = Mathf.Sin(elapsed * shakeFrequency) * shakeAmount;
            float offsetY = Mathf.Cos(elapsed * shakeFrequency * 1.3f) * shakeAmount * 0.5f;

            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        transform.position = originalPosition;

        // === 2단계: 잔으로 이동 ===
        ShakerManager.Instance.ChangeState(ShakerState.Pouring);

        Vector3 targetPos = glassPosition.position + new Vector3(0, 1f, 0); // 잔 위쪽

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                pourMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // === 3단계: 기울여서 따르기 ===
        float tiltElapsed = 0f;
        float tiltDuration = 0.3f;

        while (tiltElapsed < tiltDuration)
        {
            tiltElapsed += Time.deltaTime;
            float t = tiltElapsed / tiltDuration;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, pourTiltAngle, t));
            yield return null;
        }

        // === 4단계: 음료 등장! ===
        yield return new WaitForSeconds(0.3f);

        if (finalDrinkObject != null)
            finalDrinkObject.SetActive(true);

        // === 5단계: 완료 상태 ===
        ShakerManager.Instance.ChangeState(ShakerState.Done);

        Debug.Log("음료 완성! 들어간 재료: " + ShakerManager.Instance.addedIngredients.Count + "개");
    }
}