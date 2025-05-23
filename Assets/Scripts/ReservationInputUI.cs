using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

[Serializable]
public class ReservationListWrapper
{
    public List<ReservationData> reservations;
}

[Serializable]
public class TestResponseWrapper
{
    public bool result;
    public List<TestReservation> reservationResponseList;
}

[Serializable]
public class TestReservation
{
    public string useDate;
    public string useStartTime;
    public string manufacturerName;
    public string modelName;
    public string gradeName;
    public string gradeDetailName;
    public string vehicleNo;
    public string customerName;
    public string firmName;
    public string dealerName;
}

/// <summary>
/// 입력 필드 선택, 문자 및 숫자 입력, 삭제, 제출, 예약 조회 기능을 담당합니다.
/// 테스트 버튼으로 로컬 JSON 불러오기도 지원함.
/// </summary>
public class ReservationInputUI : Singleton<ReservationInputUI>
{
    [Header("입력 필드")]
    public TMP_InputField nameInput;    // 문자용
    public TMP_InputField phoneInput;   // 숫자용

    [Header("키보드 컨테이너")]
    public GameObject textKeyboardContainer;
    public GameObject numericKeyboardContainer;

    [Header("네비게이션 버튼")]
    public Button prevButton;
    public Button confirmButton;

    [Header("테스트용 버튼")]
    public Button testButton;

    // 현재 입력 중인 필드
    private TMP_InputField currentField;
    public TMP_InputField CurrentField => currentField;

    protected override void Awake()
    {
        base.Awake();
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
    }

    void Start()
    {
        // 뒤로
        prevButton.onClick.AddListener(() =>
        {
            HideAllKeyboards();
            CustomerUIManager.Instance.ShowMain();
        });
        // 확인(API 호출)
        confirmButton.onClick.AddListener(OnConfirm);
        // 테스트 버튼(로컬 JSON)
        testButton.onClick.AddListener(OnTestLoad);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var sel = EventSystem.current.currentSelectedGameObject;
            if (sel == nameInput.gameObject)
            {
                currentField = nameInput;
                HangulComposer.Instance.ResetAll();
                currentField.text = string.Empty;
                textKeyboardContainer.SetActive(true);
                numericKeyboardContainer.SetActive(false);
            }
            else if (sel == phoneInput.gameObject)
            {
                currentField = phoneInput;
                HangulComposer.Instance.ResetAll();
                currentField.text = string.Empty;
                textKeyboardContainer.SetActive(false);
                numericKeyboardContainer.SetActive(true);
            }
        }
    }

    public void AddLetter(string letter)
    {
        if (currentField == phoneInput)
        {
            currentField.text += letter;
        }
        else if (currentField != null && !string.IsNullOrEmpty(letter))
        {
            currentField.text = HangulComposer.Instance.Add(letter[0]);
        }
    }

    public void AddNumber(string number)
    {
        if (currentField != null && !string.IsNullOrEmpty(number))
            currentField.text += number;
    }

    public void DeleteLetter()
    {
        if (currentField == phoneInput)
        {
            if (!string.IsNullOrEmpty(currentField.text))
                currentField.text = currentField.text.Remove(currentField.text.Length - 1);
        }
        else if (currentField != null)
        {
            currentField.text = HangulComposer.Instance.DeleteLastJamo();
        }
    }

    public void SubmitWord()
    {
        if (currentField != null)
            HideAllKeyboards();
    }

    public void BreakComposition()
    {
        if (currentField != null)
            currentField.text = HangulComposer.Instance.Add(' ');
    }

    private void OnConfirm()
    {
        // 전화번호 포맷
        var digits = new string(phoneInput.text.Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && digits.StartsWith("010"))
            phoneInput.text = $"{digits.Substring(0, 3)}-{digits.Substring(3, 4)}-{digits.Substring(7, 4)}";

        StartCoroutine(CheckReservations());
    }

    private IEnumerator CheckReservations()
    {
        string url = $"https://api.example.com/reservations/check?name={nameInput.text}&phone={phoneInput.text}";
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"예약 확인 실패: {req.error}");
                yield break;
            }
            var wrapper = JsonUtility.FromJson<ReservationListWrapper>(req.downloadHandler.text);
            ProcessReservations(wrapper.reservations);
        }
    }

    private void OnTestLoad()
    {
        StartCoroutine(LoadLocalTestData());
    }

    private IEnumerator LoadLocalTestData()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "api_1_sample.txt");
        using (var req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("로컬 테스트 데이터 로드 실패: " + req.error);
                yield break;
            }
            var json = req.downloadHandler.text;
            var wrapper = JsonUtility.FromJson<TestResponseWrapper>(json);

            var list = new List<ReservationData>();
            foreach (var r in wrapper.reservationResponseList)
            {
                list.Add(new ReservationData
                {
                    time = r.useStartTime,
                    vehicleNumber = r.vehicleNo,
                    vehicleModel = $"{r.manufacturerName} {r.modelName}",
                    customer = r.customerName,
                    supervisor = r.firmName,
                    dealer = r.dealerName
                });
            }
            ProcessReservations(list);
        }
    }

    private void ProcessReservations(List<ReservationData> reservations)
    {
        var futureList = reservations.FindAll(r =>
            DateTime.ParseExact(r.time, "HH:mm", CultureInfo.InvariantCulture) >= DateTime.Now);

        if (futureList.Count == 0)
            CustomerUIManager.Instance.ShowNone();
        else if (futureList.Count == 1)
            StartCoroutine(ShowCompleteRoutine());
        else
        {
            futureList.Sort((a, b) =>
                Math.Abs((DateTime.Parse(a.time) - DateTime.Now).Ticks)
                    .CompareTo(Math.Abs((DateTime.Parse(b.time) - DateTime.Now).Ticks)));
            var subset = futureList.GetRange(0, Mathf.Min(5, futureList.Count));
            ReservationSelection.Instance.Init(subset);
            CustomerUIManager.Instance.ShowSelection();
        }
    }

    private IEnumerator ShowCompleteRoutine()
    {
        CustomerUIManager.Instance.ShowComplete();
        FindObjectOfType<CustomerStateSocketClient>()
            .SendState(CustomerState.STATE_RESERVATION_CONFIRMED);
        yield return new WaitForSeconds(10f);
        CustomerUIManager.Instance.ShowMain();
    }

    private void HideAllKeyboards()
    {
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
    }
}
