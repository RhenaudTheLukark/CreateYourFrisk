-- A basic encounter script skeleton you can copy and modify for your own creations.

-- music = "shine_on_you_crazy_diamond" --Always OGG. Extension is added automatically. Remove the first two lines for custom music.
encountertext = "[speed:3]Now you can move the [lettereffect:rotate]Arena[lettereffect:none] ! [lettereffect:shake]Check it out !" --Modify as necessary. It will only be read out in the action select screen.
if not isCYF then
    encountertext = "You better use this mod on CYKa !\nHere nothing will work."
end
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}

unescape = true
autolinebreak = true

getposition = false
a = false

enemies = {
"poseur3"
}

enemypositions = {
{0, 0}
}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
--possible_attacks = {"arenatest_move", "arenatest_moveto", "arenatest_moveandplayer", "arenatest_moveandresize", "arenatest_movetoandresize"}
--possible_attacks = {"WaveTest - Dust()"}
possible_attacks = {"arenatest_move"}

function EncounterStarting()
	--require "Libraries/blue"
	--blue.TurnBlue()
    -- If you want to change the game state immediately, this is the place.
	Player.lv = 20
	Player.hp = 99
	if not isCYF then
		DEBUG("Not in CYF !")
		encountertext = "You better use this mod on CYKa !\nHere nothing will work."
	else
	    if not GetRealGlobal("testCYF") then
	        SetRealGlobal("testCYF", 0)
			DEBUG("You entered this mod 1 times.")
		else
		    SetRealGlobal("testCYF", GetRealGlobal("testCYF") + 1)
			DEBUG("You entered this mod " .. GetRealGlobal("testCYF") .. " times.")
		end
		DEBUG(GetAlMightyGlobal("AlMighty"))
		SetAlMightyGlobal("AlMighty", "Weehee ! AlMightyGlobals !")
		DEBUG("In CYF !")
		if not GetAlMightyGlobal("Aeudegaga") then
		    DEBUG("not GetAlMighty works with unknown values")
		end
		if GetAlMightyGlobal("Aeudegaga") == false then
		    DEBUG("GetAlMighty = false works with unknown values")
		end
	    CreateLayer("WEEHEE", "Top", true)
	    CreateProjectileLayer("After", "", false)
	    CreateProjectileLayer("Before", "", true)
		--enemies[1].Call("SetSliceAnimOffset", {100,0})
		--enemies[1].Call("SetBubbleOffset", {0,100})
		--enemies[1].Call("SetDamageUIOffset", {-50,-50})
		DEBUG(enemies[1]["monstersprite"].absx)
		DEBUG(Player.name)
		SetFrameBasedMovement(true)
	end
	enemies[1].Call("SetDamage", 29)
	--State("DEFENDING")
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    -- This example line below takes a random attack from 'possible_attacks'.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
	DEBUG(nextwaves[1])
end

--function EnteringState(newstate, oldstate)
--    if newstate ~= "DEFENDING" and oldstate == "DEFENDING" then
--        State("ENEMYDIALOGUE")
--    end
--end

function BeforeEnemySelect()
    SetAction("FIGHT")
end

--[[for variable_that_indicates_the_number_of_loops_already_finished = starting_number, ending_number do
        -- code
    end]]

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
	if isCYF then
		if enemies[1].GetVar("hp") != 30 then
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
	
    if isCYF then
		--[[if not getposition and enemies[1].GetVar("canmove") then
		    enemybeginpos = {enemies[1].GetVar("posx"), enemies[1].GetVar("posy")}
			getposition = true
		elseif getposition then
		    enemies[1].Call("MoveTo", {enemybeginpos[1] + 200 * math.sin(Time.time * 3), enemybeginpos[2] + 40 * math.sin(Time.time * 4)})
		end]]
		if Input.GetKey("Escape") == 1 then
		    DEBUG("Unescapable !")
		elseif Input.GetKey("K") == 1 then
		    DEBUG("New keys !")
			enemies[1].Call("Kill")
		end
		--enemies[1].Call("Move", {5 * math.sin(Time.time), 0})
		--enemies[1].Call("Move", {5 * math.sin(Time.time), 0, true}) to move with arena bound
		--enemies[1].Call("BindToArena", false)
	end
end
