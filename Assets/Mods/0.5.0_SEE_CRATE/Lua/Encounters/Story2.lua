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

enemies = {"claribel", "ellie"}
enemypositions = {{-34, 0}, {0, 0}}

possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	oldname = Player.name
	Player.name = "FRISKY"
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	fade.alpha = 1
    enemies[2]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:60][next]",
	                                 "[noskip]But,[w:15] after some time playing around with the engine,[w:15] he found out that lots of features were missing.[w:60][next]",
									 "[noskip]First of all,[w:15] there was no overworld:[w:20] to make an AU with his friends,[w:15] RhenaudTheLukark needed one.[w:60][next]",
									 "[noskip]Thus, after contacting with lvkuln,[w:15] he got access to the sources a bit before the official release of the open-source version.[w:60][next]",
									 "[noskip]Having been a complete beginner in Unity,[w:15] he first tried to fix the bugs related to the open-source version of the engine...[w:20] and succeeded.[w:60][next]",}
    enemies[1]["currentdialogue"] = {"", "", "", "", "", 
	                            	 "[noskip]After fixing these bugs,[w:15] he felt something new,[w:15] as if he did something extraordinary:[w:20] it was at this moment he knew he'd have to resume lvkuln's work.[w:60][next]",
									 "[noskip]He then tried to contact the original developer,[w:15] but in vain:[w:20] it was already too late.[w:20] IRL stuff took the latter away from the project.[w:60][next]",
									 "[noskip]Thus,[w:15] he tried to create something on his own,[w:15] even if he had never followed any tutorials on using Unity or even C#.[w:60][next]",
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