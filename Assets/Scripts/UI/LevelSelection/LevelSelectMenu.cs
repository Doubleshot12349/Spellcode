using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    public string gameplaySceneName = "HexSandbox";

    public void LoadGrassLevel()
    {
        LevelSelectionData.selectedLevel = "Grass";
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadIceLevel()
    {
        LevelSelectionData.selectedLevel = "Ice";
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadRockLevel()
    {
        LevelSelectionData.selectedLevel = "Rock";
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
