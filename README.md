# Wizard’s Spell-Code
## Team Info
- Brett Thompson, programmer, music producer
- Kaitlyn McLaughlin
- Max Leibowitz

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
In contrast, programming-centered games such as SHENZHEN I/O, Butburner, and The Farmer Was Replaced place real code at the core of their gameplay, teaching algorithmic reasoning and problem solving. However, these experiences are typically slow, puzzle-driven, and detached from real-time action, along with being targeted at people new to programming. Our project aims to address these gaps by creating a fully iterative and creative combination of the wizard game and the programming game genres that can appeal to programmers of any level.


- Novelty
- Effects
- Technical Approach
  - Unity, C#


## Risks
Wizard battle, code your own spells, 2D
Features: 
- Battlefield grid with enemies and allies
- Both enemies and allies can cast spells to move/damage each other
- Player’s can write their own spells
Stretch Goals:
- Online multiplayer
- Easter eggs (i.e “segfault” has special effect) 

# Magic System
- internally each “spell” will be stored as a C# object with variables followed by a main program loop
- will be compiled when the player saves their spell
- objects deleted when they move out-of bounds (players aren’t allowed to move out of bounds)

Keywords:
- Spell (declare function)
- Start (open scope/function)
- Stop (end scope/function)
- Cast (execute  function)
- Conjure (create variable)
- Summon (place object)
- Enchant (assign value to a variable)
- Divine (compare 2 values)
- Weal (then block)
- Woe (else block)
- Wait

Types:
- Number (needs math implemented)
- String (name of objects?)
- Object (places an object on the map)


## Example programs
```
Spell FIRE Direction //fire’s default behaviour
Start //init code block
Move Self Direction //moves spell object (fireball sprite) 
Stop //end code block
```

```
Spell Ray-of-Fire //makes a persistent, growing line of fires for 5 turns
Start //init code block
Loop 5 //runs the following code 5 times
Start
Summon FIRE Right //creates a FIRE spell object 1 square to the right
Wait //Code will wait till next turn before continuing execution
Stop
Stop //end code block
```

