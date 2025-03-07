using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetTimes : MonoBehaviour
{
    public Button resetFastestTime;  

    private void Start()
    {
        resetFastestTime.onClick.AddListener(ResetAllFastestTimes);
    }

    private void ResetAllFastestTimes()
    {
        foreach (sceneManager.Scene scene in System.Enum.GetValues(typeof(sceneManager.Scene)))
        {
            string fastestTimeKey = "FastestTime_" + scene.ToString();

            //resets all fastest times for each scene in build index to MaxValue = default.
            PlayerPrefs.SetFloat(fastestTimeKey, float.MaxValue);
        }

        PlayerPrefs.Save();
    }
}
