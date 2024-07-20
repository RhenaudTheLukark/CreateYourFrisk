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

enemies = {"punderbolt", "punderbolt"}
enemypositions = { { 0, 0 }, { 0, 0 } }

possible_attacks = {"thechoice"}

function EncounterStarting()
	Audio.Stop()
	fade = CreateSprite("black", "Top")
	fade.x = 320
	fade.y = 240
	fade.Scale(640, 480)
	enemies[1].Call("SetSprite", "Punderbolt/normal")
	enemies[2].Call("SetBubbleOffset", {0, 30})
	enemies[2]["currentdialogue"] = {"[noskip][func:LaunchFade, true][w:30][next]",
									 "[noskip][effect:none][func:Animate, smile]Here we are.[w:30][next]",
									 "[noskip][effect:none][func:Animate, normal]Now that we're together,[w:10] what will you do?[w:30][next]",
									 "[noskip][effect:none][func:Animate, angry]Will you attempt to fight me?[w:30][next]",
									 "[noskip][effect:none][func:Animate, happy]Or will you leave me alone?[w:30][next]",
									 "[noskip][effect:none][func:Animate, normal][func:Unpause]It's your choice,[w:10] now.[w:30][next]",
									 "[func:State, DEFENDING][next]"}
	enemies[1]["randomdialogue"] = {""}
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
	if (beginfade or endfade) and Time.time - currentTime >= 1/3 then
		alphaup = endfade
		endfade = false
		beginfade = false
		if alphaup then fade.alpha = fade.alpha + Time.dt
		else  			fade.alpha = fade.alpha - Time.dt
		end
	end
	if (fade.alpha * 1000) % 1000 ~= 1000 and (fade.alpha * 1000) % 1000 ~= 0 then
		if alphaup then fade.alpha = fade.alpha + Time.dt
		else  			fade.alpha = fade.alpha - Time.dt
		end
		if fade.alpha > 1 then fade.alpha = 1 end
		if fade.alpha < 0 then fade.alpha = 0 end
		if white and (fade.alpha * 1000) % 1000 > 500 then enemies[1].Call("Kill") enemies[2].Call("Kill") end
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