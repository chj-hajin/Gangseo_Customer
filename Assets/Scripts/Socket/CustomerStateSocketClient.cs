using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

[Serializable]
public class ServerConfig
{
    public string videoServerIP;
    public int videoServerPort;
}

public enum CustomerState
{
    STATE_IDLE,
    STATE_RESERVATION_CONFIRMED,
    STATE_WALK_IN
}

public class CustomerStateSocketClient : Singleton<CustomerStateSocketClient>
{
    [Header("Video Server (Remote PC) Settings")]
    [Tooltip("StreamingAssets에서 불러오는 비디오 서버 IP")]
    public string videoServerIP = "127.0.0.1";
    [Tooltip("StreamingAssets에서 불러오는 포트")]
    public int videoServerPort = 9999;

    private void Awake()
    {
        LoadConfig();
    }

    void Start()
    {
        // Optional: 첫 연결 테스트용 로그
        Debug.Log($"[Socket] Start() – Server at {videoServerIP}:{videoServerPort}");
    }

    private void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "ServerConfig.json");
        Debug.Log($"[Socket] Config 경로: {configPath}, Exists: {File.Exists(configPath)}");

        if (!File.Exists(configPath))
        {
            Debug.LogError($"[Socket] Config 파일을 찾을 수 없습니다: {configPath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(configPath);
            var cfg = JsonUtility.FromJson<ServerConfig>(json);
            videoServerIP = cfg.videoServerIP;
            videoServerPort = cfg.videoServerPort;
            Debug.Log($"[Socket] Loaded ServerConfig: IP={videoServerIP}, Port={videoServerPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] Config 읽기 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 메시지마다 새 연결 → 전송 → 닫기 방식
    /// </summary>
    public void SendState(CustomerState state)
    {
        Debug.Log($"[Socket] → SendState 호출: {state}");
        try
        {
            using (var tmpClient = new TcpClient())
            {
                tmpClient.NoDelay = true;
                tmpClient.Connect(videoServerIP, videoServerPort);
                Debug.Log($"[Socket] Connected to {videoServerIP}:{videoServerPort}");

                using (var ns = tmpClient.GetStream())
                {
                    // 끝에 개행 문자 붙이면 서버에서 Read 후 Trim() 시에도 잘 분리됩니다.
                    string msg = state.ToString() + "\n";
                    byte[] data = Encoding.UTF8.GetBytes(msg);

                    ns.Write(data, 0, data.Length);
                    ns.Flush();
                    Debug.Log($"[Socket] Sent bytes: {data.Length}");
                }
                // using 블록 종료 시 스트림&소켓 모두 닫힘
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] SendState error: {e.Message}");
        }
    }
}
