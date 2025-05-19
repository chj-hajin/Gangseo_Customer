using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public class ReservationData
{
    public string time;
    public string vehicleNumber;
    public string vehicleModel;
    public string customer;
    public string supervisor;
    public string dealer;
}

[Serializable]
public class ReservationListWrapper
{
    public List<ReservationData> reservations;
}

public class ReservationInputUI : MonoBehaviour
{
    public InputField nameInput;
    public InputField phoneInput;
    public GameObject textKeyboard;
    public GameObject numericKeyboard;
    public Button prevButton;
    public Button confirmButton;

    void Start()
    {
        prevButton.onClick.AddListener(() => CustomerUIManager.Instance.ShowMain());
        confirmButton.onClick.AddListener(OnConfirm);
        textKeyboard.SetActive(false);
        numericKeyboard.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected == nameInput.gameObject)
                ActivateKeyboard(true);
            else if (selected == phoneInput.gameObject)
                ActivateKeyboard(false);
        }
    }

    private void ActivateKeyboard(bool isText)
    {
        textKeyboard.SetActive(isText);
        numericKeyboard.SetActive(!isText);
    }

    private void OnConfirm()
    {
        StartCoroutine(CheckReservations());
    }

    private IEnumerator CheckReservations()
    {
        // api 우뜨케받는데 어뜨케받는데
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
            {
                CustomerUIManager.Instance.ShowNone();
            }
            else if (futureList.Count == 1)
            {
                yield return StartCoroutine(ShowCompleteThenMain());
            }
            else
            {
                futureList.Sort((a, b) =>
                    Math.Abs((DateTime.Parse(a.time) - DateTime.Now).Ticks)
                        .CompareTo(Math.Abs((DateTime.Parse(b.time) - DateTime.Now).Ticks)));

                // 일단 5개까지만 보여주기 (피그마가 그렇게 되어있음)
                var subset = futureList.GetRange(0, Mathf.Min(5, futureList.Count));
                ReservationSelection.Instance.Init(subset);
                CustomerUIManager.Instance.ShowSelection();
            }
        }
    }

    private IEnumerator ShowCompleteThenMain()
    {
        CustomerUIManager.Instance.ShowComplete();
        yield return new WaitForSeconds(10f);
        CustomerUIManager.Instance.ShowMain();
    }
}
