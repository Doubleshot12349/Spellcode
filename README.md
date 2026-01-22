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
  - Easter eggs (i.e “segfault” has special effect, ACE exploits in some spells) 

