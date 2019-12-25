-- A basic encounter script skeleton you can copy and modify for your own creations.

-- music = "shine_on_you_crazy_diamond" --Always OGG. Extension is added automatically. Remove the first two lines for custom music.
encountertext = "The path is blocked by\rmany quins!" --Modify as necessary. It will only be read out in the action select screen.
if not isCYF then
    error("You better use this mod on CYF!\nHere nothing will work.")
end
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}
autolinebreak = true

enemies =        {"poseur",   "poseur",  "poseur",  "poseur",  "poseur",  "poseur",  "poseur",  "poseur",  "poseur"}
enemypositions = {{-200, 10}, {-150, 7}, {-100, 5}, {-50, 2},  {0, 0},    {50, -2},  {100, -5}, {150, -7}, {200, -10}}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
    -- If you want to change the game state immediately, this is the place.
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    -- This example line below takes a random attack from 'possible_attacks'.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleItem(ItemID)
    BattleDialog({"Selected item " .. ItemID .. "."})
end
