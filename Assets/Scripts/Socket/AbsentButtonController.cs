// AbsentReceiver.cs  (Customer PC)
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class AbsentReceiver : MonoBehaviour
{
    [Header("리스닝 포트")]
    [Tooltip("Employee PC가 보낼 메시지를 받을 포트")]
    [SerializeField] private int listenPort = 9999;

    [Header("조작할 오브젝트")]
    [Tooltip("부재중 표시용 오브젝트")]
    [SerializeField] private GameObject absentObject;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning = false;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    void Start()
    {
        if (absentObject != null)
            absentObject.SetActive(false);

        isRunning = true;
        listenerThread = new Thread(StartListening) { IsBackground = true };
        listenerThread.Start();
    }

    void Update()
    {
        while (actionQueue.TryDequeue(out var act))
            act?.Invoke();
    }

    void OnDestroy()
    {
        isRunning = false;
        listener?.Stop();
        listenerThread?.Join();
    }

    private void StartListening()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();
            Debug.Log($"[Customer] Listening on port {listenPort}");

            while (isRunning)
            {
                if (!listener.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }

                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    var buf = new byte[1024];
                    int len;
                    while (isRunning && (len = stream.Read(buf, 0, buf.Length)) > 0)
                    {
                        var msg = Encoding.UTF8.GetString(buf, 0, len).Trim();
                        Debug.Log($"[Customer] Received: {msg}");

                        if (msg == "ABSENT_ON")
                            actionQueue.Enqueue(() => absentObject.SetActive(true));
                        else if (msg == "ABSENT_OFF")
                            actionQueue.Enqueue(() => absentObject.SetActive(false));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Customer] Listener error: {e}");
        }
        finally
        {
            listener?.Stop();
        }
    }
}
