remind = 0
brag = 0
mock = 0
apologise = 0

can_be_spared = true

function HandleAttack(attackstatus)
	if attackstatus == -1 then
		Encounter["miss"] = Encounter["miss"] + 1
		Encounter["action_this_turn"] = "miss"
	else
		Encounter["hit"] = Encounter["hit"] + 1
		Encounter["spare"] = Encounter["spare"] - 2
		if hp > hpmax*0.75 then
			Encounter["DET"] = Encounter["DET"] + 2
		elseif hp > hpmax*0.5 then
			Encounter["DET"] = Encounter["DET"] + 3
			Encounter["health_state"] = 1
		elseif hp > hpmax*0.25 then
			Encounter["DET"] = Encounter["DET"] + 4
			Encounter["health_state"] = 2
		else
			Encounter["DET"] = Encounter["DET"] + 5
			Encounter["health_state"] = 3
		end
		Encounter.Call("LoadAnims")
		if canspare == true then
			Encounter.Call("Animate",Encounter['Anim_Bet'])
			Encounter['betrayal'] = true
			Audio.Pause()
		elseif hp ~= 0 then
			Encounter.Call("Animate",Encounter['Anim_Hurt'])
		else
			Encounter.Call("Animate",Encounter['Anim_Collapse'])
			Encounter["final_anim"] = true
		end
		Encounter["action_this_turn"] = "attack"
		if Encounter["spare_state"] == 0 then
			Encounter["spare_state"] = 2
		elseif Encounter["spare_state"] == 1 then
			can_be_spared = false
		elseif Encounter["spare_state"] == 2 and can_be_spared == true and Encounter["spared_before"] == true then
			can_be_spared = false
			Encounter["DET"] = 15
			Encounter["spare"] = 0
			Encounter["spare_state"] = 3
		end
		if Encounter["hit"] > 2 then
			can_be_spared = false
		end
	end
	Encounter.Call("Refresh_DET_Base")
end

