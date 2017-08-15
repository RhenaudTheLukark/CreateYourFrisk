-- A basic encounter script skeleton you can copy and modify for your own creations.

-- music = "shine_on_you_crazy_diamond" --Always OGG. Extension is added automatically. Remove the first two lines for custom music.
encountertext = "The path is blocked by\nmany quins !" --Modify as necessary. It will only be read out in the action select screen.
if not isCYF then
    encountertext = "You better use this mod on CYKa !\nHere nothing will work."
end
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}

enemies =        {"poseur3",  "poseur3", "poseur3", "poseur3", "poseur3", "poseur3", "poseur3", "poseur3", "poseur3"}
enemypositions = {{-200, 10}, {-150, 7}, {-100, 5}, {-50, 2},  {0, 0},    {50, -2},  {100, -5}, {150, -7}, {200, -10}}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
--possible_attacks = {"arenatest_move", "arenatest_moveto", "arenatest_moveandplayer", "arenatest_moveandresize", "arenatest_movetoandresize"}
--possible_attacks = {"WaveTest - Dust()"}
possible_attacks = {"arenatest_move"}

function EncounterStarting()
    -- If you want to change the game state immediately, this is the place.
	Player.lv = 20
	Player.hp = 99
	if not isCYF then
		DEBUG("Not in CYF !")
		encountertext = "You better use this mod on CYKa !\nHere nothing will work."
	else
	    CreateLayer("WEEHEE", "Top", true)
	    CreateProjectileLayer("After", "", false)
	    CreateProjectileLayer("Before", "", true)
	end
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    -- This example line below takes a random attack from 'possible_attacks'.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function BeforeEnemySelect()
    SetAction("FIGHT")
end

--[[for variable_that_indicates_the_number_of_loops_already_finished = starting_number, ending_number do
        -- code
    end]]

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
	if isCYF then
		if enemies[1].GetVar("hp") ~= 30 then
			enemies[1].Call("SetDamage", -29)
		end
		SetFrameBasedMovement(false)
		enemies[1].Call("BindToArena", true)
	end
    DEBUG(Player.name)
	--State("ENEMYDIALOGUE")
end

function HandleItem(ItemID)
    BattleDialog({"Selected item " .. ItemID .. "."})
end

function Update()
end
