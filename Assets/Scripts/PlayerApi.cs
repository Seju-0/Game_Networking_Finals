using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Fake backend with API-style methods:
/// POST /api/player/register
/// POST /api/player/login
/// GET  /api/player
/// PUT  /api/player
/// PUT  /api/player/account
/// DELETE /api/delete/:playerId
/// 
/// Data is stored in:
/// - PlayerPrefs  (per user json + CurrentUser)
/// - users_db.json in Application.persistentDataPath
/// </summary>
public class PlayerApi : MonoBehaviour
{

    public static PlayerApi Instance { get; private set; }

    // ---------- Request / Response DTOs ----------

    [Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    public class UpdatePlayerRequest
    {
        public string username; // optional, if empty we use CurrentUser
        public int wins;
        public int losses;
    }

    [Serializable]
    public class UpdateAccountRequest
    {
        public string username;   // optional, if empty we use CurrentUser
        public string newEmail;
        public string newPassword;
    }

    public class ApiResponse<T>
    {
        public bool success;
        public string message;
        public T data;

        public ApiResponse(bool success, string message, T data = default)
        {
            this.success = success;
            this.message = message;
            this.data = data;
        }
    }

    // ---------- Internal DB ----------

    [Serializable]
    public class UserListWrapper
    {
        public List<UserData> users = new List<UserData>();
    }

    private Dictionary<string, UserData> users = new Dictionary<string, UserData>();
    private string dbPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            SaveDatabase();
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        dbPath = Path.Combine(Application.persistentDataPath, "users_db.json");
        LoadDatabase();
    }

    // ---------- API: POST /api/player/register ----------

