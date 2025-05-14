using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StopWatch : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    private bool timerActive;
    public LayerMask stopwatchEnder;  // The layer mask for the stopwatch trigger
    public LayerMask resetLevel;
    public static string finalTime;
    private float fastestTime;
    private string currentSceneKey;

    private void Start()
    {
        timerActive = true;
        elapsedTime = 0f;
        UpdateTimer();

        // Get the scene name as a unique key for each level
        string currentSceneName = SceneManager.GetActiveScene().name;
        currentSceneKey = "FastestTime_" + currentSceneName;

        // Load the fastest time for the current level
        fastestTime = PlayerPrefs.GetFloat(currentSceneKey, float.MaxValue);
    }

    private void Update()
    {
        if (timerActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
        timerText.text = time.ToString(@"mm\:ss\:fff");
    }

    // This method is triggered when the player's collider enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered trigger with: " + other.gameObject.name);

        // Ensure the player interacts only with the stopwatch (use LayerMask or Tag check)
        if (((1 << other.gameObject.layer) & stopwatchEnder) != 0)
        {
            Cursor.lockState = CursorLockMode.None;

            timerActive = false;
            SaveTime();
            UpdateFastest();

            // Save and switch scene
            string currentLevel = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("LastLevel", currentLevel);
            PlayerPrefs.Save(); // Ensure it's saved immediately

            sceneManager.Instance.LoadScene(sceneManager.Scene.ScoreBoard);
        }


        if (((1 << other.gameObject.layer) & resetLevel) != 0)
        {
            sceneManager.Instance.RestartScene();
        }
    }

    void SaveTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
        finalTime = time.ToString(@"mm\:ss\:fff");

        // Save the final time for this specific level
        string currentLevel = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("FinalTime_" + currentLevel, finalTime);
    }

    void UpdateFastest()
    {
        if (elapsedTime < fastestTime)
        {
            fastestTime = elapsedTime;
            PlayerPrefs.SetFloat(currentSceneKey, fastestTime);
            PlayerPrefs.Save(); // Ensure the value is saved immediately
        }
    }
}
