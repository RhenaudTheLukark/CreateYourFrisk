if not GetRealGlobal("ow") then error("You really should try to access these encounters the normal way...\n\nHere is a clue: You should try talking to the dog.\n\nNow good luck!", 0) end

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

enemies = {"cereb"}
enemypositions = { {0, -8} }

possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	oldname = Player.name
	Player.name = "FRISKY"
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	fade.alpha = 1
    require "Animations/cereb_anim"
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
									 "[noskip][effect:none]After two months of hard work,[w:10] RhenaudTheLukark released CYF 0.1.[w:30][next]",
									 "[noskip][effect:none]He did everything he could to keep the upcoming engine a secret,[w:10] but was too excited about showing it to the world.[w:30][next]",
									 "[noskip][effect:none]This new engine wasn't very well known,[w:10] as the official Unitale 0.2.1a version was still used by a good part of the community.[w:30][next]",
									 "[noskip][effect:none]The only new thing was the overworld system,[w:10] but it was unusable without giving away the sources.[w:30][next]",
									 "[noskip][effect:none]Then,[w:10] the developer decided to extend his engine to make it more useful for the community.[w:30][next]",
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
    AnimateCereb()
	if (beginfade or endfade) and Time.time - currentTime >= 1/3 then
	    alphaup = endfade
		endfade = false
		beginfade = false
		if alphaup then  fade.alpha = fade.alpha + Time.dt
		else  		     fade.alpha = fade.alpha - Time.dt
		end
	end
	if (fade.alpha * 1000) % 1000 ~= 1000 and (fade.alpha * 1000) % 1000 ~= 0 then
		if alphaup then  fade.alpha = fade.alpha + Time.dt
		else  		     fade.alpha = fade.alpha - Time.dt
		end
		if fade.alpha > 1 then fade.alpha = 1 end
		if fade.alpha < 0 then fade.alpha = 0 end
	end
end
require "Waves/bullettest_bouncy"

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