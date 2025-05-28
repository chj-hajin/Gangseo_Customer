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

[Serializable]
public class ReservationRequestBody
{
    public string centerCode;
    public string name;
    public string phonenumber;
}

public class ReservationInputUI : Singleton<ReservationInputUI>
{
    [Header("입력 필드")]
    public TMP_InputField nameInput;
    public TMP_InputField phoneInput;

    [Header("키보드 컨테이너")]
    public GameObject textKeyboardContainer;
    public GameObject numericKeyboardContainer;

    [Header("네비게이션 버튼")]
    public Button prevButton;
    public Button confirmButton;

    [Header("센터 코드 (POST URL)")]
    public string centerCode = "YOUR_CENTER_CODE";

    private TMP_InputField currentField;

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
            CustomerUIManager.Instance.ShowReservationInput();
        });

        // 확인
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
        if (string.IsNullOrEmpty(name) || phone.Length < 9)
        {
            Debug.LogWarning("이름 또는 전화번호 형식이 잘못되었습니다.");
            return;
        }
        StartCoroutine(PostReservationCheck(name, phone));
    }

    private IEnumerator PostReservationCheck(string name, string phone)
    {
        string url = $"http://175.118.126.63/www_encar/getLIst.cfm";
        var body = new ReservationRequestBody
        {
            centerCode = centerCode,
            name = name,
            phonenumber = phone
        };
        string json = JsonUtility.ToJson(body);

        //Post
        var req = new UnityWebRequest(url, "POST");
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[PostReservationCheck] 실패: {req.error}");
            yield break;
        }

        var wrapper = JsonUtility.FromJson<TestResponseWrapper>(req.downloadHandler.text);

        if (!wrapper.result)
        {
            // false 면 입력창 + NoneReservation 타이틀
            CustomerUIManager.Instance.ShowNoneReservation();
            yield break;
        }


        var list = wrapper.reservationResponseList;
        if (list.Count == 1)
        {
            // 단일
            CustomerUIManager.Instance.ShowComplete(list[0].customerName);
        }
        else
        {
            // 다중
            CustomerUIManager.Instance.ShowSelection();
            ReservationSelection.Instance.Init(list);
        }
    }
}
