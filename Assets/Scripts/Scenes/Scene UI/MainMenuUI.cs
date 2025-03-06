using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button play;
    public Button howToPlay;


    void Start()
    {
        play.onClick.AddListener(StartPlay);
        howToPlay.onClick.AddListener(StartTutorial);
    }

    void StartPlay()
    {
        sceneManager.Instance.LoadNewGame();
    }

    void StartTutorial()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.HowToPlay);
    }
}
