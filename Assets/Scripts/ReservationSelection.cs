using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class ReservationData
{
    public string time, vehicleNumber, vehicleModel, customer, supervisor, dealer;

}
public class ReservationSelection : Singleton<ReservationSelection>
{
    public RectTransform content;
    public GameObject itemPrefab;
    public Button prevButton;
    public Button confirmButton;

    private List<ReservationData> currentList;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Init(List<ReservationData> list)
    {
        currentList = list;
        foreach (Transform t in content) Destroy(t.gameObject);

        foreach (var r in list)
        {
            var go = Instantiate(itemPrefab, content);
            go.GetComponentInChildren<Text>().text = $"{r.time} {r.customer}";
            go.GetComponent<Toggle>().group = content.GetComponent<ToggleGroup>();
        }

        prevButton.onClick.RemoveAllListeners();
        prevButton.onClick.AddListener(() => CustomerUIManager.Instance.reservationInputScreen.SetActive(true));
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);
    }

    private void OnConfirm()
    {
        StartCoroutine(ShowCompleteThenMain());
    }

    private IEnumerator ShowCompleteThenMain()
    {
        CustomerUIManager.Instance.completeScreen.SetActive(true);
        yield return new WaitForSeconds(10f);
        CustomerUIManager.Instance.ShowMain();
    }
}
