// InputFieldClickHandler.cs
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// nameInput, phoneInput 에 이 컴포넌트를 붙여 주세요.
public class InputFieldClickHandler : MonoBehaviour, IPointerClickHandler
{
    TMP_InputField _inputField;

    void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //ReservationInputUI.Instance.SetCurrentField(_inputField);
    }
}
