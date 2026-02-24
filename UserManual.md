# Spellcode User Manual
## 1. High Level Description
### What Is Spellcode?
Spellcode is a Unity-based programming game in which players create and cast spells by writing code in a custom C-inspired scripting language.

Instead of selecting prebuilt abilities, players:
1. Write spell logic using the Spellcode language
2. Define spell behavior through structured code
3. Execute those spells in a grid-based game world

Spellcode combines:
- A custom C-based spell programming language
- A grammar-defined spell parser
- Runtime interpretation of spell logic
- Grid-based movement and interaction
- Visual and audio feedback for spell execution

### Why Would a User Want to Use It?
Spellcode is designed for users who enjoy:
- Programming-based gameplay
- Logic-driven systems
- Custom ability design
- Experimental spell creation
- Combinging coding and interactive games

  It provides a creative environment where writing code directly affects in-game outcomes.

  Players aren't just casting spells, they are creating them.

  ## 2. Installation Instructions

  ## 3. How to Run the Software

  ## 4. How to Use the Software
  ### 4.1 Core Gameplay Concept:
  #### Programming Spells
  The central mechanic of Spellcode is writing spells using a custom C-based language.

  Players define spell behavior using structured code that follows the grammar specified in:
  - grammar.txt
  - MagicDocumentation.md

  Spell behavior is interpreted at runtime and translated into in-game actions.

  ### 4.2 Writing a Spell
  A spell consists of:
  - Defined logic statements
  - Control structures (if supported)
  - Actions (damage, movement, interaction, etc.)
  - Target definitions
 
  Players input spell code through the spell interface (if implemented) or predefined scripts during development.

  The system parses the spell code according to the custom grammat and executes it in the game world.

  ### 4.3 Executing a Spell
  General flow:
  1. Player writes or selects a coded spell
  2. Spell is parsed
  3. Spell is validated
  4. Player selects a valid target time
  5. Spell effects execute
  6. Visual and audio feedback play
 
  If the spell contains syntax errors, it will fail to execute.

  ### 4.4 Movement System
  - Player moves across a grid-based map
  - Valid movement tiles are visually indicated
  - Movement follows grid constraints

  Movement may be required before or after spell execution depending on game logic.

  ### 4.5 Visual and Audio Feedback
  Spellcode includes:
  - Spell animation
  - Particle effects
  - Sound effects
  - Character animation controllers
 
  These are triggered when valid spells execute successfully.

  ## 5. Work in Progress Features
  The following functionallity my be partially implemented:
  - Full in-game spell editor UI
  - Advanced error reporting for spell syntax
  - Expanded language grammar support
  - Spell debugging tools
  - Multiplayer spell interactions
  - Save/load spell definitions
  - Additional spell libraries

  These features are under active development.

  ## 6. How to Report a Bug
  Report all issues via GitHub: https://github.com/Doubleshot12349/Spellcode/issues
  #### Include the Following Information:
  1. Title
  2. Description
  3. Steps to Reproduce
  4. Expected Behavior
  5. Actual Behavior
  6. Spell Code Used (very important)
  7. Unity version
  8. Operating system

  Spell-related bugs should always include the exact spell code used.

  ## 7. Known Bugs and Limitatons
  Current known limitations:
  - The custom spell language supports a limited grammar.
  - Syntax error feedback system may not be detailed.
  - Some invalid spell structures may not generate clear error messages.
  - Runtime interpretation may not handle all edge cases.
  - Performance may vary depending on spell complexity.
  - UI for spell input may be limited
 
  All known bugs are tracked in GitHub Issues.
