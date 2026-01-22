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
