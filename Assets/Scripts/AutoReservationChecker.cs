using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class AutoReservationChecker : MonoBehaviour
{
    [Header("센터 코드 (API 문서에 명시된 값)")]
    public string centerCode = "219";

    [Header("테스트용 고객 정보")]
    public string customerName = "홍길동";
    public string customerPhoneNumber = "01012341234";

    // “host” = https://api.encar.com/reservation
    private const string baseUrl = "https://api.encar.com/reservation";
    private const string path = "/external/reservation/sales-agent/reservation/center/";
    private const string bearerToken = "z39s3MkYPHpU4hC7wHD94ovk9jHyDs";

    void Start()
    {
        StartCoroutine(CheckReservation());
    }

    private IEnumerator CheckReservation()
    {
        // 1) 전화번호 숫자만
        string phone = Regex.Replace(customerPhoneNumber, @"\D", "");

        // 2) 요청 바디 JSON
        var body = new
        {
            customerName = customerName,
            customerPhoneNumber = phone
        };
        string json = JsonUtility.ToJson(body);

        // 3) 최종 URL 조립
        string url = baseUrl + path + centerCode;

        // 4) UnityWebRequest 설정
        using var req = new UnityWebRequest(url, "POST");
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", $"Bearer {bearerToken}");

        // 5) 디버그 로그
        Debug.Log($"[AutoChecker] POST {url}");
        Debug.Log($"[AutoChecker] Body: {json}");

        // 6) 전송
        yield return req.SendWebRequest();

        // 7) 결과 출력
        if (req.result != UnityWebRequest.Result.Success)
        {
            // HTTP 코드와 서버가 내려준 본문까지 같이 찍어보기
            Debug.LogError($"[AutoChecker] HTTP {(int)req.responseCode} {req.error}");
            Debug.LogError($"[AutoChecker] 서버 응답: {req.downloadHandler.text}");
            yield break;
        }
        else
        {
            Debug.Log($"[AutoChecker] Response: {req.downloadHandler.text}");
        }
    }
}
