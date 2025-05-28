using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SelectionListItem : MonoBehaviour
{
    [Header("날짜/요일/시간")]
    public TMP_Text dateText;
    public TMP_Text weekdayText;
    public TMP_Text timeText;

    [Header("오전/오후 표시")]
    public TMP_Text ampmText;

    public Toggle toggle;

    /// <param name="selectOnStart">첫 항목을 기본 선택하려면 true</param>

    public void Setup(string date, string weekday, string time, bool selectOnStart)
    {
        dateText.text = date;
        weekdayText.text = weekday;
        timeText.text = time;
        toggle.isOn = selectOnStart;

        // "HH:mm" 형식으로 들어온 time 에서 시(hour)만 분리
        if (TimeSpan.TryParse(time, out var ts))
        {
            ampmText.text = ts.Hours < 12 ? "오전" : "오후";
        }
        else
        {
            // 파싱 실패 시 기본값
            ampmText.text = "";
        }
    }

    private void Awake()
    {
        // ToggleGroup 이 붙은 부모 Content 에 자동으로 그룹 추가
        var group = GetComponentInParent<ToggleGroup>();
        toggle.group = group;
    }
}
