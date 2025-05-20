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
public class ReservationData
{
    public string time, vehicleNumber, vehicleModel, customer, supervisor, dealer;
}

[Serializable]
public class ReservationListWrapper
{
    public List<ReservationData> reservations;
}

public class ReservationInputUI : Singleton<ReservationInputUI>
{
    [Header("입력 필드")]
    public TMP_InputField nameInput;             // 문자용
    public TMP_InputField phoneInput;            // 숫자용

    [Header("키보드들")]
    public GameObject textKeyboardContainer;     // 문자 키보드 전체
    public GameObject numericKeyboardContainer;  // 숫자 키보드 전체

    [Header("버튼")]
    public Button prevButton;
    public Button confirmButton;

    private TMP_InputField currentField;         // 현재 입력 중인 필드

    protected override void Awake()
    {
        base.Awake();
        // 시작할 때 키보드 둘 다 숨김
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
    }

    void Start()
    {
        // 이전 버튼
        prevButton.onClick.AddListener(() =>
        {
            HideAllKeyboards();
            CustomerUIManager.Instance.ShowMain();
        });
        // 확인 버튼
        confirmButton.onClick.AddListener(OnConfirm);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 클릭된 UI 가져오기
            var sel = EventSystem.current.currentSelectedGameObject;
            if (sel == nameInput.gameObject)
            {
                // 문자 필드 클릭
                currentField = nameInput;
                textKeyboardContainer.SetActive(true);
                numericKeyboardContainer.SetActive(false);
                currentField.text = "";
            }
            else if (sel == phoneInput.gameObject)
            {
                // 숫자 필드 클릭
                currentField = phoneInput;
                textKeyboardContainer.SetActive(false);
                numericKeyboardContainer.SetActive(true);
                currentField.text = "";
            }
        }
    }

    // 키보드 버튼에서 호출
    public void AddLetter(string letter)
    {
        if (currentField != null)
            currentField.text += letter;
    }

    public void DeleteLetter()
    {
        if (currentField != null && currentField.text.Length > 0)
            currentField.text = currentField.text.Remove(currentField.text.Length - 1);
    }

    public void SubmitWord()
    {
        // Enter 기능 필요하면 여기 추가
        HideAllKeyboards();
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
                Debug.LogError("예약 확인 실패: " + req.error);
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
        yield return new WaitForSeconds(10f);
        CustomerUIManager.Instance.ShowMain();
    }

    private void HideAllKeyboards()
    {
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
    }
}
