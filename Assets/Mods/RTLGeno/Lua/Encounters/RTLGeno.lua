-- An animation demo with a rotating Sans head.

--Appel d'un script d'ennemi : enemies[1] = RTL, enemies[2] = Lukark

music = "Anticipation_Amplified"
encountertext = "It's time to\nkick his ass." --Modify as necessary. It will only be read out in the action select screen.
nextwaves = {"BallArms"}
wavetimer = 10.0
arenasize = {130, 130}
test = false

enemies = {
"RTL",
"Lukark"
}

enemypositions = {
{0, -40},
{0, 20}
}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"BallArms"}

function EncounterStarting()	
    require "Animations/RTL_anim"
	require "Animations/lukark_anim"
	require "Libraries/advanced_playerlib"
	
	Player.name = "RHENAO"
	Player.lv = 12
	Player.hp = 64
	Audio.Pitch(0.2)
	Audio.Volume(1)
	SetGlobal("wavetimer",10.0)
	
	SetGlobal("RTL","close")
	SetGlobal("RTLDead",false)
	SetGlobal("sparedRTL",false)
    SetGlobal("sparingRTL",10)
	
	SetGlobal("revived",false)
	SetGlobal("animDilate",false)
	
	SetGlobal("Lukark","hidden")
	SetGlobal("LukarkDead",false)
	AnimateLukark()
	
	switchrevive = false
	switchrevive2 = false
	
    --Include the animation Lua file. It's important you do this in EncounterStarting, because you can't create sprites before the game's done loading.
    --Be careful that you use different variable names as you have here, because the encounter's will be overwritten otherwise!
    --You can also use that to your benefit if you want to share a bunch of variables with multiple encounters.
end

function Update()
    --By calling the AnimateSans() function on the animation Lua file, we can create some movement!
	if GetGlobal("revived") == false then
		AnimateRTL()
	else
		AnimateLukark()
		if switchrevive == false then 
			enemies[2].Call("Activate")
			enemies[1].Call("Deactivate")
			switchrevive = true
		end
	end
	AdvPlayerLib.Size.PulseByHP()
end

function EnemyDialogueStarting()
	SetGlobal("placingRTL", 0)
	if switchrevive2 == false then
		encountertext = "The true battle\nbegins now."
		switchrevive2 = true
    end
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    -- This example line below takes a random attack from 'possible_attacks'.
	if GetGlobal("revived") == true then
		if enemies[2]["hp"] <= 200 then
			encountertext = "You can see that\nLukark's health is low."
		else
			encountertext = RandomEncounterText()
		end
	else
		encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
		
	end
	SetGlobal("RTL","close")
	SetGlobal("Lukark", "normal")
	nextwaves = { possible_attacks[math.random(#possible_attacks)] }
	SetGlobal("placingRTL", -1)
end

function DefenseEnding() --This built-in function fires after the defense round ends.
	SetGlobal("Lukark", "normal")
	--encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
	if GetGlobal("revived") == false and GetGlobal("sparingRTL") == 1 then
		SetGlobal("sparedRTL", true)
		SetGlobal("sparingRTL", 0)
    end
	State("ENEMYDIALOGUE")
end