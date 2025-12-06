using UnityEngine;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject leaderboardPanel;

    [Header("Managers")]
    public LeaderboardManager leaderboardManager;

    public void OpenLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        leaderboardManager.LoadLeaderboard();
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
}
