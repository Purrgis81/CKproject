using UnityEngine;

public enum GlassType
{
    Cocktail,
    Highball,
    OnTheRocks,
    Hurricane,
    Margarita
}

public class GlassController : MonoBehaviour
{
    public static GlassController Instance;

    [Header("잔 프리팹")]
    public GameObject cocktailPrefab;
    public GameObject highballPrefab;
    public GameObject onTheRocksPrefab;
    public GameObject hurricanePrefab;
    public GameObject margaritaPrefab;

    [Header("현재 잔")]
    public GlassType currentGlassType = GlassType.Cocktail;

    [Header("잔 위치")]
    [Tooltip("잔이 생성될 위치 (이 오브젝트의 자식으로 생성됨)")]
    public Transform glassHolder;

    private GameObject currentGlassObject;  // 현재 생성된 잔
    private bool isFlying = false;  // 날아가는 중인지
    private Camera cam;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (glassHolder == null) glassHolder = transform;

        ChangeGlass(GlassType.Cocktail);
    }

    void Update()
    {
        if (!isFlying)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeGlass(GlassType.Cocktail);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeGlass(GlassType.Highball);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeGlass(GlassType.OnTheRocks);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeGlass(GlassType.Hurricane);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeGlass(GlassType.Margarita);
        }
        DetectFlying();
        if (isFlying && currentGlassObject != null)
        {
            CheckIfOutOfScreen();
        }
    }

    public void ChangeGlass(GlassType type)
    {
        currentGlassType = type;

        if (currentGlassObject != null)
        {
            Destroy(currentGlassObject);
        }

        GameObject prefabToSpawn = GetPrefabByType(type);

        if (prefabToSpawn != null)
        {
            currentGlassObject = Instantiate(prefabToSpawn, glassHolder.position, Quaternion.identity, glassHolder);
            Debug.Log($"🥃 잔 변경: {type}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {type} 프리팹이 연결 안 됨!");
        }
    }

    // ★ 종 누를 때: 지금 선택된 잔 종류 그대로 깨끗하게 다시 생성 (1번으로 안 돌아감!)
    public void ResetGlass()
    {
        isFlying = false;
        ChangeGlass(currentGlassType);
        Debug.Log($"🥃 잔 리셋 (현재 종류 유지: {currentGlassType})");
    }

    GameObject GetPrefabByType(GlassType type)
    {
        switch (type)
        {
            case GlassType.Cocktail: return cocktailPrefab;
            case GlassType.Highball: return highballPrefab;
            case GlassType.OnTheRocks: return onTheRocksPrefab;
            case GlassType.Hurricane: return hurricanePrefab;
            case GlassType.Margarita: return margaritaPrefab;
            default: return null;
        }
    }

    void CheckIfOutOfScreen()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(currentGlassObject.transform.position);

        if (viewportPos.x < -0.2f || viewportPos.x > 1.2f ||
            viewportPos.y < -0.2f || viewportPos.y > 1.2f)
        {
            Debug.Log("🥃 잔이 화면 밖! 재생성!");

            Destroy(currentGlassObject);
            currentGlassObject = null;
            isFlying = false;

            Invoke(nameof(RespawnGlass), 1f);
        }
    }

    void RespawnGlass()
    {
        ChangeGlass(currentGlassType);
        Debug.Log("🥃 잔 재생성!");
    }

    public GameObject GetCurrentGlass()
    {
        return currentGlassObject;
    }

    public void OnGlassThrown()
    {
        isFlying = true;
        Debug.Log("🚀 잔 날아가는 중! 키 입력 막힘");
    }

    void DetectFlying()
    {
        if (currentGlassObject == null) return;

        Rigidbody2D rb = currentGlassObject.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (rb.linearVelocity.magnitude > 1f)
        {
            isFlying = true;
        }
    }
}