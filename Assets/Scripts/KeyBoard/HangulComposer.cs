// HangulComposer.cs
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// OS 한글 IME와 동일하게 초성·중성·종성 및 복합 모음/받침을 처리합니다.
/// </summary>
public class HangulComposer
{
    private static readonly char[] LTable = {
        'ㄱ','ㄲ','ㄴ','ㄷ','ㄸ','ㄹ','ㅁ','ㅂ','ㅃ','ㅅ','ㅆ','ㅇ','ㅈ','ㅉ','ㅊ','ㅋ','ㅌ','ㅍ','ㅎ'
    };
    private static readonly char[] VTable = {
        'ㅏ','ㅐ','ㅑ','ㅒ','ㅓ','ㅔ','ㅕ','ㅖ','ㅗ','ㅘ','ㅙ','ㅚ','ㅛ','ㅜ','ㅝ','ㅞ','ㅟ','ㅠ','ㅡ','ㅢ','ㅣ'
    };
    private static readonly char[] TTable = {
        '\0','ㄱ','ㄲ','ㄳ','ㄴ','ㄵ','ㄶ','ㄷ','ㄹ','ㄺ','ㄻ','ㄼ','ㄽ','ㄾ','ㄿ','ㅀ','ㅁ','ㅂ','ㅄ','ㅅ','ㅆ','ㅇ','ㅈ','ㅊ','ㅋ','ㅌ','ㅍ','ㅎ'
    };

    private static readonly Dictionary<char, int> LIndex = InitIndex(LTable);
    private static readonly Dictionary<char, int> VIndex = InitIndex(VTable);
    private static readonly Dictionary<char, int> BasicTIndex = new Dictionary<char, int>
    {
        ['ㄱ'] = 1,
        ['ㄲ'] = 2,
        ['ㄴ'] = 4,
        ['ㄷ'] = 7,
        ['ㄹ'] = 8,
        ['ㅁ'] = 16,
        ['ㅂ'] = 17,
        ['ㅅ'] = 19,
        ['ㅆ'] = 20,
        ['ㅇ'] = 21,
        ['ㅈ'] = 22,
        ['ㅊ'] = 23,
        ['ㅋ'] = 24,
        ['ㅌ'] = 25,
        ['ㅍ'] = 26,
        ['ㅎ'] = 27
    };
    private static readonly Dictionary<(int, char), int> CompositeT = new Dictionary<(int, char), int>
    {
        [(1, 'ㅅ')] = 3,
        [(4, 'ㅈ')] = 5,
        [(4, 'ㅎ')] = 6,
        [(8, 'ㄱ')] = 9,
        [(8, 'ㅁ')] = 10,
        [(8, 'ㅂ')] = 11,
        [(8, 'ㅅ')] = 12,
        [(8, 'ㅌ')] = 13,
        [(8, 'ㅍ')] = 14,
        [(8, 'ㅎ')] = 15,
        [(17, 'ㅅ')] = 18
    };
    // 복합 모음: 키와 값 쌍 중괄호 형태로 초기화
    private static readonly Dictionary<(char, char), char> CompositeV = new Dictionary<(char, char), char>
    {
        { ('ㅗ','ㅏ'), 'ㅘ' },
        { ('ㅗ','ㅐ'), 'ㅙ' },
        { ('ㅗ','ㅣ'), 'ㅚ' },
        { ('ㅜ','ㅓ'), 'ㅝ' },
        { ('ㅜ','ㅔ'), 'ㅞ' },
        { ('ㅜ','ㅣ'), 'ㅟ' },
        { ('ㅡ','ㅣ'), 'ㅢ' },
        { ('ㅏ','ㅣ'), 'ㅐ' },
        { ('ㅓ','ㅣ'), 'ㅔ' },
        { ('ㅑ','ㅣ'), 'ㅒ' },
        { ('ㅕ','ㅣ'), 'ㅖ' }
    };

    private List<char> history = new List<char>();
    private static HangulComposer _inst;
    public static HangulComposer Instance => _inst ??= new HangulComposer();
    private HangulComposer() { }

    private static Dictionary<char, int> InitIndex(char[] table)
    {
        var dict = new Dictionary<char, int>();
        for (int i = 0; i < table.Length; i++) dict[table[i]] = i;
        return dict;
    }

    /// <summary>입력 초기화</summary>
    public void ResetAll() => history.Clear();

    /// <summary>자모 하나 추가 후 합성 결과 반환</summary>
    public string Add(char jamo)
    {
        history.Add(jamo);
        return Compose();
    }

    /// <summary>마지막 자모 삭제 후 합성 결과 반환</summary>
    public string DeleteLastJamo()
    {
        if (history.Count > 0) history.RemoveAt(history.Count - 1);
        return Compose();
    }

    /// <summary>지금까지 입력된 자모를 모두 조합하여 결과 문자열 반환</summary>
    public string Compose()
    {
        var sb = new StringBuilder();
        int i = 0, n = history.Count;
        while (i < n)
        {
            char c0 = history[i];
            if (LIndex.TryGetValue(c0, out int L) && i + 1 < n && VIndex.TryGetValue(history[i + 1], out int V0))
            {
                int V = V0, vLen = 1;
                // 복합 모음 검사
                if (i + 2 < n && CompositeV.TryGetValue((history[i + 1], history[i + 2]), out char cv))
                {
                    V = VIndex[cv]; vLen = 2;
                }
                int T = 0, tLen = 0;
                int next = i + 1 + vLen;
                if (next < n)
                {
                    char j1 = history[next];
                    // 복합 받침 검사
                    if (next + 1 < n && BasicTIndex.TryGetValue(j1, out int t0) && CompositeT.TryGetValue((t0, history[next + 1]), out int ct))
                    {
                        // 뒤에 모음이 오면 받침 분리
                        if (next + 2 < n && VIndex.ContainsKey(history[next + 2])) { T = t0; tLen = 1; }
                        else { T = ct; tLen = 2; }
                    }
                    // 단일 받침
                    else if (BasicTIndex.TryGetValue(j1, out int baseT))
                    {
                        if (next + 1 < n && VIndex.ContainsKey(history[next + 1])) { T = 0; tLen = 0; }
                        else { T = baseT; tLen = 1; }
                    }
                }
                int code = 0xAC00 + (L * 21 + V) * 28 + T;
                sb.Append((char)code);
                i += 1 + vLen + tLen;
            }
            else
            {
                sb.Append(c0);
                i++;
            }
        }
        return sb.ToString();
    }
}
