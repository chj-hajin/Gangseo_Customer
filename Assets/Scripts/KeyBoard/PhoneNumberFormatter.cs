// PhoneNumberFormatter.cs
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class PhoneNumberFormatter : MonoBehaviour
{
    TMP_InputField _input;

    void Awake()
    {
        _input = GetComponent<TMP_InputField>();
        _input.contentType = TMP_InputField.ContentType.Custom;  // 숫자 + 하이픈 가능
        _input.onValueChanged.AddListener(Format);
    }

    void Format(string raw)
    {
        // 1) 숫자만 뽑고 최대 11자리(예: 01012345678)로 자름
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length > 11)
            digits = digits.Substring(0, 11);

        // 2) 자리수에 따라 하이픈 자동 추가
        string formatted;
        if (digits.Length <= 3)
            formatted = digits;
        else if (digits.Length <= 7)
            formatted = $"{digits.Substring(0, 3)}-{digits.Substring(3)}";
        else
            formatted = $"{digits.Substring(0, 3)}-{digits.Substring(3, 4)}-{digits.Substring(7)}";

        // 3) 변경된 텍스트가 다르면 업데이트
        if (formatted != raw)
        {
            _input.text = formatted;
            // 캐럿을 맨 끝으로 보냅니다
            _input.caretPosition = formatted.Length;
        }
        

        // 만약에 +82를 해야하는 일이 생긴다면 이런 코드를 사용해보기
        //string formatted = Regex.Replace(str1, @"(^\+82|^0)", ""); // +82 또는 0 제거
    }

    void OnDestroy()
    {
        _input.onValueChanged.RemoveListener(Format);
    }
}
