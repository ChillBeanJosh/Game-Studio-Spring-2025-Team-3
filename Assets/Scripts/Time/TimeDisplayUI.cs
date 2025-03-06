using System;
using TMPro;
using UnityEngine;

public class TimeDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI fastestTimeText;
    public TextMeshProUGUI levelName; // Displays the name of the last played level

    private void Start()
    {
        // Retrieve the last played level
        string lastLevel = PlayerPrefs.GetString("LastLevel", "Unknown Level");

        // Set the level name text
        levelName.text = "Level: " + lastLevel;

        // Retrieve the final time for the last level
        string finalTimeKey = "FinalTime_" + lastLevel;
        string finalTime = PlayerPrefs.GetString(finalTimeKey, "No recorded time");

        // Retrieve the fastest time for the last level
        string fastestTimeKey = "FastestTime_" + lastLevel;
        float fastestTime = PlayerPrefs.GetFloat(fastestTimeKey, float.MaxValue);

        // Display Final Time
        finalTimeText.text = "Final Time: <color=green>" + finalTime + "</color>";

        // Display Fastest Time
        if (fastestTime == float.MaxValue)
        {
            fastestTimeText.text = "No record yet";
        }
        else
        {
            TimeSpan fastestTimeSpan = TimeSpan.FromSeconds(fastestTime);
            fastestTimeText.text = "Fastest Time: <color=red>" + fastestTimeSpan.ToString(@"mm\:ss\:fff") + "</color>";
        }
    }
}
