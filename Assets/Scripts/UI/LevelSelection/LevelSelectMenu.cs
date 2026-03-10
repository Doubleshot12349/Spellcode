using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    

    public void LoadGrassLevel()
    {
        LevelSelectionData.selectedLevel = "Grass";
        
    }

    public void LoadIceLevel()
    {
        LevelSelectionData.selectedLevel = "Ice";
        
    }

    public void LoadRockLevel()
    {
        LevelSelectionData.selectedLevel = "Rock";
        
    }

    public void LoadFireLevel()
    {
        LevelSelectionData.selectedLevel = "Fire";
        
    }

    public void LoadWaterLevel()
    {
        LevelSelectionData.selectedLevel = "Water";

    }

    public void LoadSkyLevel()
    {
        LevelSelectionData.selectedLevel = "Sky";

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
