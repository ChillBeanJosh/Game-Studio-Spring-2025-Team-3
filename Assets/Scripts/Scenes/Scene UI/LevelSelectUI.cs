using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button returnToMainMenu;

    //More Buttons for the specified levels
    public Button straightLevel;
    public Button joshLevel;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);

        //More Listeners for the specified levels
        straightLevel.onClick.AddListener(StartStraightLevel);
        joshLevel.onClick.AddListener(JoshStage);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }

    void StartStraightLevel()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Straight_Line);
    }
    void JoshStage()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.JoshLevel);
    }

}


