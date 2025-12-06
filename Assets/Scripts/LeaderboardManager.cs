using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Leaderboard Parent")]
    public Transform leaderboardParent;

    [Header("Leaderboard Row Template")]
    public GameObject leaderboardRowPrefab;

    private List<UserData> sortedUsers = new List<UserData>();

    private void Start()
    {
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        // Clear previous rows first
        foreach (Transform child in leaderboardParent)
            Destroy(child.gameObject);

        // Fetch DB list straight from PlayerApi via reflection through saved Users
        var response = PlayerApi.Instance.GetPlayer(); // ensures DB loaded at least once

        // Now load full DB
        var db = LoadAllUsers();
        sortedUsers = db.OrderByDescending(u => u.Wins).ToList();

        DisplayLeaderboard();
    }

    private List<UserData> LoadAllUsers()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "users_db.json");

        if (!System.IO.File.Exists(path))
            return new List<UserData>();

        string json = System.IO.File.ReadAllText(path);
        PlayerApi.UserListWrapper wrapper = JsonUtility.FromJson<PlayerApi.UserListWrapper>(json);

        return wrapper?.users ?? new List<UserData>();
    }

    private void DisplayLeaderboard()
    {
        for (int i = 0; i < sortedUsers.Count; i++)
        {
            GameObject row = Instantiate(leaderboardRowPrefab, leaderboardParent);

            TMP_Text[] cols = row.GetComponentsInChildren<TMP_Text>();

            UserData u = sortedUsers[i];

            cols[0].text = (i + 1).ToString();  // Rank
            cols[1].text = u.Username;         // Username
            cols[2].text = u.Wins.ToString();  // Wins
        }
    }
}
