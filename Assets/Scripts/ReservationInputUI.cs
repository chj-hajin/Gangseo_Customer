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

/// <summary>
/// 입력 필드 선택, 문자 및 숫자 입력, 삭제, 제출, 예약 조회 기능을 담당합니다.
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
        prevButton.onClick.AddListener(() =>
        {
            HideAllKeyboards();
            CustomerUIManager.Instance.ShowMain();
        });
        confirmButton.onClick.AddListener(OnConfirm);
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

    /// <summary>
    /// 문자(자모) 버튼 클릭 시 호출
    /// 숫자 필드인 경우 단순 추가, 문자 필드인 경우 HangulComposer 사용
    /// </summary>
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

    /// <summary>
    /// 숫자 버튼 클릭 시 호출
    /// 여러 자리 문자열도 한 번에 추가 가능
    /// </summary>
    public void AddNumber(string number)
    {
        if (currentField != null && !string.IsNullOrEmpty(number))
            currentField.text += number;
    }

    /// <summary>
    /// 백스페이스 버튼 클릭 시 호출
    /// 숫자 필드인 경우 마지막 문자 삭제, 문자 필드인 경우 자모 조합기 사용
    /// </summary>
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

    /// <summary>
    /// 확인(엔터) 버튼 클릭 시 호출
    /// 키보드 UI 숨김
    /// </summary>
    public void SubmitWord()
    {
        if (currentField != null)
            HideAllKeyboards();
    }

    /// <summary>
    /// 스페이스(단어 구분) 버튼 클릭 시 호출
    /// 공백 추가
    /// </summary>
    public void BreakComposition()
    {
        if (currentField != null)
        {
            currentField.text = HangulComposer.Instance.Add(' ');
        }
    }

    /// <summary>
    /// 예약 확인 프로세스
    /// </summary>
    private void OnConfirm()
    {
        // 전화번호 자동 포맷
        var digits = new string(phoneInput.text.Where(char.IsDigit).ToArray());
        if (digits.Length == 10 && digits.StartsWith("010"))
            phoneInput.text = $"{digits.Substring(0, 3)}-{digits.Substring(3, 4)}-{digits.Substring(7, 4)}";
        StartCoroutine(CheckReservations());
    }

    private IEnumerator CheckReservations()
    {
        string url = $"https://api.example.com/reservations/check?name={nameInput.text}&phone={phoneInput.text}";
        using (var req = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"예약 확인 실패: {req.error}");
                yield break;
            }
            var wrapper = JsonUtility.FromJson<ReservationListWrapper>(req.downloadHandler.text);
            var futureList = wrapper.reservations.FindAll(r =>
                DateTime.ParseExact(r.time, "HH:mm", CultureInfo.InvariantCulture) >= DateTime.Now);
            if (futureList.Count == 0)
                CustomerUIManager.Instance.ShowNone();
            else if (futureList.Count == 1)
                yield return StartCoroutine(ShowCompleteRoutine());
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
