using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleStage : MonoBehaviour
{
    public TextMeshProUGUI progressText;

    public void StartGame()
    {
        StartCoroutine(this.StartGameAsync());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.progressText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator StartGameAsync()
    {
        var operation = SceneManager.LoadSceneAsync("Game");

        while(operation.isDone == false)
        {
            this.progressText.text = operation.progress + "%";
            yield return null;
        }
    }
}
