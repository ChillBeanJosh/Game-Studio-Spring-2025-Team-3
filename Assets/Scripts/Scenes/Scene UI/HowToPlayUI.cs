using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    public Button returnToMainMenu;

    private void Start()
    {
        returnToMainMenu.onClick.AddListener(StartMainMenu);
    }

    void StartMainMenu()
    {
        sceneManager.Instance.LoadMainMenu();
    }
}
