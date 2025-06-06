using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    public static sceneManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    //List of Scenes in the build Index, make sure they are in ORDER.
    public enum Scene
    {
        mainMenu_Assets,
        HowToPlay,
        LevelSelect,
        Straight_Line,
        Climbing_Up,
        Changing_Charges,
        Rooftop_Runs,
        Railgun,
        Fly_High,
        ScoreBoard

    }

    //List of Scene Functions.

    //Used to refrence any specific scene on the build index.
    public void LoadScene(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }


    //Used for the Start Game Button.
    public void LoadNewGame()
    {
        SceneManager.LoadScene(Scene.LevelSelect.ToString());
    }


    //Loads next Scene in build index.
    public void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    //Loads the Main Menu Scene.
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(Scene.mainMenu_Assets.ToString());
    }

    //Restarts the current Scene.
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) 
        {
            RestartScene();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.lockState = CursorLockMode.None;
            LoadMainMenu();
        }
    }
}
