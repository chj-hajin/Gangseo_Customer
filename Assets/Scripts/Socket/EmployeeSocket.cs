using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

[Serializable]
public class EmployeeServerConfig
{
    public string employeeServerIP;
    public int employeeServerPort;
}

public enum EmployeeReservationState
{
    Reservation_Confirm,
    Reservation_None
}

public class EmployeeSocket : Singleton<EmployeeSocket>
{
    [Header("Employee Server Settings")]
    public string employeeServerIP = "127.0.0.1";
    public int employeeServerPort = 9999;

    private TcpClient client;
    private NetworkStream stream;

    void Awake() => LoadConfig();
    void Start() => ConnectToEmployee();

    private void LoadConfig()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "EmployeeServerConfig.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"[EmployeeSocket] Config not found: {path}");
            return;
        }
        try
        {
            var json = File.ReadAllText(path);
            var cfg = JsonUtility.FromJson<EmployeeServerConfig>(json);
            employeeServerIP = cfg.employeeServerIP;
            employeeServerPort = cfg.employeeServerPort;
            Debug.Log($"[EmployeeSocket] Config loaded: {employeeServerIP}:{employeeServerPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EmployeeSocket] Config load error: {e.Message}");
        }
    }

    private void ConnectToEmployee()
    {
        try
        {
            client = new TcpClient(employeeServerIP, employeeServerPort);
            stream = client.GetStream();
            Debug.Log($"[EmployeeSocket] Connected to {employeeServerIP}:{employeeServerPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EmployeeSocket] Connection error: {e.Message}");
        }
    }

    /// <summary>
    /// 상태 메시지(예약/비예약) 전송
    /// </summary>
    public void SendEmployeeState(EmployeeReservationState state)
    {
        SendEmployeeState(state.ToString());
    }

    /// <summary>
    /// 추가 정보를 포함한 임의 문자열 메시지 전송
    /// </summary>
    public void SendEmployeeState(string msg)
    {
        if (stream == null || client == null || !client.Connected)
        {
            Debug.LogWarning("[EmployeeSocket] Not connected, retrying...");
            ConnectToEmployee();
            if (stream == null) return;
        }
        try
        {
            var data = Encoding.UTF8.GetBytes(msg);
            stream.Write(data, 0, data.Length);
            Debug.Log($"[EmployeeSocket] Sent: {msg}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EmployeeSocket] Send error: {e.Message}");
            stream?.Close();
            client?.Close();
            stream = null;
            client = null;
        }
    }

    void OnDestroy()
    {
        stream?.Close();
        client?.Close();
    }

    /// <summary>
    /// 메시지 보낼 때마다 새 TCP 연결을 맺고 한 번 쓰고 닫음.
    /// </summary>
    public void SendSocketMessage(string msg)
    {
        try
        {
            using (var client = new TcpClient(employeeServerIP, employeeServerPort))
            using (var stream = client.GetStream())
            {
                var data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
            Debug.Log($"[EmployeeSocket] Sent: {msg}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EmployeeSocket] Send error: {e.Message}");
        }
    }
}
