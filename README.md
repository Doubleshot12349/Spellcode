# Wizard’s Spell-Code
## Team Info
- Brett Thompson, programmer, music producer
- Kaitlyn McLaughlin, programmer, asset developer
- Max Leibowitz, system integration, testing
- Aman Nurmukhanbetov, gameplay systems, networks

[Git repo](https://github.com/Doubleshot12349/Spellcode)

## Communications
We will primarily use Discord for day-to-day coordination, quick questions, and informal check-ins. All major decisions, technical discussions, and progress updates will be recorded in the Discord channels.

For structured documentation, planning, and issue tracking, we will use the project’s Git repository on GitHub/GitLab. Each member is expected to:
- Commit regularly with clear messages
- Use issues and project boards for task management
- Review pull requests collaboratively



# Project Description
## Abstract
This is a top down battle game in which the player controls a wizard to fight against other wizards and fantasy creatures. The main gimmick of the game is that the player will create their spells in a simple magic-themed scripting language. This provides a deeper level of learning programming beyond what other games are able to do. This includes advanced topics, like theory, algorithms (especially graph), and functional programming.

## Goal
To create a wizard battle game where players can write their own spells using simple code to move, attack, and outsmart enemies.
- Players use a magic-themed coding language to create spells.
- Spells are compiled and run during battles against other wizards and creatures.
- The game makes learning to code fun and hands-on.
- Provide visual feedback and effects so spells feel magical and responsive.
- Design levels and challenges that encourage players to improve and iterate on their spell code.


## Current Practice
A few other wizard/spell casting games to look into:
  - Mage Arena - Voice control
  - YAPYAP - Voice control
  - Magika/Magika 2 - Element combination
  - Noita - Complex item combination
  - Blade and Sorcery - VR

Programming-based games:
  - The Farmer Was Replaced
  - SHENZHEN I/O
  - Bitburner
  
Most wizard and spell-casting games emphasize player expression through interactive systems rather than true programmability. Titles such as Magika, Notia, and Mage Arena allow players to combine elements or use novel input methods to produce a variety of effects, but the underlying spell behaviors remain fully predefined by the developer. Players explore a fixed design, discovering combinations rather than authoring their own logic. These systems can feel complex and creative, yet they do not engage their players in genuine computational thinking, as concepts such as variables, control flow, or iteration are absent.

In contrast, programming-centered games such as SHENZHEN I/O, Butburner, and The Farmer Was Replaced place real code at the core of their gameplay, teaching algorithmic reasoning and problem solving. However, these experiences are typically slow, puzzle-driven, and detached from real-time action, along with being targeted at people new to programming.

## Novelty
Our project aims to address these gaps by creating a fully iterative and creative combination of the wizard game and the programming game genres that can appeal to programmers of any level. Making it a multiplayer game will encourage players to continue with our game through competition, being able to show off and help their friends play the game.

## Effects
Most programming games nowadays are targeted towards simple, beginner coding, which leaves experienced coders bored, and with no "fun" way to practice and develop their skills. It being multiplayer will also help players introduce others to coding, which is becoming more important in all careers, not just Computer Science.

## Technical Approach
  - Unity
  - C# scripting
  - JSON for save data
  - Unity Test Framework for test cases


## Risks
  - Unity deprecates current version
    -> We would need to update all our scripts to match the new version of Unity
  - Unable to find/make sufficient Audio/Visual assets
    -> There are many free assets available for game development, especially in 2D,so if this becomes an issue we will have to rely more heavily on these, which could make the game a hodgepodge of various art styles.

## Features: 
  - Battlefield grid with enemies, allies, and obstacles
  - Both enemies and allies can cast spells to move/damage each other
  - Players can write their own spells
  - 4 offensive and 2 defensive "Base spells"
  - 3 Tutorial levels to teach the magic system
## Stretch Goals:
  - Online multiplayer
  - Single-player Challenge levels
  - Easter eggs (i.e “segfault” has special effect)

# Functional Requirements

  ## FR-01 Player vs Player Match
  - Actors: Player1 and Player2
  - Triggers: "Start Game" is pressed and a map is selected
  - Preconditions:
      1. 3 spells selected for each player
      2. Each player is placed on opposite sides of the map with full health and mana
  - Postconditions:
      1. One player is left standing and declared the winner
      2. The other player's health has been reduced to 0
      3. Map is cleared and a rematch is offered
  - List of Steps:
      1. Player 1 moves to a different square
      2. Player 1 selects a spell to cast in a desired direction
      3. Player 2 suffers damage if hit with Player 1's spell
      4. Player 2 moves to a different square
      5. Player 2 selects a spell to cast in a desired direction
      6. Player 1 suffers damage if hit with Player 2's spell
      7. Play continues from step 1 until a player is defeated
  - Extensions/variations of the success scenario
      1. A player chooses to skip movement and only cast a spell on their turn.
      2. A spell misses its target due to range, direction, or obstacle interference
      3. A defensive spell is cast, reducing or negating incoming damage
      4. A spell affects multiple tiles, damaging both the opponent and the environment
      5. A player runs out of mana and must wait a turn or use a low-cost spell
      6. Environmental hazards on the map influence movement and spell outcomes
      7. A player's custom spell behaves differently based on parameters defined in their code
   - Exceptions: failure conditions and scenarios
       1. A player selects an invalid or uncompilable spell
         - The system rejects the spell and prompts the player to fix their code
       2. A player attempts to move to an occupied or blocked square
          - The move is denied, and the player is prompted to choose another square 
       3. A spell is cast without sufficient mana
          - The spell fails, and the player loses their turn
       4. The game state becomes desynchronized
          - The match is paused and either resynchronized or safely terminated
       5. A runtime error occurs in a custom spell during execution
          - The spell is canceled, the error is reported, and the player is forced to select a different spell
       6. A player disconnects or quits mid-match
          - The remaining player is declared the winner, and the match ends.
         
  ## FR-02 Custom Spell Creation and Validation
  - Actors: Player
  - Triggers: Player selects "Create/Edit Spell" from the main menu or loadout screen
  - Preconditions:
      1. Player is at the main menu or spell loadout screen
      2. Player has at least one available spell slot
  - Postconditions:
      1. A spell is saved to the player's loadout
      2. The spell is either marked as valid or usable or invalid and locked
      3. The player is returned to the spell loadout screen
  - List of Steps:
      1. Player selects an empty or existing spell slot
      2. The spell editor interface opens
      3. Player writes or modifies spell code
      4. Player presses "Compile" or "Validate"
      5. The system parses the spell code
      6. If valid, the system displays a success message and enables "Save"
      7. Player saves the spell
      8. The spell becomes available for selection in matches
  - Extension/variations of the success scenario:
      1. Player loads a template base spell and modifies it
      2. Player tests the spell in a sandbox preview mode
      3. The editor highlights syntax in different colors for readability
      4. The system suggests corrections for minor syntax errors
      5. A spell includes parameters that alter its behavior at runtime
      6. The player renames and categorizes spells (e.g., "Offence," "Defence," "Utility")
      7. The player duplicates an existing spell to create a variant
  - Exceptions: failure conditions and scenarios:
      1. The player enters malformed syntax
          - The system highlights the error and provides a descriptive message
      2. The spell exceeds complexity or resource limits
         - The system rejects the spell and explains which limits were violated
      3. The player attempts to save without validating
         - The system blocks saving and prompts validation
      4. The parser encounters an internal error
         - The editor displays a generic error and prevents saving
      5. The player closes the editor with unsaved changes
         - The system prompts the player to save, discard, or cancel
      6. The spell compiles but contains unsafe runtime behavior
         - The system flags the spell as invalid and prevents match use

  ## FR-03 Health, Mana, and Special Effects System
  - Actors: Player 1, Player 2, Field Controller
  - Triggers: After either player has finished taking their turn during a match
  - Preconditions:
    1. A match has been started between the 2 players
    2. It is Player 1's turn
    3. Each Players' mana has been refreshed
    4. The match is on a water-affinity field
    5. There are no other ongoing effects active on the field like:
      - Fire balls
      - Acid pools
      - Summoned Creatures
      - Poison or regeneration applied to either player
  - Postconditions:
    1. Each player's health has been adjusted accordingly
    2. Battlefield controller has added the appropriate effects to the field (see above for examples)
    3. Each player has lost mana according to which spell they cast
  - List of Steps:
    1. Player 1 casts a non-fire "quick spell" (one with no ongoing effects)
    2. Battlefield controller animates the correct sprite on the screen
    3. If a collision occurs between the spell sprite and the other player:
      i. The other player's health is decreased by the appropriate amount for the spell
      ii. The spell sprite is removed from the field
    4. Otherwise the spell missed:
      i. The other player's health has not been changed
      ii. The spell sprite is removed from the field when it reaches the boundary or has a collision with a non-player obstacle

    
  - Extension/variations of the success scenario:
    1. A "quick" fire spell is selected by the player resulting in reduced damage if it hits
    2. A "long" spell is cast resulting in:
      - its sprite remains on the field for 1 or more turns
      - collisions can happen if either player moves onto the spell, or if a spell hits another spell
      - a spells iterative/recursive behavior is evoked
    3. A poison or regen spell is cast and contacts either player resulting in:
      - Health increase/decrease at the start of each of their turns
    
  - Exceptions: failure conditions and scenarios:
    1. Spells fail to trigger collisions with a player, obstacle, or the boundary of the field
    2. A player's health is not adjusted properly when they are hit with a spell
    3. A player's mana is not adjusted properly when they are hit with a spell
    4. A spell is not removed from the field at appropriate times

  ## FR-04
  - Actors: Effects, Players
  - Triggers: Just after an effect was cast
  - Preconditions:
    1. There is an active match
    1. One player has cast a spell against the other
    1. The spell created an effect (i.e. fireball)
    1. An environment is selected
  - Postconditions:
    1. The effect's damage is changed according to the environment
    1. If the effect runs into an obstacle, it will be blocked
    1. If the effect is incompatible with the environment, it will be randomly steered
  - List of steps:
    1. Player 1 casts a spell
    1. The spell creates an effect targeted towards player 2
    1. The effect's damage is modified appropriately
    1. The effect is randomly steered as needed
  - Extension/variations of the success scenario:
    1. A player casts a spell
    1. The spell creates an ice effect
    1. The effect checks the environment (volcano) and determines that it should deal less damage
    1. The effect hits the other player and deals reduced damage
  - Exceptions:
    1. An effect passes through an obstacle
    1. A spell fails to be randomly steered when it was supposed to
    1. A spell doesn't properly change its damage


# Non-Functional Requirements

  ## NFR-A User Interface
  - The game window is clear and easy to read
  - Keyboard controls are easy to use
  ## NFR-B Magic System
  - Spell creation is well documented and intuitive
  - Invalid spell code is caught by the program and not allowed to be used in a match
    
# External Requirements

  ## XR-i Error Detection
  - Invalid user input is caught and handled gracefully
  - Memory requirements are stress tested so the game doesn't crash under all but the most extreme cases
    
  ## XR-ii Access
  - INSTALLATION.md has clear, working instructions for installing the game
  - Game is web-hosted on itch.io and/or another platform
  
    
  ## XR-iii Dev-Access
  - Github repo is public
  - INSTALLATION.md has clear, working instructions for downloading and opening the Unity project for other developers

  ## XR-iv Online Play
  If online play stretch goal is implemented:
  - INSTALLATION.md has detailed instructions for setting up online play, including
      - links to instructions for getting IPv6 addresses, port numbers, and setting up port forwarding on a router
  - Desktop version of the game has prompts for inputting IPv6 addresses and port numbers to allow for online multiplayer

# Team Process Description

  ## Software Toolset
  Our team will use a combination of industry-standard development tools to ensure effective collaboration and stable       development
  - Unity - Primary game engine for rendering, physics, input handling, and deployment
  - C# - Core programming language for game logic and systems
  - GitHub - Version control, issue tracking, and project management
  - Visual Studio / VS Code - Integrated development environments for C# and scripting
  - Discord - daily communication, quick feedback, and coordination
  - Unity Test Framework - Automated tests for spell parsing, combat logic, and edge cases
  
  This toolset is justified by its accessibility, maturity, and compatibility. Unity and C# provide quick iteration for game systems, while GitHub and Discord support asynchronous collaboration and accountability. Automated testing reduces regressions in the spell system, which is central to the project.
  ## Team Roles
  - Brett Thompson - Core Systems Programmer and Audio Lead

    Brett focuses on executing custom player spells, environment interactions, and audio assets. This role is important for integrating the customized programming language with Unity's game mechanics. 

- Kaitlyn McLaughlin - Gameplay Programmer and Asset Developer

  Kaitlyn is responsible for player controls, UI elements, level layouts, and visual assets. This role ensures that comples programming mechanics are presented i na  clear and approchable way.
- Max Leibowitz - Systems Integration and Testing Lead

  Max oversees system integration, testing infrastructure, and gameplay balance. This role is necessary to ensure that independently developed components work together reliably.
- Aman Nurmukhanbetov - Gameplay Systems and Networking Support

  Aman focuses on turn management, match flow, and support for multiplayer or simulated multiplayer systems. This role is essential for coordinating player actions, enforcing rules, and preparing the project for online play as a stretch goal.

  ## Task Schedule
  Week 5
  - Brett:Set up GitHub workflow, make audio files
  - Kaitlyn: Create battlefield grid, player movement, and placeholder sprites
  - Max: Implement basic spell language grammar and output format(bytecode)
  - Aman: Implement turn manager skeleton and basic match state model
    
  Week 6
    - Brett: Spell execution engine supports movement and damage spells
    - Kaitlyn: UI for spell selection and turn display works in a sandbox scene
    - Max: Parser, finish bytecode
    - Aman: Turn order enforcement and action queueing functional
    
  Week 7
    - Brett: Implement health, mana, and visual feedback for damage and movement
    - Kaitlyn: Implement Spell edit UI, syntax highlighting etc.
    - Max: compiler and spell validation, Add error reporting for invalid spell code
    - Aman: Match setup and reset logic complete
    
  Week 8
    - Brett: Defensive spells and conditional logic supported, test for spell interactions,End-to-end test: two players can complete a full local match
    - Kaitlyn: Title screen and match setup flow functional, UI and animation tests
    - Max: Tests to validate that the spell language works and compiles properly
    - Aman: Local PvP loop stable (start -> turns -> win condition -> reset)
    
  Week 9
    - Brett: Environmental interactions supported in spell execution, save loading
    - Kaitlyn: Tutorial level demonstrating spell creation and casting
    - Max: bug fixing, lambda functions?
    - Aman: Balance tests and structured bug tracking from playtests
    
  Week 10
    - All: Polish, bug fixes, documentation, and external playtesting integration
  ## Risks
  1. Spell System Complexity
  
     The custom scripting language may become too complex or unstable to implement within the timeline
  2. System Integration Failures
   
     UI, turn logic, and spell execution may not align cleanly
     
  3. Scope Creep from Multiplayer Goals
  
     Online play could exceed the project's time budget

  ## External Feedback Elicitation
    External feedback is most valuble one a complete local PvP match can be played (Week 4 of development). At this stage, core systems exist, but design decisions are still flexible. We will:

   - Distribute a playable build to classmates and friends
   -  Observe first-time users writing and casting spells
   -  Collect structured feedback on:
       -  Clarity of spell syntax
       -  Ease of learning
       -  Perceived fun and fairness
  
    This feedback will directly inform revisions to the spell language, tutorials, and UI before final polish

