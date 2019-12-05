if not GetRealGlobal("ow") then error("You really should try to access these encounters the normal way...\n\nHere is a clue: You should try talking to the dog.\n\nNow good luck!", 0) end

encountertext = "Poseur strikes a pose!" --Modify as necessary. It will only be read out in the action select screen.
nextwaves = {"thechoice"}
wavetimer = math.huge
arenasize = {155, 130}
autolinebreak = true
unescape = true

currentTime = Time.time
beginfade = false
endfade = false
alphaup = false
count = 0
white = false
getcloser1 = false
getcloser2 = false
getfurther = false

enemies = {"punderbolt", "punderbolt", "wdspecial"}
enemypositions = {{0, 0}, {0, 0}, {379, 0}}

possible_attacks = {"thechoice"}

function EncounterStarting()
	Audio.Stop()
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	fade.alpha = 1
    enemies[1].Call("SetSprite", "Punderbolt/normal")
    enemies[2]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
                                     "[noskip][func:Animate, smile]Here we are.[w:30][next]",
                                     "[noskip][func:Animate, normal]Now that we're together,[w:10] what will you do?[w:30][next]",
                                     "[noskip][func:Animate, angry]Will you attempt to fight me?[w:30][next]",
									 "[noskip][func:Animate, happy]Or will you leave me alone?[w:30][next]",
                                     "[noskip][func:Animate, normal][func:Unpause]It's your choice,[w:10] now.[func:SetBubble, leftwide][func:GetCloser][w:40][next]",
									 "[noskip][func:Animate, lookright][waitall:5]...[waitall:1]wait.[w:30][next]",
									 "[noskip][func:Animate, lookrightsmile][func:Pause]Hey little buddy,[w:10] come here![w:30][next]",
                                     "",
                                     "[noskip][func:Animate, lookrightsmile]Of course![w:15] Don't worry,[w:10] we won't hurt you![w:30][next]",
                                     "", "",
                                     "[noskip][func:LaunchAnim]There,[w:10] there.[w:30][next]",
                                     "[noskip]I'd like to thank WD200019 here for improving these little encounters...[w:30][next]",
                                     "[noskip]...[w:10]and for being my best contributor and friend.",
                                     "[noskip][func:EndAnim]Since they joined the team,[w:10] they overall morphed CYF into a much nicer program.[w:30][next]",
                                     "[noskip][func:Animate, lookrightsmile]Do you have anything to say,[w:10] buddy?[w:30][next]",
                                     "", "", "", "",
                                     "[noskip][func:Animate, happy]That's true,[w:10] and now you're an official member of the team![w:30][next]",
                                     "[noskip][func:Animate, lookbottomrightsmile]Anyway,[w:10] thank you.[w:15] Go join the others,[w:10] I'll be back in a minute.[w:30][next]",
									 "[func:GetFurther]",
                                     "[noskip][func:Animate, smile]What was I saying again?[w:15] Ah,[w:10] yes.[w:30][next]",
									 "[noskip][func:Animate, normal][func:Unpause]It's your choice,[w:10] now.[w:30][next]",
									 "[func:State, DEFENDING][next]"}
    enemies[3]["currentdialogue"] = {"", "", "", "", "", "", "", "",
	                                 "[noskip][func:Animate, A2]Who?[w:15] Me?[w:30][func:SetBubble, top][next]",
                                     "", "[noskip] [next]",
									 "[noskip][func:Animate, A3]Yeah![w:15][func:GetCloser2] Here I come![w:40][func:SetBubble, rightwide][next]",
                                     "", "", "", "", "",
									 "[noskip][func:Animate, A6]Aww...[w:10]but it wasn't all me![w:15] I needed your help too![w:30][next]",
									 "[noskip][func:Animate, A5]I started out as a modder,[w:10] then I learned more and more about how the engine works.[w:30][next]",
									 "[noskip]I had an idea for a new UI for CYF,[w:10] and you wanted me to code it for real![w:30][next]",
									 "[noskip][func:Animate, A6]Then I added features and fixes over time,[w:10] until you decided to release them all![w:30][next]",
                                     "[noskip][func:Animate, A7][func:SetBubble, toptiny]", "",
									 "[noskip][func:Animate, A4]Okay![w:60][next]"}
	enemies[1]["randomdialogue"] = {""}
	enemies[3]["randomdialogue"] = {""}
	require "Waves/bullettest_touhou"
    State("ENEMYDIALOGUE")
end

function LaunchFade(begin, whitee)
    if whitee == nil then whitee = false end
    white = whitee
    if whitee then fade.Set("white") end
    if begin then
	    beginfade = true
	    fade.alpha = 1
	else
	    endfade = true
	    fade.alpha = 0
    end
end

function Update()
    if getcloser1 then
	    if enemies[3]["monstersprite"].absx ~= 579 then enemies[3]["monstersprite"].absx = enemies[3]["monstersprite"].absx - 1
		else                      		                getcloser1 = false
		end
	end
    if getcloser2 then
	    if enemies[3]["monstersprite"].absx ~= 363 then enemies[3]["monstersprite"].absx = enemies[3]["monstersprite"].absx - 2
		else		                                    getcloser2 = false
		end
	end
    if getfurther then
	    if enemies[3]["monstersprite"].absx < 700 then  enemies[3]["monstersprite"].absx = enemies[3]["monstersprite"].absx + 2
		else                         		            getfurther = false
		end
	end
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
		if white and (fade.alpha * 1000) % 1000 > 500 and enemies[1]["name"] == "Punderbolt" then enemies[1].Call("Kill") enemies[2].Call("Kill") end
	elseif white then
	    count = count + 1
		if count == 30 then
		    SetAlMightyGlobal("CrateYourFrisk", true)
		    Misc.DestroyWindow()
		end
	end
end

function EnemyDialogueStarting() end

function EnemyDialogueEnding() end

function DefenseEnding() encountertext = RandomEncounterText() end

function HandleSpare() State("ENEMYDIALOGUE") end

function HandleItem(ItemID) BattleDialog({"Selected item " .. ItemID .. "."}) end