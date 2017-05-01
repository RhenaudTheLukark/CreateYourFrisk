if not GetRealGlobal("ow") then error("You really should try to access these encounters the normal way... Here is a clue: you need to move the dog in the map test2 using an event. Now good luck!") end

encountertext = "Poseur strikes a pose!" --Modify as necessary. It will only be read out in the action select screen.
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}
autolinebreak = true
unescape = true

currentTime = Time.time
beginfade = false
endfade = false
alphaup = false

enemies = {"mionn"}
enemypositions = {{0,0}}

possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	oldname = Player.name
	Player.name = "FRISKY"
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	fade.alpha = 1
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:60][next]",
	                                 "[noskip][effect:none][func:Animate]However,[w:15] the next releases of Create Your Frisk weren't very successful,[w:15] despite a lot of new features,[w:15] tweaks and functions.[w:60][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/sad]The problem was that all the versions had at least one bug that greatly reduced the appeal of the engine.[w:60][next]",
									 "[noskip][effect:none]Even today,[w:15] in 0.5.1,[w:15] though RTL said that 0.4.4.4 was stable,[w:15] it wasn't.[w:60][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/transit_to_fight4]Besides,[w:15] there will always be a bug somewhere.[w:15] That's how coding is.[w:60][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/sad]He had a lot of problems with this,[w:15] and felt sorry for himself because he couldn't make a good, stable engine for people to use freely...[w:60][next]",
									 "[noskip][func:LaunchFade, false][w:35][func:State,DONE]"}
    require "Waves/bullettest_touhou"
	State("ENEMYDIALOGUE")
end

function LaunchFade(begin)
    if begin then  
	    beginfade = true
	    fade.alpha = 1
	else           
	    endfade = true
	    fade.alpha = 0
    end
end

function Update()
    enemies[1].Call("Update")
	if (beginfade or endfade) and Time.time - currentTime >= 1/3 then
	    alphaup = endfade
		endfade = false
		beginfade = false
		if alphaup then  fade.alpha = fade.alpha + Time.dt
		else  		     fade.alpha = fade.alpha - Time.dt
		end
	end
	if (fade.alpha * 1000) % 1000 != 1000 and (fade.alpha * 1000) % 1000 != 0 then
		if alphaup then  fade.alpha = fade.alpha + Time.dt
		else  		     fade.alpha = fade.alpha - Time.dt
		end
		if fade.alpha > 1 then fade.alpha = 1 end
		if fade.alpha < 0 then fade.alpha = 0 end
	end
end

function EnteringState(newstate, oldstate)
	if newstate == "DONE" then
		Player.name = oldname
	end
end

function EnemyDialogueStarting()
end

function EnemyDialogueEnding()
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding()
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
     State("ENEMYDIALOGUE")
end

function HandleItem(ItemID)
    BattleDialog({"Selected item " .. ItemID .. "."})
end