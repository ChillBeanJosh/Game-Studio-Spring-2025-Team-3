using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button returnToMainMenu;

    //More Buttons for the specified levels
    public Button joshLevel;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);

        //More Listeners for the specified levels
        joshLevel.onClick.AddListener(JoshStage);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }

    void JoshStage()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.JoshLevel);
    }

}