    public ApiResponse<UserData> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.username) ||
            string.IsNullOrWhiteSpace(req.email) ||
            string.IsNullOrWhiteSpace(req.password))
        {
            return new ApiResponse<UserData>(false, "All fields are required.");
        }

        // username already exists
        if (users.ContainsKey(req.username))
        {
            return new ApiResponse<UserData>(false, "Username already exists.");
        }

        // create new player
        UserData newUser = new UserData
        {
            Username = req.username,
            Email = req.email,
            Password = req.password,
            Wins = 0,
            Losses = 0,
            AccountCreationDate = DateTime.Now.ToString(),
            LastLoggedIn = DateTime.Now.ToString()
        };

        users[req.username] = newUser;   // Add to database  
        SaveUserToPlayerPrefs(newUser);
        SaveDatabase();

        // Set current user so your UI knows who logged in
        PlayerPrefs.SetString("CurrentUser", newUser.Username);
        PlayerPrefs.Save();

        return new ApiResponse<UserData>(true, "Player registered.", newUser);
    }

    // ---------- API: POST /api/player/login ----------

    public ApiResponse<UserData> Login(LoginRequest req)
    {
        if (!users.ContainsKey(req.username))
        {
            return new ApiResponse<UserData>(false, "Account not found.");
        }

        UserData user = users[req.username];

        if (user.Password != req.password)
        {
            return new ApiResponse<UserData>(false, "Incorrect password.");
        }

        // Update last login
        user.LastLoggedIn = DateTime.Now.ToString();
        users[req.username] = user;
        SaveUserToPlayerPrefs(user);
        SaveDatabase();

        PlayerPrefs.SetString("CurrentUser", req.username);
        PlayerPrefs.Save();

        return new ApiResponse<UserData>(true, "Login successful.", user);
    }

    // ---------- API: GET /api/player ----------

    public ApiResponse<UserData> GetPlayer()
    {
        string username = PlayerPrefs.GetString("CurrentUser", "");
        if (string.IsNullOrEmpty(username))
        {
            return new ApiResponse<UserData>(false, "No player logged in.", null);
        }

        if (!users.ContainsKey(username))
        {
            return new ApiResponse<UserData>(false, "Player not found in database.", null);
        }

        return new ApiResponse<UserData>(true, "Player data loaded.", users[username]);
    }

    // ---------- API: PUT /api/player ----------

    public ApiResponse<UserData> UpdatePlayer(UpdatePlayerRequest req)
    {
        string username = string.IsNullOrEmpty(req.username)
            ? PlayerPrefs.GetString("CurrentUser", "")
            : req.username;

        if (string.IsNullOrEmpty(username))
        {
            return new ApiResponse<UserData>(false, "No player specified.", null);
        }

        if (!users.ContainsKey(username))
        {
            return new ApiResponse<UserData>(false, "Player not found.", null);
        }

        UserData user = users[username];
        user.Wins = req.wins;
        user.Losses = req.losses;

        users[username] = user;
        SaveUserToPlayerPrefs(user);
        SaveDatabase();

        return new ApiResponse<UserData>(true, "Player updated.", user);
    }

    // ---------- API: PUT /api/player/account (email + password) ----------

    public ApiResponse<UserData> UpdateAccount(UpdateAccountRequest req)
    {
        string username = string.IsNullOrEmpty(req.username)
            ? PlayerPrefs.GetString("CurrentUser", "")
            : req.username;

        if (string.IsNullOrEmpty(username))
        {
            return new ApiResponse<UserData>(false, "No player specified.", null);
        }

        if (!users.ContainsKey(username))
        {
            return new ApiResponse<UserData>(false, "Player not found.", null);
        }

        if (string.IsNullOrWhiteSpace(req.newEmail) ||
            string.IsNullOrWhiteSpace(req.newPassword))
        {
            return new ApiResponse<UserData>(false, "Email and password are required.", null);
        }

        UserData user = users[username];
        user.Email = req.newEmail;
        user.Password = req.newPassword;

        users[username] = user;
        SaveUserToPlayerPrefs(user);
        SaveDatabase();

        return new ApiResponse<UserData>(true, "Account updated.", user);
    }

    // ---------- API: DELETE /api/delete/:playerId ----------
    // NOTE: here playerId == username
    public ApiResponse<bool> DeletePlayer(string playerId)
    {
        if (!users.ContainsKey(playerId))
        {
            return new ApiResponse<bool>(false, "Player not found.", false);
        }

        users.Remove(playerId);
        SaveDatabase();

        // remove from PlayerPrefs
        string key = playerId + "_Data";
        PlayerPrefs.DeleteKey(key);
        PlayerPrefsUtility.RemoveUserKey(key);

        if (PlayerPrefs.GetString("CurrentUser", "") == playerId)
        {
            PlayerPrefs.DeleteKey("CurrentUser");
        }

        PlayerPrefs.Save();

        return new ApiResponse<bool>(true, "Player deleted.", true);
    }

    // ---------- Helpers ----------

    private void LoadDatabase()
    {
        users.Clear();

        if (!File.Exists(dbPath))
        {
            // Create empty DB file
            File.WriteAllText(dbPath, JsonUtility.ToJson(new UserListWrapper(), true));
            return;
        }

        string json = File.ReadAllText(dbPath);
        if (string.IsNullOrEmpty(json))
            return;

        UserListWrapper wrapper = JsonUtility.FromJson<UserListWrapper>(json);
        if (wrapper != null && wrapper.users != null)
        {
            foreach (var u in wrapper.users)
            {
                if (!string.IsNullOrEmpty(u.Username))
                    users[u.Username] = u;
            }
        }
    }

    private void SaveDatabase()
    {
        UserListWrapper wrapper = new UserListWrapper
        {
            users = new List<UserData>(users.Values)
        };

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(dbPath, json);
    }

    private void SaveUserToPlayerPrefs(UserData user)
    {
        string key = user.Username + "_Data";
        string json = JsonUtility.ToJson(user);
        PlayerPrefs.SetString(key, json);
        PlayerPrefsUtility.AddUserKey(key);
        PlayerPrefs.Save();
    }

    public List<UserData> GetAllUsers()
    {
        // Return a copy of the current in-memory users
        return new List<UserData>(users.Values);
    }
}
