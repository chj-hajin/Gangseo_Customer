using System;
using System.Collections.Generic;
using System.Text;

public class HangulComposer : Singleton<HangulComposer>
{
    static readonly char[] CHO = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
    static readonly char[] JUNG = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
    static readonly char[] JONG = { '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

    List<char> completed = new List<char>(); // 완성된 음절
    int cho = -1, jung = -1, jong = 0;        // 현재 조합 중인 음절

    public void Reset()
    {
        completed.Clear();
        cho = jung = -1;
        jong = 0;
    }

    public void Backspace()
    {
        // 1) 현재 조합 중인 음절이 있으면
        if (cho >= 0 || jung >= 0 || jong > 0)
        {
            // 음절 조합 단계 역추적: 여기선 간단하게 리셋 후 재조합 대신 삭제로 처리
            cho = jung = -1;
            jong = 0;
            return;
        }
        // 2) 아니면 완성된 음절 리스트에서 마지막 하나 제거
        if (completed.Count > 0)
            completed.RemoveAt(completed.Count - 1);
    }

    public void Add(char c)
    {
        // 초성 입력
        if (cho < 0)
        {
            int idx = Array.IndexOf(CHO, c);
            if (idx >= 0) { cho = idx; return; }
        }
        // 중성 입력
        if (cho >= 0 && jung < 0)
        {
            int idx = Array.IndexOf(JUNG, c);
            if (idx >= 0) { jung = idx; return; }
        }
        // 종성 입력
        if (cho >= 0 && jung >= 0)
        {
            int idx = Array.IndexOf(JONG, c);
            if (idx > 0) { jong = idx; CommitSyllable(); return; }
        }
        // 조합 불가능 → 지금까지 조합된 음절 완성 처리
        CommitSyllable();
        // 그리고 새 음절로 재시작
        cho = jung = -1;
        jong = 0;
        Add(c);
    }

    private void CommitSyllable()
    {
        if (cho >= 0 && jung >= 0)
        {
            int code = 0xAC00 + (cho * 21 + jung) * 28 + jong;
            completed.Add((char)code);
        }
        else
        {
            if (cho >= 0) completed.Add(CHO[cho]);
            if (jung >= 0) completed.Add(JUNG[jung]);
        }
        cho = jung = -1;
        jong = 0;
    }

    public string GetText()
    {
        var sb = new StringBuilder();
        // 1) 완성된 음절들
        foreach (var ch in completed) sb.Append(ch);
        // 2) 현재 조합 중인 부분
        if (cho >= 0 && jung >= 0)
        {
            int code = 0xAC00 + (cho * 21 + jung) * 28 + jong;
            sb.Append((char)code);
        }
        else
        {
            if (cho >= 0) sb.Append(CHO[cho]);
            if (jung >= 0) sb.Append(JUNG[jung]);
        }
        return sb.ToString();
    }
}
