using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StopWatch : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime;
    private bool timerActive;
    public LayerMask stopwatchEnder;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & stopwatchEnder) != 0)
        {
            timerActive = false;
            SaveTime();
            UpdateFastest();

            // Save the last level name before switching scenes
            string currentLevel = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetString("LastLevel", currentLevel);
            PlayerPrefs.Save(); // Ensure it's saved immediately

            sceneManager.Instance.LoadScene(sceneManager.Scene.ScoreBoard); 
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
