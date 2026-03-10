using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpellSelectScript : MonoBehaviour
{
    public static Dictionary<string, string> spells;
    private readonly static string fireBallProgram =
        "while true {     var pos = get_click();     print('A');     var effect = spawn_effect(0);     if effect != -1 {         move_effect(pos[0], pos[1], effect);    } }";
    private readonly static string lightningProgram =
        "while true {     var pos = get_click();     print('A');     var effect = spawn_effect(1);     if effect != -1 {         move_effect(pos[0], pos[1], effect);    } }";
    private readonly static string iceSpikeProgram =
        "while true {     var pos = get_click();     print('A');     var effect = spawn_effect(2);     if effect != -1 {         move_effect(pos[0], pos[1], effect);    } }";
    private readonly static string portalsProgram =
        "while true {     var pos = get_click();     print('A');     var effect = spawn_effect(3);     if effect != -1 {         move_effect(pos[0], pos[1], effect);    } }";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
        
    }
}
