// CustomerUIManager.cs
using UnityEngine;
using System.Collections;

public class CustomerUIManager : Singleton<CustomerUIManager>
{
    [Header("스크린들")]
    public GameObject mainScreen;
    public GameObject reservationInputScreen;
    public GameObject noneScreen;
    public GameObject selectionScreen;
    public GameObject completeScreen;

    [Header("예약 확인 종료되고 이미지 리스트")]
    public GameObject[] walkInObjects;

    [Header("비예약 고객인 것 확인하고 나서의 이미지 리스트")]
    public GameObject[] noneWalkInObjects;

    [Header("IDLE로 돌아가는 타임 아웃 시간")]
    [Tooltip("IDLE로 돌아가는 함수 호출하는거 잊지 말 것")]
    public float idleTimeout = 60f;
    private float lastClickTime;

    protected override void Awake()
    {
        base.Awake();
        ShowMain();
    }

    void Update()
    {
        if (Time.time - lastClickTime > idleTimeout)
            ShowMain();
        if (Input.anyKeyDown)
            lastClickTime = Time.time;
    }

    // IDLE 화면
    public void ShowMain()
    {
        HideAll();
        mainScreen.SetActive(true);
        FindObjectOfType<CustomerStateSocketClient>().SendState(CustomerState.STATE_IDLE);
    }

    // 예약 입력 화면
    public void ShowReservationInput()
    {
        HideAll();
        reservationInputScreen.SetActive(true);
    }

    // 예약 없음 화면
    public void ShowNone()
    {
        HideAll();
        noneScreen.SetActive(true);
        FindObjectOfType<CustomerStateSocketClient>()
          .SendState(CustomerState.STATE_WALK_IN);
    }

    // 예약 선택 화면 (여러개의 데이터가 있을 시)
    public void ShowSelection()
    {
        HideAll();
        selectionScreen.SetActive(true);
    }

    // 예약 완료 화면
    public void ShowComplete()
    {
        HideAll();
        completeScreen.SetActive(true);
    }

    // 회원 예약 확인 버튼
    public void OnWalkInButton()
    {
        HideAll();
        StartCoroutine(PlayWalkIn());
       
    }

    // 비회원 고객 확인 버튼
    public void OnNoneWalkInButton()
    {
        HideAll();
        noneScreen.SetActive(true);
        FindObjectOfType<CustomerStateSocketClient>().SendState(CustomerState.STATE_WALK_IN);
        StartCoroutine(PlayNoneWalkIN());
    }

    // 예약 확인 버튼 클릭 시 Walk-in 애니메이션 재생
    private IEnumerator PlayWalkIn()
    {
        foreach (var obj in walkInObjects)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(5f);
            obj.SetActive(false);
        }
        ShowMain();
    }

    // 비회원 확인 버튼 클릭 시 Walk-in 애니메이션 재생
    private IEnumerator PlayNoneWalkIN()
    {
        foreach (var obj in noneWalkInObjects)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(5f);
            obj.SetActive(false);
        }
        ShowMain();
    }

    // 초기화
    private void HideAll()
    {
        mainScreen.SetActive(false);
        reservationInputScreen.SetActive(false);
        noneScreen.SetActive(false);
        selectionScreen.SetActive(false);
        completeScreen.SetActive(false);
    }
}
