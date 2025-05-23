using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

[Serializable]
public class AbsentReceiverConfig
{
    public int listenPort;
    public string absentImagePath;
}

public class AbsentButtonController : MonoBehaviour
{
    [Header("JSON 설정 파일 이름 (StreamingAssets)")]
    [SerializeField] private string configFileName = "AbsentReceiverConfig.json";

    [Header("부재중 루트 오브젝트 (On/Off 토글)")]
    [SerializeField] private GameObject absentRoot;    // ← 루트

    [Header("부재중 이미지 (이미지 로드만)")]
    [SerializeField] private Image absentUIImage;      // ← 자식 Image 컴포넌트

    private int listenPort = 9999;
    private string absentImagePath;

    private TcpListener listener;
    private Thread listenerThread;
    private volatile bool isRunning;
    private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

    void Awake()
    {
        LoadConfig();
    }

    void Start()
    {
        if (absentRoot == null || absentUIImage == null)
        {
            Debug.LogError("[AbsentReceiver] 필수 컴포넌트가 할당되지 않음!");
            enabled = false;
            return;
        }

        // 이미지 로드
        LoadAbsentSprite();

        // 루트 오브젝트는 처음에 숨기기
        absentRoot.SetActive(false);

        // TCP 리스닝 시작
        isRunning = true;
        listenerThread = new Thread(StartListening) { IsBackground = true };
        listenerThread.Start();
    }

    void Update()
    {
        while (actionQueue.TryDequeue(out var act))
            act.Invoke();
    }

    void OnDestroy()
    {
        isRunning = false;
        listener?.Stop();
        listenerThread?.Join();
    }

    private void LoadConfig()
    {
        var path = Path.Combine(Application.streamingAssetsPath, configFileName);
        if (!File.Exists(path))
        {
            Debug.LogError($"[AbsentReceiver] Config 파일이 없습니다: {path}");
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var cfg = JsonUtility.FromJson<AbsentReceiverConfig>(json);
            listenPort = cfg.listenPort;
            absentImagePath = cfg.absentImagePath;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AbsentReceiver] Config 읽기 오류: {e.Message}");
        }
    }

    private void LoadAbsentSprite()
    {
        var fullPath = Path.Combine(Application.streamingAssetsPath, absentImagePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[AbsentReceiver] 이미지 파일이 없습니다: {fullPath}");
            return;
        }

        var bytes = File.ReadAllBytes(fullPath);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
        {
            Debug.LogError("[AbsentReceiver] 텍스처 로드 실패");
            return;
        }

        var spr = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f));
        absentUIImage.sprite = spr;
        absentUIImage.SetNativeSize();
    }

    private void StartListening()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, listenPort);
            listener.Start();

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
                        if (msg.Equals("Absent_on", StringComparison.OrdinalIgnoreCase))
                            actionQueue.Enqueue(() => absentRoot.SetActive(true));   // ← 루트 켜기
                        else if (msg.Equals("Absent_off", StringComparison.OrdinalIgnoreCase))
                            actionQueue.Enqueue(() => absentRoot.SetActive(false));  // ← 루트 끄기
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (isRunning)
                Debug.LogError($"[AbsentReceiver] Listener error: {e.Message}");
        }
        finally
        {
            listener?.Stop();
        }
    }
}
