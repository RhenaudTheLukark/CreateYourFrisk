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

enemies = {"lukark"}
enemypositions = {{0, 20}}

possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	oldname = Player.name
	Player.name = "FRISKY"
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	fade.alpha = 1
    require "Animations/lukark_anim" 
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:60][next]",
                                	 "[noskip][func:Animate,smile]But in the end, it's not that important.[w:60][next]",
									 "[noskip]Now, we're close to 1.0's release.[w:60][next]",
									 "[noskip][func:Animate,normal]There's not much left to do, now.[w:60][next]",
									 "[noskip]We're close to the end.[w:60][next]",
									 "[noskip][func:Animate,sad]I could have abandoned this a lot of times...[w:60][next]",
									 "[noskip]I could have stopped everything and quit as lvk\ndid...[w:60][next]",
									 "[noskip][func:Animate,normal]But I'm still here.[w:60][next]",
									 "[noskip][func:Animate,smile]Thanks to my friends.[w:60][next]",
									 "[noskip]Thanks to my testers.[w:60][next]",
									 "[noskip][func:Animate,happy]Thanks to you all.[w:60][next]",
									 "[noskip][func:Animate,smile]Without all of you, I'd have stopped a long time ago, and I'm very proud about giving you this new version of Create Your Frisk.[w:60][next]",
									 "[noskip][func:Animate,normal]For you, this message may not be very much...[w:60][next]",
									 "[noskip]But for me...[w:60][next]",
									 "[noskip][func:Animate,happy]It's everything.[w:60][next]",
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
    AnimateLukark()
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