using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpellSelectScript : MonoBehaviour
{
    public static Dictionary<string, string> spells;
    private string fireBallProgram;
    private string lightningProgram;
    private string iceSpikeProgram;
    private string portalsProgram;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (spells == null)
        {
            spells = new()
            {
                { "Fireball", fireBallProgram },
                { "Lightning", lightningProgram },
                { "IceSpike", iceSpikeProgram },
                { "Portals", portalsProgram }
            };
        }
        DontDestroyOnLoad(gameObject);
        
    }

    // Update is called once per frame
    private void Update()
    {
        SceneManager.activeSceneChanged += ChangedActiveScene;
    }

    private void ChangedActiveScene(Scene _, Scene newScene)
    {
        DontDestroyOnLoad(gameObject);
        if (newScene.name == "HexSandbox")
        {
            //run spell select
        }
    }
    
    
    public void DeleteSpell(string name)
    {
        //Destroy(spells[name]);
    }
}
