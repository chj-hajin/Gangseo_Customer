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
    [SerializeField] GameObject[] walkInObjects;

    [Header("None-Walk-in 애니메이션 오브젝트들")]
    [SerializeField] GameObject[] noneWalkInObjects;
    [SerializeField] GameObject dateObject;

    [Header("예약 확인이 안됩니다 문구")]
    [SerializeField] GameObject Idle_Text;
    [SerializeField] GameObject None_Reservation_Text;

    [Header("Complete 화면용 텍스트")]
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

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);

        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_IDLE);
    }

    public void ShowReservationInput()
    {
        HideAll();
        reservationInputScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);
    }

    public void ShowNoneReservationInput()
    {
        HideAll();
        reservationInputScreen.SetActive(true);

        None_Reservation_Text.SetActive(true);
        Idle_Text.SetActive(false);
    }

    public void ShowSelection()
    {
        HideAll();
        selectionScreen.SetActive(true);

    }

    /// <summary>
    /// 단일 예약 확정 → Complete 화면 + Walk-in 애니메이션
    /// </summary>
    public void ShowComplete(string customerName)
    {
        HideAll();
        completeCustomerNameText.text = customerName;
        completeScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);

        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_RESERVATION_CONFIRMED);

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

    public void ShowNoneReservation()
    {
        HideAll();
        noneReservationScreen.SetActive(true);

        Idle_Text.SetActive(true);
        None_Reservation_Text.SetActive(false);

        CustomerStateSocketClient.Instance.SendState(CustomerState.STATE_WALK_IN);

        StartCoroutine(PlayNoneWalkIn());
    }

    private IEnumerator PlayNoneWalkIn()
    {
        dateObject.SetActive(false);
        foreach (var obj in noneWalkInObjects)
        {    
            obj.SetActive(true);
            yield return new WaitForSeconds(5f);
            dateObject.SetActive(true);
            obj?.SetActive(false);
        }
        ShowMain();
    }

    private void HideAll()
    {
        mainScreen.SetActive(false);
        reservationInputScreen.SetActive(false);
        selectionScreen.SetActive(false);
        completeScreen.SetActive(false);
    }
}
