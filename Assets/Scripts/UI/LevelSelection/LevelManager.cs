using UnityEngine;
using UnityEngine.Rendering;

public class LevelManager : MonoBehaviour
{

    public GameObject grassLevel;
    public GameObject iceLevel;
    public GameObject rockLevel;
    public GameObject fireLevel;
    public GameObject waterLevel;
    public GameObject skyLevel;

    private void Start()
    {
        SetLevel(LevelSelectionData.selectedLevel);
    }

    public void SetLevel(string level)
    {
        if (grassLevel != null) grassLevel.SetActive(false);
        if (iceLevel != null) iceLevel.SetActive(false);
        if (rockLevel != null) rockLevel.SetActive(false);
        if (fireLevel != null) fireLevel.SetActive(false);
        if (waterLevel != null) waterLevel.SetActive(false);
        if (skyLevel != null) skyLevel.SetActive(false);

        switch (level)
        {
            case "Grass":
                if (grassLevel != null) grassLevel.SetActive(true);
                break;

            case "Ice":
                if (iceLevel != null) iceLevel.SetActive(true);
                break;

            case "Rock":
                if (rockLevel != null) rockLevel.SetActive(true);
                break;

            case "Fire":
                if (fireLevel != null) fireLevel.SetActive(true);
                break;

            case "Water":
                if (waterLevel != null) waterLevel.SetActive(true);
                break;

            case "Sky":
                if (skyLevel != null) skyLevel.SetActive(true);
                break;

            default:
                if (grassLevel != null) grassLevel.SetActive(false);
                break;
        }
    }


}
