using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

[Serializable]
public class TestResponseWrapper
{
    public bool result;
    public List<TestReservation> reservationResponseList;
}

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

    private TMP_InputField currentField;

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
            CustomerUIManager.Instance.ShowReservationInput();
        });

        confirmButton.onClick.AddListener(OnConfirm);
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var sel = EventSystem.current.currentSelectedGameObject;
        if (sel == nameInput.gameObject)
        {
            currentField = nameInput;
            HangulComposer.Instance.ResetAll();
            currentField.text = string.Empty;
            textKeyboardContainer.SetActive(true);
            numericKeyboardContainer.SetActive(false);
            nameInput.ActivateInputField();
        }
        else if (sel == phoneInput.gameObject)
        {
            currentField = phoneInput;
            HangulComposer.Instance.ResetAll();
            currentField.text = string.Empty;
            textKeyboardContainer.SetActive(false);
            numericKeyboardContainer.SetActive(true);
            phoneInput.ActivateInputField();
        }
    }

    public void AddLetter(string letter)
    {
        if (currentField == nameInput && !string.IsNullOrEmpty(letter))
            currentField.text = HangulComposer.Instance.Add(letter[0]);
    }

    public void AddNumber(string number)
    {
        if (currentField == phoneInput && !string.IsNullOrEmpty(number))
            currentField.text += number;
    }

    public void DeleteLetter()
    {
        if (currentField == phoneInput)
        {
            if (!string.IsNullOrEmpty(currentField.text))
                currentField.text = currentField.text.Remove(currentField.text.Length - 1);
        }
        else if (currentField == nameInput)
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
        if (currentField == nameInput)
            currentField.text = HangulComposer.Instance.Add(' ');
    }

    private void HideAllKeyboards()
    {
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
    }

    private void OnConfirm()
    {
        string name = nameInput.text.Trim();
        string phone = Regex.Replace(phoneInput.text, @"\D", "");
        if (string.IsNullOrEmpty(name) || phone.Length < 1)
        {
            Debug.LogWarning("[ReservationInputUI] 이름과 전화번호를 입력하세요.");
            return;
        }
        StartCoroutine(PostReservationCheck(name, phone));
    }

    // 
    private IEnumerator PostReservationCheck(string name, string phone)
    {
        const string url = "http://175.118.126.63/www_encar/getList.cfm";

        // 나중에 진짜 api 호출할 때는 여기 부분 바궈야 함  -> customerName, customerPhoneNumber
        // 아니면 body에 담아주거나
        var form = new WWWForm();
        form.AddField("name", name);
        form.AddField("phonenumber", phone);

        using (var req = UnityWebRequest.Post(url, form))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ReservationInputUI] 요청 실패: {req.error}");
                yield break;
            }

            // 1) 응답 원문
            string txt = req.downloadHandler.text;
            Debug.Log($"[ReservationInputUI] 원본 응답 JSON:\n{txt}");

            txt = Regex.Replace(txt, @",\s*}", "}");

            // 3) 파싱 시도
            TestResponseWrapper wrapper;
            try
            {
                wrapper = JsonUtility.FromJson<TestResponseWrapper>(txt);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ReservationInputUI] JSON 파싱 실패: {e.Message}\n→ 예약 없음 처리");
                CustomerUIManager.Instance.ShowNoneReservationInput();
                yield break;
            }

            // 4) result==false 면 바로 “예약 없음 - input에 적절한 값이 없음”
            if (!wrapper.result)
            {
                CustomerUIManager.Instance.ShowNoneReservationInput();
                nameInput.text = string.Empty;
                phoneInput.text = string.Empty;
                yield break;
            }

            // 5) 리스트가 없거나 비어 있을 때도 “예약 없음”
            if (wrapper.reservationResponseList == null || wrapper.reservationResponseList.Count == 0)
            {
                CustomerUIManager.Instance.ShowNoneReservationInput();
                yield break;
            }

            // 6) 단일 vs 다중 분기
            var list = wrapper.reservationResponseList;
            if (list.Count == 1)
                CustomerUIManager.Instance.ShowComplete(list[0].customerName);
            else
            {
                CustomerUIManager.Instance.ShowSelection();
                ReservationSelection.Instance.Init(list);
            }
        }
    }


}