function HandleCustomCommand(command)
	Encounter["action_this_turn"] = "act"
	Encounter["in_menus"] = false
	Encounter.Call("Remove_Menu_Attacks")
	if command == "CHECK" then
		DET = Encounter.GetVar('DET')
		if canspare == true then
			BattleDialog({"MIONN - ATK 20 DEF 20 DET "..DET.."\nShe gave up.\nShe's vulnerable now."})
		elseif Encounter['spare'] > 0 and can_be_spared == true then
			BattleDialog({"MIONN - ATK 20 DEF 20 DET "..DET.."\nHuman-monster hybrid.\nCan still be saved."})
		else
			BattleDialog({"MIONN - ATK 20 DEF 20 DET "..DET.."[color:ff0000]\nSo weak, yet so pesky. A freak\rof nature. Destroy it!"})
		end
	elseif command == "FLIRT" then
		Encounter["action_this_turn"] = "flirt"
		BattleDialog({"You attempt to flirt with\rMionn.\nShe seems unsettled."})
	elseif command == "MOCK" then
		if mock == 0 then
			can_be_spared = false
			Encounter["DET"] = Encounter["DET"] + 15
			BattleDialog({"With a mocking smile, you tell\rher what a grotesque freak she\ris."})
		elseif mock == 1 then
			Encounter["DET"] = Encounter["DET"] + 12
			BattleDialog({"With an evil grin you tell\rher that her friends\rnever loved her."})
		elseif mock == 2 then
			Encounter["DET"] = Encounter["DET"] + 9
			BattleDialog({"You remind Mionn that there's\rnothing left for her in this\rworld."})
		elseif mock == 3 then
			Encounter["DET"] = Encounter["DET"] + 6
			BattleDialog({"You tell her that she won't\rstand a chance against you.[w:15]\nJust like her friends."})
		elseif mock == 4 then
			Encounter["DET"] = Encounter["DET"] + 3
			BattleDialog({"With an evil laugh, you tell\rher that there's one more\rthing you can rip from her."})
		else
			BattleDialog({"You try to say something evil,\rbut Mionn stops you."})
		end
		Encounter["action_this_turn"] = "mock"
		mock = mock + 1
	elseif command == "BRAG" then
		if brag == 0 then
			can_be_spared = false
			Encounter["DET"] = Encounter["DET"] + 8
			BattleDialog({"You tell Mionn that she'll\rfall just as easily as\rthat stupid goat from the Ruins."})
		elseif brag == 1 then
			Encounter["DET"] = Encounter["DET"] + 12
			BattleDialog({"You mockingly parodise how\rSkeeter screamed while you\rpulled his legs out one by one."})
		elseif brag == 2 then
			Encounter["DET"] = Encounter["DET"] + 16
			BattleDialog({"You start to count your kills\ron your fingers, starting\rwith the folks of Snowdin:",
			"Chilldrake,[w:10] Ice Cap,[w:10] Gyftrot,[w:10]\rthe Canine Unit..."})
		elseif brag == 3 then
			Encounter["DET"] = Encounter["DET"] + 20
			BattleDialog({"With an evil grin, you strike\ra pose and let out a\rdistorted chuckle.[w:20] [color:ff0000]NYEH heh Heh!"})
		else
			BattleDialog({"You try to torment her some\rmore, but Mionn jumps at you\rmid-setence. She's had enough."})
		end
		Encounter["action_this_turn"] = "brag"
		brag = brag + 1
	elseif command == "REMIND" then
		if remind == 0 then
			can_be_spared = false
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"You ask Mionn if she could\rsmell cinnamon and butterscotch."})
		elseif remind == 1 then
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"With a light chuckle, you remind\rMionn how Skeeter always creeped\reveyone out crawling on walls."})
		elseif remind == 2 then
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"You remind her of the good\rtimes she had in Snowdin town."})
		elseif remind == 3 then
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"You begin to hum a familiar\rtune.\nIt sounds like an anime opening."})
		elseif remind == 4 then
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"You put your hands into your\rpocket and grin at Mionn,\rcracking a [color:ff0000]deadly[color:ffffff] pun.","She fails to laugh."})
		elseif remind == 5 then
			Encounter["DET"] = Encounter["DET"] + 3
			def = def - 5
			BattleDialog({"Putting your hand on your waist,\ryou let out a joyful chuckle.","Mionn averts her eyes.\nShe looks heartbroken."})
		else
			BattleDialog({"You try to grasp another joyful\rmemory, but Mionn stops you."})
		end
		Encounter["action_this_turn"] = "remind"
		remind = remind + 1
	elseif command == "APOLOGISE" then
		if apologise == 0 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"With a little hesitation,\ryou murmur an apology."})
		elseif apologise == 1 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"You repeat your apology a\rlittle louder. Mionn stops\rfor a brief moment."})
		elseif apologise == 2 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"With shaking hands, you\rapologise again for your\rdeeds."})
		elseif apologise == 3 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"You try to pour your heart\rinto your apology."})
		elseif apologise == 4 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"With a trembling voice, you\rburst out a heartfelt, genuine\rapology. [w:10]Or so you wish."})
		elseif apologise == 5 then
			Encounter["DET"] = Encounter["DET"] - 1
			BattleDialog({"You start to count your\revil deeds, apologising for\rthem one by one.","As you reach Snowdin, Mionn\rabruptly interrputs your rant."})
		else
			BattleDialog({"You try to say something\relse as an apology, but\ryou ran out of ideas."})
		end
		Encounter["action_this_turn"] = "apologise"
		apologise = apologise + 1
	elseif command == "LIMIT" then
		Encounter["DET"] = 100
		State("ENEMYDIALOGUE")
	elseif command == "FUN" then
		Encounter["DET"] = 66
		State("ENEMYDIALOGUE")
	elseif command == "MENU" then
		Encounter["DET"] = 75
		State("ENEMYDIALOGUE")
	end
	Encounter.Call("Refresh_DET_Base")
end

function StartFight()
	Encounter.Call("StartFight")
	Audio.Unpause()
end

function OnDeath()
	Audio.Pause()
	Encounter.Call('Flame_Remove')
	if canspare == true then
		Encounter['shatter_soul_y'] = 320
		State("ENEMYDIALOGUE")
	else
		BattleDialog({"[novoice][starcolor:ff0000][color:ff0000]The deed is done."})
	end
end

function AnimText(input)
	SetSprite("Stances"..Encounter["health_state"].."/"..input)
end

function MeltState(input)
	SetSprite("Final//melt"..Encounter['melt_type']..""..input)
end

function Liftoff()
	Encounter['liftoff'] = 1
	Encounter['flash'] = 200
	Encounter.Call("CreateFlash")
	Audio.PlaySound("flash")
end

function Destroy()
	Encounter.Call("ActivateDummy",true)
	Encounter.Call("SetupShatter")
	Encounter.SetVar('nextwaves',{"nowave"})
	State("DEFENDING")
	Audio.Pause()
	Kill()
end

require "Libraries/Funionn"