using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button returnToMainMenu;

    //More Buttons for the specified levels
    public Button straightLevel;
    public Button climbingLevel;
    public Button changingLevel;
    public Button joshLevel;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);

        //More Listeners for the specified levels
        straightLevel.onClick.AddListener(StartStraightLevel);
        climbingLevel.onClick.AddListener(StartClimbLevel);
        changingLevel.onClick.AddListener(StartChangingLevel);

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

    void StartClimbLevel()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Climbing_Up);
    }

    void StartChangingLevel()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Changing_Charges);
    }

    void JoshStage()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Fly_High);
    }

}


