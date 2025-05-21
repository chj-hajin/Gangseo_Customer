using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardButtonController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI containerText;

    public void AddLetter()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLetter(containerText.text);
            //HangulComposer.Instance.Add(containerText.text[0]);
        }


        else
            Debug.Log(containerText.text + " 눌렀음 ");
    }

    public void DeleteLetter()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.DeleteLetter();
        else
            Debug.Log("마지막 글자 지우기");
    }

    public void SubmitWord()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SubmitWord();
        else
            Debug.Log("확인 완료");
    }
}
