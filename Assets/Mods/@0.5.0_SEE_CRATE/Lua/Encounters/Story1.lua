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

enemies = {"exrumia"}
enemypositions = {{0, 0}}

possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	oldname = Player.name
	Player.name = "FRISKY"
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	require "Animations/exrumia_anim"
	enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
	                                 "[noskip]If you came here,[w:10] it must be for a good reason.[w:30][next]",
									 "[noskip]We'll tell you the story of Create Your Frisk.[w:30][next]",
									 "[noskip]In February 2016,[w:10] RhenaudTheLukark,[w:10] the creator of Create Your Frisk decided,[w:10] with some of their friends,[w:10] to create their own AU.[w:30][next]",
									 "[noskip]They concluded that Unitale was the easiest way to do it.[w:30][next]",
									 "[noskip]If we had told him what would have happened,[w:10] he'd have laughed at it.[w:30][next]",
									 "[noskip][func:LaunchFade, false][w:35][func:State,DONE]"}
	require "Waves/bullettest_touhou"
    State("ENEMYDIALOGUE")
end

--[[If you came here, this is for a good reason.
    We'll tell you the story of Create Your Frisk.
	In February, RhenaudTheLukark, the creator of Create Your Frisk decided, with some of their friends, to create their own AU.
	They concluded that Unitale was the easiest way to do it.
	If we had told him what would have happened, he'd have laughed at it.
	~~~~~~~~~~
	But, after some time playing around with the engine, he found out that lots of features were missing.
	First of all, there was no overworld: to make an AU with his friends, RhenaudTheLukark needed one.
	Thus, after contacting with lvkuln, he got access to the sources a bit before the official release of the open-source version.
	Having been a complete beginner in Unity, he first tried to fix the bugs related to the open-source version of the engine... and succeeded.
	~~~~~~~~~~
	After fixing these bugs, he felt something new, as if he did something extraordinary: this was at this moment he knew he'd have to resume lvkuln's work.
	fade.alpha = 1
    require "Animations/exrumia_anim"
    enemies[1]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:60][next]",
	                                 "[noskip]If you came here,[w:20] it is for a good reason.[w:60][next]",
	He then tried to contact the original developer, but in vain: it was already too late. IRL stuff took the latter away from the project.
	Thus, he tried to create something on his own, even if he had never followed any tutorial on using Unity or even a C# tutorial.
	~~~~~~~~~~
	After two months of hard work, RhenaudTheLukark released CYF 0.1.
	He did every possible thing to keep secret of the incoming engine, but was too excited about showing it to the world.
	This new engine wasn't very well known, as the official Unitale 0.2.1a version was still used by a good part of the community.
	The only new thing was the overworld system, but it was unusable without giving away the sources.
	Then, the developer decided to extend his engine to make it more useful for the community.
	~~~~~~~~~~
	However, the next releases of Create Your Frisk weren't very successful, despite a lot of new features, tweaks and functions.
	The problem was that all the versions had at least one bug that greatly reduced the appeal of the engine.
	Even today, with the 0.5.0, though RTL said that 0.4.4.4 was stable, it wasn't. Besides, there will always have a bug somewhere, that's how coding works.
	He had a lot of problems about this, as feeling sorry for himself because he couldn't make a good engine for people to use without any bug...
	~~~~~~~~~~
	But in the end, it's not that important.
	Now, we're close to 1.0's release.
	There's not much to do, now.
	We're close to the end.
	I could have abandoned a lot of times...
	I could have stopped everything and quit as lvk did thousand times...
	But I'm still here.
	Thanks to my friends.
	Thanks to my testers.
	Thanks to you all.
	Without all of you, I'd have stopped a long time ago, and I'm very proud about giving you this new version of Create Your Frisk.
	For you, this message may not be very much...
	But for me...
	It's everything.
	]]

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
    AnimateExRumia()
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