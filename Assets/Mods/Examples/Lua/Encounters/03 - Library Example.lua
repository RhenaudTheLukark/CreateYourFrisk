-- An example of how to use libraries.
-- First, let's include our library.
voicer = require "randomvoice"
-- Now, set some voices that are included in the default directory.
voicer.setvoices({"v_sans", "v_fluffybuns", "v_papyrus", "v_flowey"})
-- We can now use the voicer.randomize() function on all our dialogue! See the EnemyDialogueStarting function below.

encountertext = "A library example that randomizes\ra monster's voice per letter.\nCheck it out!" --Modify as necessary. It will only be read out in the action select screen.
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}

enemies = {
"poseur"
}

enemypositions = {
{0, 0}
}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
    -- If you want to change the game state immediately, this is the place.
    local randomdialogue = enemies[1].GetVar("randomdialogue") -- retrieve dialogue first, for readability
    enemies[1].SetVar("randomdialogue", voicer.randomizetable(randomdialogue)) -- Randomize voices with the library!
end

function EnemyDialogueStarting()
    -- Good location for setting monster dialogue depending on how the battle is going.
    -- Example: enemies[1].SetVar('currentdialogue', {"Check it\nout!"})   See documentation for details.
    local enemydialogue = enemies[1].GetVar("currentdialogue") -- retrieve dialogue first, for readability
    if enemydialogue ~= nil then -- Note that this can happen when a monster is having its random dialogue!
        enemies[1].SetVar('currentdialogue', voicer.randomizetable(enemydialogue)) -- Randomize voices with the library!
    end
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    -- This example line below takes a random attack from 'possible_attacks'.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
     State("ENEMYDIALOGUE") --By default, pressing spare only spares the enemies but stays in the menu. Changing state happens here.
end

function HandleItem(ItemID)
    BattleDialog({"Selected item " .. ItemID .. "."})
end