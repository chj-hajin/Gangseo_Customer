// CustomerStateSocketClient.cs
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;

public enum CustomerState
{
    STATE_IDLE,
    STATE_RESERVATION_CONFIRMED,
    STATE_WALK_IN
}

public class CustomerStateSocketClient : MonoBehaviour
{
    [Header("Video Server (Remote PC) Settings")]
    [Tooltip("비디오 프로젝트가 실행 중인 PC의 IP")]
    public string videoServerIP = "192.168.0.9";
    [Tooltip("비디오 프로젝트가 리스닝 중인 포트")]
    public int videoServerPort = 9999;

    private TcpClient client;
    private NetworkStream stream;

    void Start()
    {
        ConnectToVideo();
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
            var msg = state.ToString();
            var data = Encoding.UTF8.GetBytes(msg);
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
