using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class SimpleLeaderboard : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text leaderboardText;   // drag your big Text (TMP) here
    public int maxEntries = 10;

    private struct Entry
    {
        public string name;
        public int wins;
    }

    // call this when you open the panel (or from Open())
    public void BuildLeaderboard()
    {
        var entries = new List<Entry>();

        // 1) REAL PLAYER (from PlayerApi)
        if (PlayerApi.Instance != null)
        {
            var res = PlayerApi.Instance.GetPlayer();  // ApiResponse<UserData>

            // NOTE: use res.data, not res.user
            if (res != null && res.success && res.data != null)
            {
                entries.Add(new Entry
                {
                    name = res.data.Username,
                    wins = res.data.Wins
                });
            }
        }

        // 2) FAKE USERS (you can change names and ranges)
        AddFake(entries, "BRT01", 3, 15);
        AddFake(entries, "LARRIE", 1, 12);
        AddFake(entries, "KIMO", 0, 10);
        AddFake(entries, "ANNA", 2, 9);
        AddFake(entries, "SHY", 5, 18);
        AddFake(entries, "JOSH", 4, 16);
        AddFake(entries, "BOT_01", 0, 8);
        AddFake(entries, "BOT_02", 0, 8);
        AddFake(entries, "BOT_03", 0, 8);

        // 3) Sort by wins and limit to maxEntries
        var sorted = entries
            .OrderByDescending(e => e.wins)
            .Take(maxEntries)
            .ToList();

        // 4) Build text lines
        var sb = new StringBuilder();
        for (int i = 0; i < sorted.Count; i++)
        {
            var e = sorted[i];
            sb.AppendLine($"{i + 1}. {e.name}  -  {e.wins} wins");
        }

        if (leaderboardText != null)
            leaderboardText.text = sb.ToString();
    }

    private void AddFake(List<Entry> list, string name, int minWins, int maxWins)
    {
        int wins = Random.Range(minWins, maxWins + 1);
        list.Add(new Entry { name = name, wins = wins });
    }

    // Called by your button
    public void Open()
    {
        gameObject.SetActive(true);
        BuildLeaderboard();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
