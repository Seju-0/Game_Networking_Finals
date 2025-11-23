using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Login UI")]
    public GameObject loginPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text loginMessage;

    [Header("Register UI")]
    public GameObject registerPanel;
    public TMP_InputField regUsernameInput;
    public TMP_InputField regEmailInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regRepeatPasswordInput;
    public TMP_Text registerMessage;

    [Header("Reset UI")]
    public GameObject resetPanel;
    public TMP_InputField resetEmailInput;
    public TMP_InputField resetPasswordInput;
    public TMP_InputField resetRepeatPasswordInput;
    public TMP_Text resetMessage;

    private void Start()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        resetPanel.SetActive(false);

        loginMessage.text = "";
        registerMessage.text = "";
        resetMessage.text = "";
    }

    // -------------------------
    // LOGIN FUNCTION
    // -------------------------
    public void OnLoginButton()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginMessage.text = "Please enter username and password.";
            return;
        }

        var req = new PlayerApi.LoginRequest
        {
            username = username,
            password = password
        };

        var res = PlayerApi.Instance.Login(req);
        loginMessage.text = res.message;

        if (res.success)
        {
            // Save the logged-in user (username as key in DB)
            PlayerPrefs.SetString("CurrentUser", username);

            // Load next scene
            SceneManager.LoadScene("GameScene");
        }
    }

    // -------------------------
    // REGISTER FUNCTION
    // -------------------------
    public void OnRegisterButton()
    {
        string username = regUsernameInput.text;
        string email = regEmailInput.text;
        string password = regPasswordInput.text;
        string repeatPass = regRepeatPasswordInput.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(repeatPass))
        {
            registerMessage.text = "Please fill in all fields.";
            return;
        }

        if (password != repeatPass)
        {
            registerMessage.text = "Passwords do not match.";
            return;
        }

        var req = new PlayerApi.RegisterRequest
        {
            username = username,
            email = email,
            password = password
        };

        var res = PlayerApi.Instance.Register(req);
        registerMessage.text = res.message;

        // If registered successfully, switch back to login screen
        if (res.success)
        {
            Invoke(nameof(SwitchToLogin), 1.5f);
        }
    }

    // -------------------------
    // RESET EMAIL + PASSWORD
    // -------------------------
    public void OnResetButton()
    {
        string email = resetEmailInput.text;
        string password = resetPasswordInput.text;
        string repeatPass = resetRepeatPasswordInput.text;

        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(repeatPass))
        {
            resetMessage.text = "Please fill all fields.";
            return;
        }

        if (password != repeatPass)
        {
            resetMessage.text = "Passwords do not match.";
            return;
        }

        // Use current logged-in username as key
        string currentUser = PlayerPrefs.GetString("CurrentUser", "");

        if (string.IsNullOrEmpty(currentUser))
        {
            resetMessage.text = "No current user found.";
            return;
        }

        var req = new PlayerApi.UpdateAccountRequest
        {
            username = currentUser,
            newEmail = email,
            newPassword = password
        };

        var res = PlayerApi.Instance.UpdateAccount(req);
        resetMessage.text = res.message;

        if (res.success)
        {
            // username in DB stays the same, we only changed email + pass
            Invoke(nameof(SwitchToLogin), 1.5f);
        }
    }

    // -------------------------
    // DELETE ACCOUNT FUNCTION
    // -------------------------
    public void OnDeleteAccountButton()
    {
        string currentUser = PlayerPrefs.GetString("CurrentUser", "");

        if (string.IsNullOrEmpty(currentUser))
        {
            Debug.LogWarning("No current user saved in PlayerPrefs.");
            loginMessage.text = "No account logged in.";
            return;
        }

        // Call the delete API
        var res = PlayerApi.Instance.DeletePlayer(currentUser);
        loginMessage.text = res.message;

        if (res.success)
        {
            // Clear PlayerPrefs
            PlayerPrefs.DeleteKey("CurrentUser");

            // Reset UI
            usernameInput.text = "";
            passwordInput.text = "";

            loginMessage.text = "Account deleted.";
        }
    }

    // -------------------------
    // UI SWITCHES
    // -------------------------
    public void SwitchToRegister()
    {
        loginPanel.SetActive(false);
        resetPanel.SetActive(false);
        registerPanel.SetActive(true);

        loginMessage.text = "";
        resetMessage.text = "";
    }

    public void SwitchToLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        resetPanel.SetActive(false);

        registerMessage.text = "";
        resetMessage.text = "";
    }

    public void SwitchToReset()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        resetPanel.SetActive(true);

        loginMessage.text = "";
        registerMessage.text = "";
    }
}
