using UnityEngine;

// 에디터에 "놓아둔 자리"를 목표로, 화면 밖에서 슬라이드로 등장/퇴장하는 공용 컴포넌트.
//   - NPC: enterFrom = Down (아래에서 위로 올라옴)
//   - 영수증: enterFrom = Up (위에서 아래로 내려옴)
//   - Arrive() 부르면 등장, Leave() 부르면 퇴장.
public class SlideInOut : MonoBehaviour
{
    public enum Dir { Up, Down, Left, Right }
    public enum State { Hidden, Arriving, Present, Leaving }

    [Header("등장 방향 (화면 밖 어디서 들어올지)")]
    public Dir enterFrom = Dir.Down;
    [Tooltip("목표 자리에서 화면 밖까지의 거리 (월드 단위)")]
    public float offscreenDistance = 7f;
    [Tooltip("이동 속도")]
    public float moveSpeed = 12f;
    [Tooltip("이 거리 안이면 '도착'으로 침")]
    public float arriveThreshold = 0.05f;
    [Tooltip("퇴장 후 오브젝트를 꺼둘지")]
    public bool deactivateWhenHidden = true;

    [Header("상태 (읽기용)")]
    public State state = State.Hidden;

    private Vector3 targetPos;     // 에디터에 놓은 자리 = 목표
    private Vector3 offscreenPos;  // 화면 밖 시작/퇴장 위치

    void Awake()
    {
        // 처음 놓인 위치를 "목표"로 기억하고, 화면 밖으로 보냄
        targetPos = transform.position;
        offscreenPos = targetPos + DirToVector(enterFrom) * offscreenDistance;
        transform.position = offscreenPos;
        state = State.Hidden;
    }

    void Update()
    {
        if (state == State.Arriving)
        {
            MoveTo(targetPos);
            if (Reached(targetPos))
            {
                transform.position = targetPos;
                state = State.Present;
            }
        }
        else if (state == State.Leaving)
        {
            MoveTo(offscreenPos);
            if (Reached(offscreenPos))
            {
                transform.position = offscreenPos;
                state = State.Hidden;
                if (deactivateWhenHidden) gameObject.SetActive(false);
            }
        }
    }

    // 등장 (화면 밖 → 목표 자리)
    public void Arrive()
    {
        gameObject.SetActive(true);
        transform.position = offscreenPos;
        state = State.Arriving;
    }

    // 퇴장 (목표 자리 → 화면 밖)
    public void Leave()
    {
        state = State.Leaving;
    }

    void MoveTo(Vector3 p)
    {
        transform.position = Vector3.MoveTowards(transform.position, p, moveSpeed * Time.deltaTime);
    }

    bool Reached(Vector3 p)
    {
        return Vector3.Distance(transform.position, p) <= arriveThreshold;
    }

    Vector3 DirToVector(Dir d)
    {
        switch (d)
        {
            case Dir.Up:    return Vector3.up;
            case Dir.Down:  return Vector3.down;
            case Dir.Left:  return Vector3.left;
            case Dir.Right: return Vector3.right;
        }
        return Vector3.down;
    }
}
