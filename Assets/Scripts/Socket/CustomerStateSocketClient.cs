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

    private TcpClient client;
    private NetworkStream stream;

    void Awake()
    {
        LoadConfig();
    }

    void Start()
    {
        ConnectToVideo();
    }

    private void LoadConfig()
    {
        // JSON 파일명도 변경
        string configPath = Path.Combine(Application.streamingAssetsPath, "ServerConfig.json");
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

    void ConnectToVideo()
    {
        try
        {
            client = new TcpClient();
            client.Connect(videoServerIP, videoServerPort);
            stream = client.GetStream();
            Debug.Log($"[Socket] Connected to VideoServer {videoServerIP}:{videoServerPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] Connection error: {e.Message}");
        }
    }

    public void SendState(CustomerState state)
    {
        if (stream == null || !client.Connected)
        {
            Debug.LogWarning("[Socket] Not connected, retrying...");
            ConnectToVideo();
            if (stream == null) return;
        }

        try
        {
            string msg = state.ToString();
            byte[] data = Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);
            Debug.Log($"[Socket] Sent state to VideoServer: {msg}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Socket] Send error: {e.Message}");
        }
    }

    void OnDestroy()
    {
        stream?.Close();
        client?.Close();
    }
}
