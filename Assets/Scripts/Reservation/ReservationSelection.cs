using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;

public class ReservationSelection : MonoBehaviour
{
    public static ReservationSelection Instance { get; private set; }

    [Header("토글 리스트 부모 (ToggleGroup 포함)")]
    public RectTransform content;
    [Header("아이템 Prefab (Toggle, SelectionListItem 포함)")]
    public GameObject itemPrefab;

    [Header("버튼")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button confirmButton;

    private List<TestReservation> currentList;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 여러 예약이 반환되었을 때 호출
    /// </summary>
    public void Init(List<TestReservation> list)
    {
        currentList = list;

        // 1) ToggleGroup 확보 및 설정
        var group = content.GetComponent<ToggleGroup>();
        if (group == null)
            group = content.gameObject.AddComponent<ToggleGroup>();
        group.allowSwitchOff = false;  // 반드시 하나만 선택

        // 2) 기존 아이템 삭제
        foreach (Transform t in content) Destroy(t.gameObject);

        // 3) 새로운 아이템 생성
        for (int i = 0; i < list.Count; i++)
        {
            var r = list[i];
            var go = Instantiate(itemPrefab, content);

            // 날짜/요일/시간 계산
            DateTime dt = DateTime.ParseExact(r.useDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            string dateStr = r.useDate;
            string weekday = dt.ToString("ddd", new CultureInfo("ko-KR"));
            string timeStr = r.useStartTime;

            // 아이템 세팅 (첫 번째만 true)
            var item = go.GetComponent<SelectionListItem>();
            item.Setup(dateStr, weekday, timeStr, i == 0);

            // ToggleGroup 에 등록
            var tog = go.GetComponent<Toggle>();
            tog.group = group;
        }

        // 4) 버튼 리스너
        prevButton.onClick.RemoveAllListeners();
        prevButton.onClick.AddListener(() =>
        {
            CustomerUIManager.Instance.ShowReservationInput();
        });

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirm);
    }

    private void OnConfirm()
    {
        // 선택된 토글 인덱스 찾기
        int idx = -1;
        for (int i = 0; i < content.childCount; i++)
        {
            var tog = content.GetChild(i).GetComponent<Toggle>();
            if (tog != null && tog.isOn)
            {
                idx = i;
                break;
            }
        }
        if (idx < 0) return;

        // 선택된 예약으로 Complete 화면
        var selected = currentList[idx];
        CustomerUIManager.Instance.ShowComplete(selected.customerName);
    }
}
