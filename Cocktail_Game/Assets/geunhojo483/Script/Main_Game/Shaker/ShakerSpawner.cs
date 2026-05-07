using UnityEngine;
using System.Collections;

public class ShakerSpawner : MonoBehaviour
{
    public static ShakerSpawner Instance;

    [Header("쉐이커 참조")]
    public ShakerStateMachine shaker;

    [Header("자동 재등장")]
    public bool autoRespawn = true;
    public float respawnDelay = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ ShakerSpawner Instance 생성됨!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 시작 시 shaker 참조 확인
        if (shaker == null)
        {
            Debug.LogError("❌ ShakerSpawner: shaker 참조가 null입니다!");
        }
        else
        {
            Debug.Log($"✅ ShakerSpawner: shaker 연결됨 ({shaker.name})");
        }
    }

    public void OnShakerFinished()
    {
        Debug.Log("🍹 OnShakerFinished 호출됨!");
        Debug.Log($"   - autoRespawn: {autoRespawn}");
        Debug.Log($"   - shaker null? {shaker == null}");

        if (autoRespawn)
        {
            Debug.Log($"   → {respawnDelay}초 후 재등장 시도!");
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            Debug.Log("   → autoRespawn이 꺼져있음!");
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log("⏰ 재등장 시간!");

        if (shaker != null)
        {
            Debug.Log("🔄 shaker.ResetAndReappear() 호출!");
            shaker.ResetAndReappear();
        }
        else
        {
            Debug.LogError("❌ shaker가 null이라 재등장 불가!");
        }
    }
}