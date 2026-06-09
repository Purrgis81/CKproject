using UnityEngine;
using System;

// 에디터에 "놓아둔 자리"를 목표로, 화면 밖에서 곡선(AnimationCurve)으로 부드럽게 등장/퇴장.
//   - NPC: enterFrom = Down (아래에서 위로)
//   - 영수증: enterFrom = Up (위에서 아래로)
//   - 등장/퇴장 설정을 따로 둠 (방향·거리·시간·곡선 각각).
//   - Arrive()=등장, Leave()=퇴장. 완료되면 onArrived / onLeft 호출.
public class SlideInOut : MonoBehaviour
{
    public enum Dir { Up, Down, Left, Right }
    public enum State { Hidden, Arriving, Present, Leaving }

    [Header("등장 (들어올 때)")]
    public Dir enterFrom = Dir.Down;
    [Tooltip("목표 자리에서 화면 밖까지 거리")]
    public float enterDistance = 7f;
    [Tooltip("등장에 걸리는 시간(초)")]
    public float enterDuration = 0.6f;
    [Tooltip("등장 곡선 (0~1). 끝에서 살짝 튕기게 하려면 1을 넘는 곡선도 OK)")]
    public AnimationCurve enterCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("퇴장 (나갈 때)")]
    public Dir exitTo = Dir.Down;
    [Tooltip("목표 자리에서 화면 밖까지 거리")]
    public float exitDistance = 7f;
    [Tooltip("퇴장에 걸리는 시간(초)")]
    public float exitDuration = 0.5f;
    [Tooltip("퇴장 곡선 (0~1)")]
    public AnimationCurve exitCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("옵션")]
    public bool deactivateWhenHidden = true;

    [Header("상태 (읽기용)")]
    public State state = State.Hidden;

    // 완료 콜백 (매니저가 사용)
    public Action onArrived;
    public Action onLeft;
    public bool IsPresent { get { return state == State.Present; } }

    private Vector3 targetPos;       // 에디터에 놓은 자리 = 목표
    private Vector3 enterStartPos;   // 등장 시작(화면 밖)
    private Vector3 exitEndPos;      // 퇴장 끝(화면 밖)

    // 현재 진행 중인 애니메이션
    private Vector3 animFrom, animTo;
    private float animDuration;
    private AnimationCurve animCurve;
    private float animElapsed;

    void Awake()
    {
        targetPos = transform.position;
        enterStartPos = targetPos + DirToVector(enterFrom) * enterDistance;
        exitEndPos = targetPos + DirToVector(exitTo) * exitDistance;

        transform.position = enterStartPos;  // 시작은 화면 밖
        state = State.Hidden;
    }

    void Update()
    {
        if (state != State.Arriving && state != State.Leaving) return;

        animElapsed += Time.deltaTime;
        float t = (animDuration <= 0f) ? 1f : Mathf.Clamp01(animElapsed / animDuration);
        float eased = (animCurve != null) ? animCurve.Evaluate(t) : t;

        // LerpUnclamped → 곡선이 1을 넘으면 살짝 튕기는 효과도 가능
        transform.position = Vector3.LerpUnclamped(animFrom, animTo, eased);

        if (t >= 1f)
        {
            transform.position = animTo;

            if (state == State.Arriving)
            {
                state = State.Present;
                if (onArrived != null) onArrived.Invoke();
            }
            else // Leaving
            {
                state = State.Hidden;
                if (deactivateWhenHidden) gameObject.SetActive(false);
                if (onLeft != null) onLeft.Invoke();
            }
        }
    }

    // 등장 (화면 밖 → 목표 자리)
    public void Arrive()
    {
        gameObject.SetActive(true);
        transform.position = enterStartPos;
        StartAnim(enterStartPos, targetPos, enterDuration, enterCurve);
        state = State.Arriving;
    }

    // 퇴장 (현재 자리 → 화면 밖)
    public void Leave()
    {
        StartAnim(transform.position, exitEndPos, exitDuration, exitCurve);
        state = State.Leaving;
    }

    void StartAnim(Vector3 from, Vector3 to, float dur, AnimationCurve curve)
    {
        animFrom = from;
        animTo = to;
        animDuration = dur;
        animCurve = curve;
        animElapsed = 0f;
    }

    Vector3 DirToVector(Dir d)
    {
        switch (d)
        {
            case Dir.Up: return Vector3.up;
            case Dir.Down: return Vector3.down;
            case Dir.Left: return Vector3.left;
            case Dir.Right: return Vector3.right;
        }
        return Vector3.down;
    }
}