using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button returnToMainMenu;

    //More Buttons for the specified levels
    public Button straightLevel;
    public Button climbingLevel;
    public Button changingLevel;
    public Button roofLevel;
    public Button kayLevel;
    public Button joshLevel;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);

        //More Listeners for the specified levels
        straightLevel.onClick.AddListener(StartStraightLevel);
        climbingLevel.onClick.AddListener(StartClimbLevel);
        changingLevel.onClick.AddListener(StartChangingLevel);
        roofLevel.onClick.AddListener(StartRooflevel);
        kayLevel.onClick.AddListener(StartKayLevel);

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

    void StartRooflevel()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Rooftop_Runs);
    }

    void StartKayLevel()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Railgun);
    }

    void JoshStage()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Fly_High);
    }

}


