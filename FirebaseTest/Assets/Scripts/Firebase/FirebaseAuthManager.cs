using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

using TMPro;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase
    public DependencyStatus DependencyStatus;
    public FirebaseAuth Auth;
    public FirebaseUser User;

    // Login
    public TMP_InputField EmailLoginField;
    public TMP_InputField PasswordLoginField;

    // Registration
    public TMP_InputField NameRegisterField;
    public TMP_InputField EmailRegisterField;
    public TMP_InputField PasswordRegisterField;
    public TMP_InputField ConfirmPasswordRegisterField;

    // Database
    private DatabaseReference _database;
    private string _currentUserID;
    

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            DependencyStatus = task.Result;

            if(DependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all firebase dependencies: " + DependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        //_currentUserID = SystemInfo.deviceUniqueIdentifier;
        
        _database = FirebaseDatabase.DefaultInstance.RootReference;

        // Set the default instance object
        Auth = FirebaseAuth.DefaultInstance;

        //FirebaseStorage storageInstance = FirebaseStorage.DefaultInstance;

        Auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if(Auth.CurrentUser != User)
        {
            bool signedIn = (User != Auth.CurrentUser && Auth.CurrentUser != null);

            if(!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
            }

            User = Auth.CurrentUser;

            if(signedIn)
            {
                Debug.Log("Signed in " + User.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(EmailLoginField.text, PasswordLoginField.text));
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(NameRegisterField.text, EmailRegisterField.text, PasswordRegisterField.text, ConfirmPasswordRegisterField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = Auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException; AuthError authError = (AuthError)firebaseException.ErrorCode;
            string failedMessage = "Login Failed! Because ";
            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid"; 
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password"; 
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Eamil is missing";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing";
                    break;
                default:
                    failedMessage += "Login Failed";
                    break;
            }

            Debug.Log(failedMessage);
        }
        else
        {
            User = loginTask.Result.User;
            Debug.LogFormat("{0} You are Succesfully Logged In", User.DisplayName);
            _currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

            // ShowDAta

            StartCoroutine(ShowUserData());
            
        }
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (name == "")
        {
            Debug.LogError("User Name is empty");
        }
        else if (email == "")
        {
            Debug.LogError("email is empty");
        }
        else if (PasswordRegisterField.text != ConfirmPasswordRegisterField.text)
        {
            Debug.LogError("Password doesn't match");
        }
        else
        {
            var registerTask = Auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Registration Failed! Because ";
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is Invalid";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is Missing";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is Missing";
                        break;
                    default:
                        failedMessage = "Registration Failed";
                        break;
                }

                Debug.Log(failedMessage);
            }
            else
            {
                // Get The User After Registration Success
                User = registerTask.Result.User;
                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = User.UpdateUserProfileAsync(userProfile);
                yield return new WaitUntil(() => updateProfileTask.IsCompleted);
                if (updateProfileTask.Exception != null)
                {
                    // Delete the user if user update failed
                    User.DeleteAsync();
                    Debug.LogError(updateProfileTask.Exception);
                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException; AuthError authError = (AuthError)firebaseException.ErrorCode;
                    string failedMessage = "Profile update Failed! Becuase ";
                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email Is Missing";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is Missing";
                            break;
                        default:
                            failedMessage = "Profile update Failed";
                            break;
                    }

                    Debug.Log(failedMessage);
                }
                else
                {
                    Debug.Log("Registration Successful, Welcome " + User.DisplayName);
                    _currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                    CreateNewUser(name);
                    UIManager.Instance.OpenLoginPanel();
                }
            }
        }
    }

    private void CreateNewUser(string name)
    {
        System.Random rand = new System.Random();
        _database.Child("users").Child(_currentUserID).Child("name").SetValueAsync(name);
        _database.Child("users").Child(_currentUserID).Child("score").SetValueAsync(rand.Next(0, 500 + 1));
        _database.Child("users").Child(_currentUserID).Child("health").SetValueAsync(rand.Next(1, 100 + 1));
        Debug.Log("New User Created: " + name);
    }

    private IEnumerator ShowUserData()
    {
        System.Threading.Tasks.Task<string> nameTask;
        nameTask = _database.Child("users").Child(_currentUserID).Child("name").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log(snapshot.Value.ToString());
                string name = snapshot.Value.ToString();
                System.Random rand = new System.Random();

                return name;
            }
            return null;
        });

        System.Threading.Tasks.Task<string> scoreTask;
        scoreTask = _database.Child("users").Child(_currentUserID).Child("score").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log(snapshot.Value);
                string score = snapshot.Value.ToString();
                return score;
            }
            return null;
        });

        System.Threading.Tasks.Task<string> healthTask;
        healthTask = _database.Child("users").Child(_currentUserID).Child("health").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("error");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log(snapshot.Value);
                string health = snapshot.Value.ToString();
                return health;
            }
            return null;
        });


        yield return new WaitUntil(() => nameTask.IsCompleted);
        string userName = nameTask.Result;

        yield return new WaitUntil(() => scoreTask.IsCompleted);
        int userScore = int.Parse(scoreTask.Result);

        yield return new WaitUntil(() => healthTask.IsCompleted);
        int userHealth = int.Parse(healthTask.Result);

        UserData recievedUserData = new UserData(userName, userScore, userHealth);
        UIManager.Instance.OpenStatsPanel(recievedUserData);
    }
}
