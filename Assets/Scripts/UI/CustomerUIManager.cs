using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CustomerUIManager : MonoBehaviour
{
    public static CustomerUIManager Instance { get; private set; }

    [Header("스크린")]
    public GameObject mainScreen;
    public GameObject reservationInputScreen;
    public GameObject noneReservationScreen;
    public GameObject selectionScreen;
    public GameObject completeScreen;

    [Header("Walk-in 애니메이션 오브젝트들")]
    [SerializeField] private GameObject[] walkInObjects;

    [Header("None-Walk-in 애니메이션 오브젝트들")]
    [SerializeField] private GameObject[] noneWalkInObjects;
    [SerializeField] private GameObject dateObject;

    [Header("예약 확인이 안됩니다 문구")]
    [SerializeField] private GameObject Idle_Text;
    [SerializeField] private GameObject None_Reservation_Text;

    [Header("Complete 화면용 텍스트 (고객명만)")]
    public TMP_Text completeCustomerNameText;

    private void Awake()
    {
        Instance = this;
        ShowMain();
    }

    public void ShowMain()
    {
        HideAll();
        mainScreen.SetActive(true);

        // 키오스크 IDLE 상태 전송
        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_IDLE);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);
    }

    public void ShowReservationInput()
    {
        HideAll();
        reservationInputScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);
    }

    public void ShowSelection()
    {
        HideAll();
        selectionScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);
    }

    public void ShowNoneReservation()
    {
        HideAll();
        noneReservationScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);

        // 키오스크 Walk-in 상태 전송
        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_WALK_IN);

        // 직원 PC로 비예약 방문 메시지 전송
        EmployeeSocket.Instance.SendSocketMessage("Reservation_None");
        Debug.Log("[CustomerUI] Sent: Reservation_None");

        StartCoroutine(PlayNoneWalkIn());
    }

    /// <summary>
    /// 단일 예약 확정 → Complete 화면 + Walk-in 애니메이션
    /// </summary>
    public void ShowComplete(
        string customerName,
        string useDate,
        string useTime,
        string vehicleNo,
        string modelName
    )
    {
        HideAll();
        completeScreen.SetActive(true);
        completeCustomerNameText.text = customerName;

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);

        // 키오스크 예약 확정 상태 전송
        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_RESERVATION_CONFIRMED);

        // 직원 PC로 예약 확정 + 상세 정보 한 번에 전송
        // 포맷: Reservation_Confirm|useDate|useTime|customerName|vehicleNo|modelName
        string payload = $"Reservation_Confirm|{useDate}|{useTime}|{customerName}|{vehicleNo}|{modelName}";
        EmployeeSocket.Instance.SendSocketMessage(payload);
        Debug.Log($"[CustomerUI] Sent: {payload}");

        StartCoroutine(PlayWalkIn());
    }

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

    private IEnumerator PlayNoneWalkIn()
    {
        dateObject.SetActive(false);
        foreach (var obj in noneWalkInObjects)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(5f);
            dateObject.SetActive(true);
            obj.SetActive(false);
        }
        ShowMain();
    }
    public void ShowNoneReservationInput()
    {
        HideAll();
        reservationInputScreen.SetActive(true);

        None_Reservation_Text.SetActive(true);
        Idle_Text.SetActive(false);
    }

    private void HideAll()
    {
        mainScreen.SetActive(false);
        reservationInputScreen.SetActive(false);
        selectionScreen.SetActive(false);
        completeScreen.SetActive(false);
        noneReservationScreen.SetActive(false);
    }
}
