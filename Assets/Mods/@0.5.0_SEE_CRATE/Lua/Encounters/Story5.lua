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
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
                                     "[noskip][effect:none]Time has flown by.[w:15] Mentalities have changed.[w:30][next]",
                                     "[noskip][effect:none]Back then,[w:10] when I first wrote this message,[w:10] I thought I was close to the end.[w:30][next]",
                                     "[noskip][effect:none]But now,[w:10] after more than 3 years of work on this engine,[w:10][func:Animate,sad] I realize that it won't ever happen.[w:30][next]",
                                     "[noskip][effect:none]Even if this engine will never be completely finished,[w:10] completely perfect...[w:30][next]",
                                     "[noskip][effect:none][func:Animate,smile]I still hope you'll have a great time with it.[w:30][next]",
                                     "[noskip][effect:none][func:Animate,normal]I could have stopped everything and quit as Unitale's creator did...[w:30][next]",
                                     "[noskip][effect:none][func:Animate,happy]But I am still here.[w:30][next]",
                                     "[noskip][effect:none]And it's all thanks to everyone around me.[w:15][func:Animate,smile] Including YOU,[w:10] who is currently running this engine.[w:30][next]",
                                     "[noskip][effect:none][func:Animate,normal]Without all of you,[w:10] I'd have stopped a long time ago,[w:10][func:Animate,smile] and I'm very proud about giving you this new version of Create Your Frisk.[w:30][next]",
                                     "[noskip][effect:none][func:Animate,normal]Maybe this message might be a little bit...[w:15][func:Animate,sad]odd[w:5] for you.[w:30][next]",
                                     "[noskip][effect:none]But...[w:15]it's my way to[w:5][func:Animate,smile] express my gratitude.[w:15] For making this engine an experience that lived much longer than I hoped it would.[w:30][next]",
                                     "[noskip][effect:none][func:Animate,normal]So,[w:10] all in all...[w:30][next]",
                                     "[noskip][effect:none][func:Animate,happy]Thank you.[w:30][next]",
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