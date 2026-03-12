using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ManualPageController : MonoBehaviour
{

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    public Button nextButton;
    public Button backButton;

    private int currentPage = 0;

    string[] titles =
    {
        "Welcome to Spellcode",
        "In this Manual",
        "Game Objective",
        "Player Controls",
        "Spell Casting",
        "Spell Language",
        "Spellcode Syntax Basics 1/2",
        "Spellcode Syntax Basics 2/2",
        "Control Flow and Functions 1/2",
        "Control Flow and Functions 2/2",
        "Spell Functions",
        "Example Spell",
        "Strategy Tips"
    };

    string[] pages =
    {
        // Welcome to spell code
        @"Spellcode is a turn-based tactical strategy game where players write their own spells using a custom C-inspired programming language. 

Instead of selecting abilites from a menu, players create spells by writing code.

This allows players to experiment with logic, creativity, and strategy while battling on a hex-based map.

Each turn, players can move, cast spells, and interact with the battlefield using code they have written themselves",

        @"Game Objective
Player Controls
Spell Casting
Spell Language
Spellcode Syntax Basics
Control Flow and Functions
Spell Functions
Example Spell
Strategy Tips",
        // Game objective
        @"The goal of Spellcode is to defeat your opponent using stetegic movement and programmable spells.

Players take turns moving across the hex grid and casting spells that affect enemies or the environment.

Victory is acheived by reducing your opponent's health to zero.

Because spells are programmable, the most effective strategy often comes from clever spell design rather than raw power.",

        //Player Controls
        @"Movement
Left click on a hex tile to move. You can only move to a tile directly adjacent to your character.

Spell Casting
Right-click on a hex tile to cast spells. 

Turn System
Players alternate turns. You can move and cast spells during your turn.

Code Editor
Use the in-game code editor to write and modify your spells.",

        // Spell casting
        @"Spells are written using the Spellcode language.
Each spell is a small program that runs when cast.
Your code determines:
    How much damage the spell deals
    Which targets are affected
    Special effects or behaviors
   
When the spell is executed, the game interprets your code and applies the results to the battlefield

Keep in mind:
Spells require Mana. Every spell you cast uses different amounts of Mana based on its power and effect.
If you run out of Mana, you won't be able to cast spells. But don't worry, your Mana will regenerate over time.",

        // Spell language
        @"Spellcode uses a simplified C-style language.

Basic structure:
    IF (condition) {
        action
    }

Variables may reprisent things like:
    health
    distance
    target

You may use comparisons and ligic to control how your spell behaves.",

        // Spellcode syntax basics 1/2
        @"Variables
Create variables with the var keyword:
    var damage = 10
    var range = 3

Supported types
    int     whole numbers
    double  decimal numbers
    bool    true or false
    char    single character
    string  text
    x[]     arrays of any supported type

Assignments
You can change a variable after creating it
    var mana = 5
    mana = mana + 1",
        
        // Spellcode syntax basics 2/2
        @"Arrays
Arrays store mutiple values:
    var pos = get_click()
    var nums = new int[3]

Array elements are accessed with brackets:
    nums[0] = 10

You can also read the size of an array:
    nums.size",

        // Control Flow and Functions
        @"Math Operations
    +       addition
    -       subtraction
    *       multiplication
    /       division
    %       modulo

Comparison Operators
    ==      equal to
    !=      not equal to
    <       less than
    <=      less than or equal to
    >       greater than
    >=      greater than or equal to

Boolean Operators
    &&      and
    ||      or
    !       not",
        
        // Control Flow and Functions 2/2
        @"If Statements
    if mana > 3 {
        print('Ready')
    } else {
        print('Not enough mana')
    }

Loops
    while mana > 0 {
        mana = mana - 1
    }

    for (var i = 0; i < 3; i = i + 1) {
        print(i)
    }

Functions
    fun add(a: int, b: int) -> int {
        return a + b
    }",
        // Spell Cunctions
        @"Spellcode includes built-in functions for interacting with the game.
get_click()     Waits for player to clcik tile and returns its coordinates
EX: var pos = get_click()
The result is an array:
    pos[0] = q coordinate
    pos[1] = r coordinate

spawn_effect(type)  Creates a spell effect and returns its effect ID
EX: var effect = spawn_effect(3)
If the spell cannot be created, it returns -1

move_effect(q, r, id)   Moves an existing effect to a tile
EX: move_effect(pos[0], pos[1], effect)

Useful Pattern
    1. Wait for a clicked tile
    2. Spawn an effect
    3. Move that effect to the clicked tile",

        // Example Spell
        @"Fireblast
    if (targetDistance <= 3) {
        damage = 10;
    }

This spell checks if the target is within range. If the condition is true, it deals damage.

By combining conditions, logic, and variables, you can design powerful and creative spells.",

        // Strategy Tips
        @"- Keep your spells simple while learning the system
- Positioning on the hex grid is very important
- Combine movement with spell range for tactical advantages.
- Test different spell logic to discover powerful combinations.

Common Mistakes
    - Using the wrong array index
    - Forgetting that get_click() returns an array
    - Trying to move an effect that failed to spawn
    - Writing loops that never stop when testing"
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdatePage();
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePage();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePage();
        }
    }

    void UpdatePage()
    {
        titleText.text = titles[currentPage];
        contentText.text = pages[currentPage];

        backButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < pages.Length - 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
