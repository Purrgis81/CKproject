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
        // glassHolder 없으면 자기 자신으로
        if (glassHolder == null) glassHolder = transform;

        // 시작 시 칵테일 잔 생성
        ChangeGlass(GlassType.Cocktail);
    }

    void Update()
    {
        // 날아가는 중이면 키 입력 무시
        if (!isFlying)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeGlass(GlassType.Cocktail);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeGlass(GlassType.Highball);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeGlass(GlassType.OnTheRocks);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeGlass(GlassType.Hurricane);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeGlass(GlassType.Margarita);
        }
        DetectFlying();
        // 날아가는 중이면 화면 밖 체크
        if (isFlying && currentGlassObject != null)
        {
            CheckIfOutOfScreen();
        }
    }

    public void ChangeGlass(GlassType type)
    {
        currentGlassType = type;

        // ★ 기존 잔 제거
        if (currentGlassObject != null)
        {
            Destroy(currentGlassObject);
        }

        // ★ 새 프리팹 생성
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

            // 잔 제거
            Destroy(currentGlassObject);
            currentGlassObject = null;

            isFlying = false;

            // 잠시 후 다시 생성
            Invoke(nameof(RespawnGlass), 1f);
        }
    }

    void RespawnGlass()
    {
        ChangeGlass(currentGlassType);  // 같은 종류로 다시 생성
        Debug.Log("🥃 잔 재생성!");
    }

    // 외부에서 현재 잔 오브젝트 접근
    public GameObject GetCurrentGlass()
    {
        return currentGlassObject;
    }
    // 잔이 던져졌을 때 외부에서 호출
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

        // 속도가 빠르면 = 날아가는 중
        if (rb.linearVelocity.magnitude > 1f)
        {
            isFlying = true;
        }
    }
}