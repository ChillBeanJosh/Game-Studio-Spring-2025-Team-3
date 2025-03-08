using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button returnToMainMenu;
    //More Buttons for the specified levels

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }
}
