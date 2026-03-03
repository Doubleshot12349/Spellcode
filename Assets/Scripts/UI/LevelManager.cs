using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public GameObject grassLevel;
    public GameObject iceLevel;
    public GameObject rockLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetLevel("Grass");
    }

    public void SetLevel(string level)
    {
        // turn everything off
        grassLevel.SetActive(false);
        iceLevel.SetActive(false);
        rockLevel.SetActive(false);

        // Activate level choice
        switch(level)
        {
            case "Grass":
                grassLevel.SetActive(true);
                break;

            case "Ice":
                iceLevel.SetActive(true);
                break;

            case "Rock":
                rockLevel.SetActive(true);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
