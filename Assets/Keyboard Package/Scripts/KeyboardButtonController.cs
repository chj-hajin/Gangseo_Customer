using UnityEngine;
using TMPro;

/// <summary>
/// 각 키보드 버튼에 부착하여 클릭 입력을 ReservationInputUI로 전달합니다.
/// </summary>
public class KeyboardButtonController : MonoBehaviour
{
    [Header("버튼에 표시된 자모 또는 텍스트")]
    [SerializeField] private TextMeshProUGUI buttonLabel;

    /// <summary>
    /// 문자(자모) 버튼 클릭 시 호출
    /// </summary>
    public void AddLetter()
    {
        ReservationInputUI.Instance?.AddLetter(buttonLabel.text);
    }

    /// <summary>
    /// 숫자 버튼 클릭 시 호출 (여러 자리 문자열 입력 지원)
    /// </summary>
    public void AddNumber()
    {
        ReservationInputUI.Instance?.AddNumber(buttonLabel.text);
    }

    /// <summary>
    /// 백스페이스 버튼 클릭 시 호출
    /// </summary>
    public void DeleteLetter()
    {
        ReservationInputUI.Instance?.DeleteLetter();
    }

    /// <summary>
    /// 단어 경계(스페이스) 버튼 클릭 시 호출
    /// </summary>
    public void BreakComposition()
    {
        ReservationInputUI.Instance?.BreakComposition();
    }

    /// <summary>
    /// 확인(엔터) 버튼 클릭 시 호출
    /// </summary>
    public void SubmitWord()
    {
        ReservationInputUI.Instance?.SubmitWord();
    }
}
