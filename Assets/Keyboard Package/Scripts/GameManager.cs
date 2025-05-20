using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Reservation UI")]
    public TMP_InputField nameInput;
    public TMP_InputField phoneInput;
    public GameObject textKeyboardContainer;
    public GameObject numericKeyboardContainer;

    private TMP_InputField currentField;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HideKeyboards();
        AddClickListener(nameInput.gameObject, () => ActivateField(nameInput));
        AddClickListener(phoneInput.gameObject, () => ActivateField(phoneInput));
    }

    void AddClickListener(GameObject go, UnityAction callback)
    {
        var trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => callback());
        trigger.triggers.Add(entry);
    }

    void ActivateField(TMP_InputField field)
    {
        currentField = field;
        if (field == nameInput)
        {
            textKeyboardContainer.SetActive(true);
            numericKeyboardContainer.SetActive(false);
        }
        else
        {
            textKeyboardContainer.SetActive(false);
            numericKeyboardContainer.SetActive(true);
        }
        field.ActivateInputField();
        field.text = ""; // 클릭 시 초기화
    }

    public void HideKeyboards()
    {
        textKeyboardContainer.SetActive(false);
        numericKeyboardContainer.SetActive(false);
        currentField = null;
    }

    // ← 추가된 메서드들
    public void AddLetter(string letter)
    {
        if (currentField != null)
            currentField.text += letter;
    }

    public void DeleteLetter()
    {
        if (currentField != null && currentField.text.Length > 0)
            currentField.text = currentField.text.Remove(currentField.text.Length - 1);
    }

    public void SubmitWord()
    {
        // 예: 키보드 닫고 포커스 해제
        HideKeyboards();
    }
}
