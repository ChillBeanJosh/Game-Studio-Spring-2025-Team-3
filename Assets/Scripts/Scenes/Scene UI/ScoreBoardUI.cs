using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardUI : MonoBehaviour
{
    public Button levelSelection;

    private void Start()
    {
        levelSelection.onClick.AddListener(StartLevelSelect);
    }

    void StartLevelSelect()
    {
        sceneManager.Instance.LoadNewGame();
    }
}
