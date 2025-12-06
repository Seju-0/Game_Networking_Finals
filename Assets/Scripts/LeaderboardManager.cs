using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Leaderboard Parent")]
    public Transform leaderboardParent;     // the Content object of your scroll view

    [Header("Leaderboard Row Template")]
    public GameObject leaderboardRowPrefab; // prefab with 3 TMP_Texts: Rank, Name, Wins

    private List<UserData> sortedUsers = new List<UserData>();

    // Called by the button through LeaderboardUIController
    public void LoadLeaderboard()
    {
        // 1) Clear old rows
        foreach (Transform child in leaderboardParent)
        {
            Destroy(child.gameObject);
        }

        // 2) Get all users from PlayerApi
        if (PlayerApi.Instance == null)
        {
            Debug.LogError("[LB] No PlayerApi instance in scene.");
            return;
        }

        List<UserData> allUsers = PlayerApi.Instance.GetAllUsers();

        if (allUsers == null || allUsers.Count == 0)
        {
            Debug.Log("[LB] No users in DB.");
            return;
        }

        // 3) Sort by Wins (highest first)
        sortedUsers = allUsers.OrderByDescending(u => u.Wins).ToList();
        Debug.Log($"[LB] Building leaderboard with {sortedUsers.Count} users.");

        // 4) Create a row for each user
        for (int i = 0; i < sortedUsers.Count; i++)
        {
            GameObject row = Instantiate(leaderboardRowPrefab, leaderboardParent);
            row.SetActive(true);

            TMP_Text[] cols = row.GetComponentsInChildren<TMP_Text>();

            if (cols.Length < 3)
            {
                Debug.LogWarning($"[LB] Row prefab has only {cols.Length} TMP_Text components.");
                continue;
            }

            UserData u = sortedUsers[i];

            cols[0].text = (i + 1).ToString();   // Rank
            cols[1].text = u.Username;          // Username
            cols[2].text = u.Wins.ToString();   // Wins

            Debug.Log($"[LB] Row {i}: {u.Username}, wins={u.Wins}");
        }

        Debug.Log($"[LB] Total children under parent: {leaderboardParent.childCount}");
    }
}
