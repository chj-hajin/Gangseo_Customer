// 일자 표시하는 스크립트
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using TMPro;


public class DateTimeDisplayCustomer : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;   // Inspector에서 할당
    [SerializeField] private TMP_Text dayOfWeekText;  // Inspector에서 할당
    [SerializeField] private TMP_Text ampmText;
    [SerializeField] private TMP_Text timeText;


    void FixedUpdate()
    {
        DateTime now = DateTime.Now;
        dateText.text = now.ToString("yyyy.MM.dd. ");
        dayOfWeekText.text = now.ToString("ddd", CultureInfo.CreateSpecificCulture("ko-KR"));
        ampmText.text = now.ToString("tt", CultureInfo.CreateSpecificCulture("ko-KR"));
        timeText.text = now.ToString("hh:mm");
    }
}
