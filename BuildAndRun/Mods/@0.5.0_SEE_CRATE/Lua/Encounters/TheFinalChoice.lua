if not GetRealGlobal("ow") then error("You really should try to access these encounters the normal way... Here is a clue: you need to move the dog in the map test2 using an event. Now good luck!") end

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
    enemies[2]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:60][next]",
                                	 "[noskip][func:Animate, smile]Here we are.[w:40][next]",
									 "[noskip][func:Animate, normal]So,[w:15] what will you do now?[w:40][next]",
									 "[noskip][func:Animate, angry]Will you attempt to fight me?[w:40][next]",
									 "[noskip][func:Animate, happy]Or will you leave me alone?[w:40][next]",
									 "[noskip][func:Animate, normal][func:Unpause]It's your choice,[w:15] now.[func:SetBubble, leftwide][func:GetCloser][w:40][next]",
									 "[noskip][func:Animate, lookright][waitall:5]...[waitall:1]wait.[w:40][next]",
									 "[noskip][func:Animate, lookrightsmile][func:Pause]Hey little buddy,[w:15] come here![w:40][next]",
									 "",
									 "[noskip][func:Animate, lookrightsmile]Of course![w:20] Don't worry,[w:15] we won't hurt you![w:40][next]",
									 "",
									 "[noskip][func:LaunchAnim]There,[w:15] there.[w:40][next]",
									 "[noskip]I'd like to thank WD200019 for improving these little encounters.[w:40][next]",
									 "[noskip]It wasn't much,[w:15] but I like these changes a lot!",
									 "[func:EndAnim][func:Animate, lookbottomrightsmile]Good, go join the others, I'll be back in a minute.",
									 "[func:GetFurther]",
                                	 "[func:Animate, smile]What was I saying again?[w:20] Ah,[w:15] yes.",	
									 "[noskip][func:Animate, normal][func:Unpause]It's your choice,[w:15] now.[w:40][next]",						 
									 "[func:State, DEFENDING][next]"}
    enemies[3]["currentdialogue"] = {"", "", "", "", "", "", "", "", 
	                                 "[noskip][func:Animate, A2]Who?[w:20] Me?[w:40][next]", "[func:SetBubble, top]",
									 "[noskip][func:Animate, A3]Yeah![w:20][func:GetCloser2] Here I come![w:40][next]", "[func:SetBubble, toptiny]", "", "", "",
									 "[noskip][func:Animate, A4]Okay![w:40][next]"}
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
	    if enemies[3]["monstersprite"].absx != 579 then  enemies[3]["monstersprite"].absx = enemies[3]["monstersprite"].absx - 1
		else                      		                 getcloser1 = false
		end
	end
    if getcloser2 then
	    if enemies[3]["monstersprite"].absx != 363 then  enemies[3]["monstersprite"].absx = enemies[3]["monstersprite"].absx - 2
		else		                                     getcloser2 = false
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
	if (fade.alpha * 1000) % 1000 != 1000 and (fade.alpha * 1000) % 1000 != 0 then
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