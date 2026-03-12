using UnityEngine;

public class ManualToggle : MonoBehaviour
{
    public GameObject manualPanel;

    public void Start()
    {
        manualPanel.SetActive(false);
    }

    public void ToggleManual()
    {
        manualPanel.SetActive(!manualPanel.activeSelf);
    }
}
