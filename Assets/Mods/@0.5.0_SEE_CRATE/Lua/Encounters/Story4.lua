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
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
	                                 "[noskip][effect:none][func:Animate]However,[w:10] the next releases of Create Your Frisk weren't very successful,[w:10] despite a lot of new features,[w:10] tweaks and functions.[w:30][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/sad]The problem was that all the versions had at least one bug that greatly reduced the appeal of the engine.[w:30][next]",
									 "[noskip][effect:none]Up to CYF v0.6.1.2,[w:10] though RTL said that a lot of versions were stable,[w:10] they weren't.[w:30][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/transit_to_fight4]Besides,[w:10] there will always be a bug somewhere.[w:10] That's how coding is.[w:30][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/sad]He had a lot of problems with this,[w:10] and felt sorry for himself because he couldn't make a good,[w:10] stable engine for people to use freely...[w:30][next]",
									 "[noskip][effect:none][func:SetSprite,Mionn/happy]Thankfully,[w:10] since CYF v0.6.2,[w:10] the engine is much more stable.[w:30][next]",
									 "[noskip][effect:none]Other developers took part in the project,[w:10] and their new additions made it better than ever before.[w:30][next]",
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