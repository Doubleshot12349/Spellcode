using UnityEngine;
using System.Collections.Generic;

public class MagicInterpreter : MonoBehaviour
{
    private class Instruction
    {
        public string name;
        public string value;
        public Instruction(string n,string v)
        {
            name = n;
            value = v;
        }

    }
    public GameObject Spell;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spell.GetComponent<Renderer>().enabled=false;
    }

    static string EndChar(string start)
    {
        switch (start)
        {
            case "start":
                return "stop";
            case "divine":
                return "stop";
            default:
                return "\n";
        }
    }
    void Cast()
    {
        //placeholder until typing gets figured out
        string text = "";
        List<Instruction> steps=null;//stores the instructions to be carried out 
        int i = 0;
        do
        {
            //reads the next word from text
            int spaceIndex = text.IndexOf(" ", i);
            string name = text.Substring(i, spaceIndex - i);
            string value = text.Substring(spaceIndex, text.IndexOf(EndChar(name),spaceIndex)-i);
            Instruction next = new(name, value);
            steps.Add(next);
            i += text.IndexOf(EndChar(name));
        } while (i < text.Length);
        

    }
}
