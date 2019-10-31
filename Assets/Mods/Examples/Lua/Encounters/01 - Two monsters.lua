encountertext = "Your path is blocked by two mannequins!" --Modify as necessary. It will only be read out in the action select screen.

wavetimer = 4
arenasize = {155, 130}
nextwaves = {"bullettest_touhou"}
autolinebreak = true

enemies = {"twoMonstersPoseur", "twoMonstersPosette"}
enemypositions = { {-180, 0}, {120, 0} }

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EnemyDialogueStarting()
    -- Good location for setting monster dialogue depending on how the battle is going.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
    State("ENEMYDIALOGUE")
end
