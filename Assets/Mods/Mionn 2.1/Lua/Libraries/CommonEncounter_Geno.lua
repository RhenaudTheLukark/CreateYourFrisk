hit = 0
miss = 0
DET = 0

itemmenu = false

timer = 0
timer2 = 0

phase_n2_attacks = {
"nowave"
}
phase_n1_attacks = {
"sad_targetshooter",
"sad_column",
"sad_storm"
}
phase_0_attacks = {
"basic_targetshooter",
"basic_column",
"basic_storm",
"basic_spiral_hand"
}
phase_1_attacks = {
"basic_sweeper",
"advanced_column",
"advanced_storm",
"basic_bigspawner",
"advanced_spiral_hand",
"basic_spiral"
}
phase_2_attacks = {
"advanced_targetshooter",
"advanced_bigspawner",
"basic_spiral_storm",
"advanced_spiral"
}
phase_3_attacks = {
"advanced_sweeper",
"advanced_spiral_storm"
}
phase_4_attacks = {
"DET_final" --This is the finall attack, leading to a melt ending
}
phase_5_attacks = {
"FU" -- This is the special melt ending
}

function Update()
	-- if Input.Menu == 1 then
		-- DET = 75
	-- end
	if filfy_casul == true and Player.hp < 92 and dunkedon ~= true then Player.hp = 92 end
	if currentstate ~= "ITEMMENU" and itemoverlay ~= nil then --makes sure that "PAGE1"/"PAGE2" does not stay on screen when you leave
		itemoverlay.Remove()
		itemmenu = false
	end
	if currentstate == "ITEMMENU" then--the following code controls changing the page. However, since it's an act menu, the cursor sometimes moves incorrectly.
		if Input.Right == 1 and Player.absx == 321 and Player.absy == 190 and #enemies[2].GetVar("commands2") >= 1 then
			ChangePage() -- the reason for getting the amount of "commands2" is because, in Undertale, pressing left/right at certain positions changes it...
		elseif Input.Left == 1 and Player.absx == 65 and Player.absy == 190 and #enemies[2].GetVar("commands2") >= 2 then
			ChangePage() -- ...but only if there are items in the correct positions on the next page.
		elseif Input.Right == 1 and Player.absx == 321 and Player.absy == 160 and #enemies[2].GetVar("commands2") >= 3 then
			ChangePage() -- the only problem with this here is that, since the items are act commands, the cursor/soul will move around a bit whenever
		elseif Input.Left == 1 and Player.absx == 65 and Player.absy == 160 and #enemies[2].GetVar("commands2") == 4 then
			ChangePage() -- the page is changed. Currently there is no way to fix it, but it's better than nothing.
		end
	end
	timer = timer + Time.mult
	while timer >= 1 do
		timer = timer - 1
		timer2 = timer2 + 1
		RunAnimations()
		RunFlash()
		Fluctuate_Flames()
		Run_Menu_Attacks()
		if Flame_Active ~= false then
			Flame_Rotate()
		end
		if shattering == true then
			RunShattering()
		end
	end
end

function EnemyDialogueEnding()
	local attacks
	if dunkedon == true then
		attacks = {"dunkedon"}
	elseif enemies[1].GetVar('canspare') == true then -- phase -2 (SPARE)
		attacks = phase_n2_attacks
	elseif DET < -7 then -- phase -1
		attacks = phase_n1_attacks
	elseif DET < 15 then -- phase 0
		attacks = phase_0_attacks
	elseif DET < 40 then -- phase 1
		attacks = phase_1_attacks
	elseif DET < 75 then -- phase 2
		attacks = phase_2_attacks
	elseif DET < 100 then -- phase 3
		attacks = phase_3_attacks
	elseif DET >= 100 then -- phase 4
		attacks = phase_4_attacks
	end
	nextwaves = { attacks[math.random(#attacks)] } -- {""} --
end

function EnemyDialogueStarting()
	local dia
	if Right_Choice ~= true then
		Fun = 0
		Initiate_Fun(false)
	end
	if ending ~= nil then
		if ending == "flirt" then
			dia = {"[noskip][effect:none][novoice][waitall:3][func:MeltState,1]No![w:15] That's![w:15]\n[func:MeltState,2]That's impossible![w:30][next]",
			"[noskip][novoice][waitall:3][func:MeltState,3]You shouldn't...[w:20]\n[func:MeltState,4]have been able\nto...[w:30][next]",
			"[noskip][novoice][waitall:3]How...[w:15] H-How did\nyou...[w:15]\n[func:MeltState,5]W-what's h-h-h-\nhappening?![w:30][next]",
			"[noskip][novoice][waitall:3]I c-can't h-hold\n[func:MeltState,6]t-together...[w:30][next]",
			"[noskip][novoice][waitall:3]H-help![w:15] S-s-s-\nSomebody![w:15][func:MeltState,7]\nA-Anybody![w:30][next]",
			"[noskip][novoice][waitall:3]S-someone...[w:15]\n[waitall:5]H...e...l...p...[w:30][next]",
			}
		elseif ending == "DET" then
			dia = {"[noskip][effect:none][novoice][waitall:3][func:MeltState,1]How...[w:10] How d-did\nyou survive t-\nthat?[w:15]\n[func:MeltState,2]I can't...[w:15] take\nit...[w:15] anymore...[w:30][next]",
			"[noskip][novoice][waitall:3][func:MeltState,3]This... f-feeling...[w:15]\nMy b-body...[w:15] It\nc-c-can't hold t-\ntogether...[w:30][next]",
			"[noskip][novoice][waitall:3]This... hurts...\n...so...much...[w:30][next]",
			"[noskip][novoice][waitall:3][func:MeltState,4]Everyone...[w:15] I...\nI've f-f-failed\nyou...[w:30][next]",
			"[noskip][novoice][waitall:3]I'm s-sorry...[w:15]\nI'm s-so s-s-\nsorry...[w:30][next]",
			"[noskip][novoice][waitall:6][func:MeltState,5]...[w:30][next]",
			"[noskip][novoice][waitall:3]At... At l-least...\n[w:15]I c-c-can... S-see\nhim...[w:15] Once\nm-more...[w:15]\n[func:MeltState,6]W-wait f-f-for\nme...[w:30][next]",
			"[noskip][effect:shake,1.0][novoice][waitall:6][func:MeltState,7]P-Pa...[effect:shake,1.5]py...[effect:shake,2.0]rus...[effect:shake,2.5][w:15][effect:shake,3.0][w:15][effect:shake,3.5][w:15][next]",
			"[noskip][func:Destroy]"
			}
		end
	else
		if action_this_turn == "attack" then
			if betrayal ~= true then
				if enemies[1].GetVar('hp') <= 0 then
					dia = {
					"[noskip][effect:none][novoice][waitall:3]I...[w:15]\nI c-couldn't\nstop you, after\nall...[w:30][next]",
					"[noskip][effect:none][novoice][waitall:3]I...[w:15]\nI have...[w:15]\nfailed...[w:30][next]",
					"[noskip][effect:none][novoice][waitall:3]I never t-\nthought it\nwould...[w:15][waitall:4]\nhurt...\nthis...\nmuch...[w:30][next]",
					"[noskip][effect:none][novoice][waitall:3]I'm s-sorry...[w:15]\neveryone...[w:30][next]",
					"[noskip][effect:none][novoice][waitall:3]I'm...[w:15]\n[waitall:6]... sorry ...[w:60][next]",
					"[noskip][func:Destroy][next]"
					}
				else
					if spare_state == 1 then
						if hit == 1 then
							dia = {"[effect:none][func:AnimText,surprise]Y-You![w:10][func:AnimText,angry]\nWhy?![w:5]\nI t-thought you\nhad c-changed!"}
						elseif hit == 2 then
							dia = {"[effect:none][func:AnimText,sad]I still had\nhope...[w:10]\nHope that it\nc-could end...[w:10]\nWithout me\nhaving to\nspill b-blood..."}
						elseif hit == 3 then
							dia = {"[effect:none]But you leave\nme no choice,\ndo you?"}
						elseif hit == 4 then
							dia = {"[effect:none]I will not\nforgive you\nfor what you\nhave done![w:10]\nTo me.[w:5]\nTo all of us."}
						elseif hit == 5 then
							dia = {"[effect:none]There is\nno turning\nback now."}
						elseif hit >= 6 then
							if hit%2 == 0 then
								dia = {"[effect:none][func:AnimText,angry]I will not\nlet you destroy\nmore lives!"}
							else
								dia = {"[effect:none][func:AnimText,angry]I will stop\nyou, no matter\nthe cost!"}
							end
						end
					elseif spare_state == 2 then
						if hit == 1 then
							dia = {"[effect:none]Did you hope\nI would hold\nback on you?"}
						elseif hit == 2 then
							dia = {"[effect:none]You took\neverything\nfrom me...[w:10]\nDo you\nunderstand?[w:5]\nEveryting!"}
						elseif hit == 3 then
							dia = {"[effect:none][func:AnimText,sad]Just after I\nf-finally had a\np-place in this\nw-world..."}
						elseif hit == 4 then
							dia = {"[effect:none][func:AnimText,angry]You are nothing\nbut a blight\nto this world!"}
						elseif hit == 5 then
							dia = {"[effect:none]There's no one\nleft to stop\nyou now.[w:10]\nAlphys took\neveryone to\nsafety and helped\nthem escape."}
						elseif hit == 6 then
							dia = {"[effect:none][func:AnimText,disapoint]But that won't\nstop you from\nkilling everyone\nyou meet, now,\nwill it?"}
						elseif hit == 7 then
							dia = {"[effect:none]It's not just\nus, not just the\nUnderground;[w:5]\nyou'd continue\nthis on the\nsurface as\nwell."}
						elseif hit == 8 then
							dia = {"[effect:none][func:AnimText,angry]Well, I won't\nlet that happen!"}
						elseif hit == 9 then
							dia = {"[effect:none]It's up to me\nnow to stop you,\nyou horrible\ncreature!"}
						elseif hit >= 10 then
							if hit%2 == 0 then
								dia = {"[effect:none][func:AnimText,angry]I won't give\nup![w:10] Not while\nyou breathe!"}
							else
								dia = {"[effect:none][func:AnimText,angry]I will end your\nreign of terror!"}
							end
						end
					elseif spare_state == 3 then
						if hit == 1 then
							dia = {"[effect:none][func:AnimText,angry]I knew it!"}
						elseif hit == 2 then
							dia = {"[effect:none][func:AnimText,sad]For a moment\nI thought that\nyou could be...[w:5]\nThat we could..."}
						elseif hit == 3 then
							dia = {"[effect:none][func:AnimText,disapoint]But now I see\nthat there is\nno point\ntrusting you."}
						elseif hit == 4 then
							dia = {"[effect:none]You haven't\nshown mercy\nto anyone."}
						elseif hit == 5 then
							dia = {"[effect:none][func:AnimText,sad]I wanted to\nbelieve, you\nknow..."}
						elseif hit == 6 then
							dia = {"[effect:none][func:AnimText,disapoint]I wanted to\nbelieve that\nyou could change."}
						elseif hit == 7 then
							dia = {"[effect:none]I was such\na fool.[w:10]\nBut that's in\nthe past now."}
						elseif hit == 8 then
							dia = {"[effect:none]Now I really\ndon't have to\nhold back on\nyou anymore.[func:AnimText,angry]Defend yourself!"}
						elseif hit >= 9 then
							if hit%2 == 0 then
								dia = {"[effect:none][func:AnimText,angry]I won't\ngive up![w:5]\nNot until one\nof us falls!"}
							else
								dia = {"[effect:none][func:AnimText,angry]One of us\nwill fall!"}
							end
						end
					end
				end
			else
				if spare_state == 1 then
					dia = {"[effect:none][novoice]B[w:9][waitall:4]-But...[w:20]\nWhy...?[w:20] I...[w:9][waitall:3]\nI've trusted\nyou...","[func:Destroy]"}
				elseif spare_state == 3 then
					dia = {"[effect:none][novoice][waitall:4]I...[w:9] I knew it...[w:20]\n[waitall:3]You can't b[w:6]-be\ntrusted...","[func:Destroy]"}
				end
			end
		elseif action_this_turn == "spare" then
			if enemies[1].GetVar('can_be_spared') == true then
				if spare == -4 then
					dia = {"[effect:none][func:AnimText,angry]You've got some\nnerves...[w:10]\nForget about it!"}
				elseif spare == -3 then
					dia = {"[effect:none][func:AnimText,disapoint]Changing our minds\nnow, are we?[w:10]\nYou got tired\nof all that\nevildoing or\nwhat?"}
				elseif spare == -2 then
					dia = {"[effect:none][func:AnimText,angry]You were the\none who started\nall this fighting,\ndon't get\nemotional now!"}
				elseif spare == -1 then
					dia = {"[effect:none]That won't work\nwith me.[w:10]\nNot after what\nyou've done\nto us."}
				elseif spare >= 0 then
					if spare_state == 1 then
						if spare == 0 then
							dia = {"[effect:none][func:AnimText,surprise]What?![w8] You... [w:8]\nYou can't really\nthink that after\nall this..."}
						elseif spare == 1 then
							dia = {"[effect:none]You must be\nout of your\nmind."}
						elseif spare == 2 then
							dia = {"[effect:none][func:AnimText,angry]No, stop that!"}
						elseif spare == 3 then
							dia = {"[effect:none]I won't...[w:10][func:AnimText,sad]\nI won't give\nin..."}
						elseif spare == 4 then
							dia = {"[effect:none]You can't be\nserious..."}
						elseif spare == 5 then
							dia = {"[effect:none]..."}
						elseif spare == 6 then
							dia = {"[effect:none]...\n..."}
						elseif spare == 7 then
							dia = {"[effect:none][func:AnimText,sad]...\n...\n..."}
						elseif spare == 8 then
							dia = {"[effect:none][func:AnimText,sad]Why are you\ndoing this?"}
						elseif spare == 9 then
							dia = {"[effect:none][func:AnimText,disapoint]Is this some\nkind of a sick\njoke?"}
						elseif spare == 10 then
							dia = {"[effect:none][func:AnimText,disapoint]You are...[w:10][func:AnimText,standard]\nYou are serious,\naren't you...?"}
						elseif spare == 11 then
							dia = {"[effect:none]But after all\nthat you have\ndone..."}
						elseif spare == 12 then
							dia = {"[effect:none]I can't...[w:10]\n[func:AnimText,disapoint]I shouldn't..."}
						elseif spare == 13 then
							dia = {"[effect:none]..."}
						elseif spare == 14 then
							dia = {"[effect:none][func:AnimText,disapoint]No..."}
						elseif spare == 15 then
							dia = {"[effect:none][func:AnimText,sad]I just can't\ndo this..."}
						elseif spare == 16 then
							dia = {"[effect:none][func:AnimText,cry]I'm s-sorry\neveryone...[w:10]\nI-I have f-failed\nyou..."}
						elseif spare == 17 then
							Audio.Pause()
							dia = {"[effect:none][func:AnimText,cry]But I just\ncan't...[w:10]\nI c-can't just\ns-simply...[w:10]\nKill someone...",
							"[effect:none]N-not even i-if\nit m-means\nt-that...[w:15]\nThat y-you...[w:15]\nAll d-died in\n[waitall:4]v-v-vain...[w:10]","I'm s-so sorry...[w:10]\n[waitall:5]Paps..."}
							DoCanSpare()
						elseif spare > 17 then
							dia = {"[effect:none]..."}
						end
					elseif spare_state == 2 then
						if spare == 1 then
							dia = {"[effect:none]Don't try to\nfool me."}
						elseif spare == 1 then
							dia = {"[effect:none]I know you\ndon't really\nmean that."}
						elseif spare == 2 then
							dia = {"[effect:none]You already\nshowed me what\nyou are capable\nof.","I've seen your\ntrue colors."}
						elseif spare == 3 then
							dia = {"[effect:none][func:AnimText,angry]Don't think I\nwill give in\nto you!"}
						elseif spare == 4 then
							dia = {"[effect:none]..."}
						elseif spare == 5 then
							dia = {"[effect:none]...\n..."}
						elseif spare == 6 then
							dia = {"[effect:none][func:AnimText,disapoint]...\n...\n..."}
						elseif spare == 7 then
							dia = {"[effect:none][func:AnimText,disapoint]Why do you keep\ndoing this?"}
						elseif spare == 8 then
							dia = {"[effect:none]You can't fool\nme!"}
						elseif spare == 9 then
							dia = {"[effect:none]You...[w:10][func:AnimText,disapoint]It c-can't be\nthat you've\nchanged your\nmind..."}
						elseif spare == 10 then
							dia = {"[effect:none][func:AnimText,disapoint]I...[w:10]\nI can't attack\nyou if you\ndon't fight back..."}
						elseif spare == 11 then
							dia = {"[effect:none][func:AnimText,sad]Please...[w:10]\nDon't toy with\nmy feelings!"}
						elseif spare == 12 then
							dia = {"[effect:none][func:AnimText,sad]..."}
						elseif spare == 13 then
							dia = {"[effect:none][func:AnimText,sad]...\n...\n..."}
						elseif spare == 14 then
							dia = {"[effect:none][func:AnimText,sad]No...[w:10][func:AnimText,cry]No![w:6]\nStop![w:6]\nPlease!"}
						elseif spare == 15 then
							dia = {"[effect:none][func:AnimText,cry]I-I can't do\nthis anymore!"}
						elseif spare == 16 then
							dia = {"[effect:none][func:AnimText,cry]I'm s-sorry\neveryone...[w:10]\nI-I have f-failed\nyou..."}
						elseif spare == 17 then
							Audio.Pause()
							dia = {"[effect:none][func:AnimText,cry]But I just\ncan't...[w:10]\nI c-can't just\ns-simply...[w:10]\nKill someone...",
							"N-not even i-if\nit m-means t-that...[w:10]\nThat y-you...[w:10]\nAll d-died in\n[waitall:4]v-v-vain...[w:10]","I'm s-so sorry...[w:10]\n[waitall:5]Paps..."}
							DoCanSpare()
						elseif spare > 17 then
							dia = {"[effect:none]..."}
						end
					end
				end
			else
				DET = DET + 2
				Refresh_DET_Base()
				after_spare = after_spare + 1
				if after_spare == 1 then
					dia = {"[effect:none]That might have\nworked with\nToriel...\nBut not with me!"}
				elseif after_spare == 2 then
					dia = {"[effect:none]You do realise\nthat mercy's\nbeen long off\nthe table, right?"}
				elseif after_spare == 3 then
					dia = {"[effect:none]Your tricks won't\nwork on me\nanymore!"}
				elseif after_spare == 4 then
					dia = {"[effect:none]You must be way\nover your head\nif you still think\nthis can work..."}
				elseif after_spare > 4 and after_spare < 10 then
					dia = {"[effect:none]..."}
				elseif after_spare == 10 then
					dunkedon = true
					dia = {"[noskip][effect:none][waitall:2]Think that you\ncan try and spare\nme like I'm some\npawn?[w:20]\nWell you didn't\nspare my best\nfriend so get\ndunked on![w:20][next]"}
				end
				-- if spare%2 == 1 then
					-- dia = {"[effect:none]You've had your\nchance. There is\nno turning back\nnow!"}
				-- else
					-- dia = {"[effect:none]You do realise\nthat mercy is off\nthe table, right?"}
				-- end
			end
			spare = spare + 1
		elseif action_this_turn == "mock" then
			local mock = enemies[1].GetVar('mock')
			if mock == 1 then
				dia = {"[effect:none]Look who's\ntalking; the one\nwho has been\nkilling all the\ninnocent!"}
			elseif mock == 2 then
				dia = {"[effect:none]What do you\nknow about\nfriendship?!\nAll you do\nis kill people!"}
			elseif mock == 3 then
				dia = {"[effect:none]You...\nYou are right...\nI don't have\nanything left...",
				"[effect:none][func:AnimText,angry]But that also\nmeans there's\nnothing left\nto lose!"}
			elseif mock == 4 then
				dia = {"[effect:none]I will stop you,\njust you wait!",
				"[effect:none]Your killing\nspree ends\nhere!"}
			elseif mock == 5 then
				dia = {"[effect:none]Try it!\nMy life doesn't\nworth anything\nwithout those\nI loved anyway!"}
			else
				dia = {"[effect:none]Please, you can't\nsay anything\nI don't already\nknow. Quit\nwasting your\nbreath!"}
			end
		elseif action_this_turn == "brag" then
			local brag = enemies[1].GetVar('brag')
			if brag == 1 then
				dia = {"[effect:none]Lady Toriel was\nso loving and\ncaring...[w:20][func:AnimText,disapoint]\nSomething you'd\nprobably never\nunderstand."}
			elseif brag == 2 then
				dia = {"[effect:none][func:AnimText,sad]He was such a\nbright, joyous\nchild...[w:20][func:AnimText,angry]\nYou will pay\nfor this!"}
			elseif brag == 3 then
				dia = {"[effect:none]They were such\ninnocent, friendly\nfolk; they meant\nno harm.","[func:AnimText,disapoint]But what do you\ncare? All you do\nis bring pain and\nsuffering to\nwherever you go."}
			elseif brag == 4 then
				dia = {"[effect:none][func:AnimText,angry]You![w:10]\nHow dare you\nmock him?![w:10]\nHow dare you\nact like him!",
				"I'll gut you![w:10]\n[func:AnimText,angry_cry]I WILL END YOU\nFOR THIS!"}
			else
				dia = {"[effect:none][func:AnimText,angry]I will end you,\nyou horrible,\ntwisted, lunatic\npsycho!\nYour killing spree\nends here and\nnow!"}
			end
		elseif action_this_turn == "miss" then
			dia = {"[effect:none]..."}
		elseif action_this_turn == "remind" then
			remind = enemies[1].GetVar('remind')
			if remind == 1 then
				dia = {"[effect:none]I wanted to\nlearn the recipe\nfrom Toriel...[w:10]\n[func:AnimText,disapoint]But you tore\nher away from\nus. You heartless\npsycho!"}
			elseif remind == 2 then
				dia = {"[effect:none][func:AnimText,sad]He was such a\nbright, polite\nyoung man...\nStill a child,\nbut already so\nwise...","[func:AnimText,angry]You tortured him\nand then murdered\nhim in cold blood.[w:10]\nWhat kind of\ncreature are\nyou?!"}
			elseif remind == 3 then
				dia = {"[effect:none][func:AnimText,disapoint]I was an outsider.\nThey didn't have\nto treat me with\nrespect but...[w:10]\nThey did...","[func:AnimText,sad]And now they\nare all gone\nbecause of you."}
			elseif remind == 4 then
				dia = {"[effect:none]Dr. Alphys gave\nme all the answers\nI've ever needed\nabout myself.[w:10] I'll\nbe forever in her\ndebt.","[func:AnimText,disapoint]Now she is safe\nalong with all\nthe others.\nYou won't reach\nthem.[w:10] Ever."}
			elseif remind == 5 then
				dia = {"[effect:none]He didn't trust\nme. I knew that.[w:10]\nBut Sans gave me\na chance to\nprove myself...","And after...[w:10]\nBecause of what\nyou've done...[w:10]\nHe's gone too..."}
			elseif remind == 6 then
				dia = {"[effect:none][func:AnimText,sad]Papyrus was so\ninnocent...[w:10]So\njoyful, so...[w:10]\ncaring...","[func:AnimText,cry]I...[w:10] I loved him![w:15]\nHow could you...[w:15]\nWhy?!"}
			elseif remind == 7 then
				dia = {"[effect:none][func:AnimText,sad]Please...[w:10] Enough![w:10]\nDon't taint the\nmemories of my\nfriends with your\nwords anymore![w:15]\n[func:AnimText,disapoint]You are not\nworthy!"}
			else
				if remind%2 == 0 then
					dia = {"[effect:none]Haven't you tor-\nmented me enough\nalready?"}
				else
					dia = {"[effect:none]You already took\neverything from\nme."}
				end
			end
		elseif action_this_turn == "apologise" then
			apologise = enemies[1].GetVar('apologise')
			if DET < 0 then
				if apologise == 1 then
					dia = {"[effect:none]What was that?\nI couldn't hear\nyou."}
				elseif apologise == 2 then
					dia = {"[effect:none]Are you...\nAre you seriously\napologising?\nYou really mean\nthat?"}
				elseif apologise == 3 then
					dia = {"[effect:none]You can't be\ngenuine. Not\nafter all you've\ndone."}
				elseif apologise == 4 then
					dia = {"[effect:none]You... It can't be\nthat you are\nactually sorry for\nwhat you've done,\ncould you?"}
				elseif apologise == 5 then
					dia = {"[effect:none]... Please ... [w:10]\nStop it..."}
				elseif apologise == 6 then
					dia = {"[effect:none]That's too much...[w:10]\nPlease! Don't\nremind me of...[w:10][func:AnimText,sad]\nI don't want to\ngo back to...[w:10]\nthat..."}
				else
					dia = {"[effect:none]..."}
				end
			else
				if apologise == 1 then
					dia = {"[effect:none]What was that?\nSpeak up and\nfight, you\nmurderer!"}
				elseif apologise == 2 then
					dia = {"[effect:none][func:AnimText,surprise]Are you serious?\nAn apology?[w:10] After\nall you've done?[w:10]\n[func:AnimText,disapoint]You must be out\nof your mind."}
				elseif apologise == 3 then
					dia = {"[effect:none]You can't be\ngenuine. Not\nafter all you've\ndone."}
				elseif apologise == 4 then
					dia = {"[effect:none]You... It can't be\nthat you are\nactually sorry\nfor what you've\ndone, could you?"}
				elseif apologise == 5 then
					dia = {"[effect:none]... Please ... [w:10]\nStop this\nnonsense."}
				elseif apologise == 6 then
					dia = {"[effect:none][func:AnimText,angry]That's enough![w:10][func:AnimText,disapoint]\nLook, I...[w:10] I\nappreciate the\neffort. But we\nboth know you\ndon't believe a\nword you just\nsaid.",
					"[func:AnimText,angry]Don't try to fool\nme![w:10] I'm not Toriel!"}
				else
					dia = {"[effect:none]That's what I\nthought.[w:10] Let us\ncontinue, then."}
				end
			end
		elseif action_this_turn == "flirt" then
			anim_running = false
			dia = {
			"[effect:none][func:AnimText,surprise]You...[w:10]\nAre you trying\nto...[w:10]\nflirt with me?",
			"[effect:none]You...[w:10]\nYou have some\nnerves...",
			"[effect:none][func:AnimText,sad]A-After you've...[w:10]\nm-murdered my\nfriends...",
			"[effect:none][func:AnimText,sad]The person I\nl-loved...[w:10]\nThe most...",
			"[effect:none][func:AnimText,disapoint]...",
			"[effect:none][func:AnimText,angry]You...!",
			"[noskip][effect:none][func:AnimText,angry_cry][waitall:3]I WILL END YOU\nFOR [func:Liftoff]THIS!!![wait:9999]"
			}
		elseif intro == false then
			dia =
			{"[effect:none]S-Stop right\nthere, human!",
			"[effect:none]Y-You thought\nyou c-could\nget away with\nw-what you've\ndone?",
			"[effect:none]You've hurt\nso many\np-people...[w:10]\nEveryone...",
			"[effect:none]I never wanted\nthings to\nturn out this\nway, but you\nleave me no\nchoice.",
			"[noskip][func:StartFight][next]",
			"[effect:none]You already took\neverything from\nme down here.[w:10]\nIt's not like\nthere's anything\nI could lose\nanymore...",
			"[effect:none]I will avenge\nthem![w:10]\nYou are going\ndown, murderer!",
			"[noskip][func:State,ACTIONSELECT]"
			}
			intro = true
		elseif enemies[1].GetVar('canspare') == true then
			dia = {"[effect:none]..."}
		elseif Fun == 666 then
			dia = {"[noskip][effect:shake][font:wingdings]\n[voice:1] [voice:2] [voice:3]G\n\n[w:10][voice:5] [voice:2] [voice:4]A\n\n[w:10][voice:2] [voice:7] [voice:6]M\n\n[w:10][voice:5] [voice:1] [voice:3]E[w:120][func:Changed]"}
		else
			dia = {"[effect:none]..."}
		end
	end
	enemies[1].SetVar('currentdialogue',dia)
end

function DefenseEnding()
	local hp = enemies[1].GetVar('hp')
	local maxhp = enemies[1].GetVar('hpmax')
	if flash > 0 then
		CreateFlash()
	end
	if DET == 66 and FIRST_FUN ~= true and difficulty == 2 and filfy_casul ~= true then
		FIRST_FUN = true
		Fun = 666
		Initiate_Fun(true)
	end
	if action_this_turn == "attack" then
		if hit == 1 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Barely a scratch? How?"}
		elseif hit == 2 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Must be her human part.[w:10]\nDamned freak."}
		elseif hit == 3 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]I will finish you!"}
		elseif hp >= maxhp*0.75 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Keep slashing!","[starcolor:ff0000][color:ff0000]Won't last forever.","[starcolor:ff0000][color:ff0000]Destroy the freak!"}
		elseif hp >= maxhp*0.50 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Red looks good on you.","[starcolor:ff0000][color:ff0000]Getting pale?","[starcolor:ff0000][color:ff0000]Getting weaker."}
		elseif hp >= maxhp*0.25 then
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Needs more tenderising.","[starcolor:ff0000][color:ff0000]Red looks good on you."}
		else
			randomencountertext = {"[starcolor:ff0000][color:ff0000]Barely standing."}
		end
		encountertext = randomencountertext[math.random(#randomencountertext)]
	elseif action_this_turn == "spare" then
		if enemies[1].GetVar('can_be_spared') == true then
			if spare < 10 then
				randomencountertext = {"Mionn doesn't believe you yet.","She needs more convincing."}
			elseif spare < 15 then
				randomencountertext = {"Mionn is starting to give in.","She still needs more convincing"}
			elseif spare <= 18 then
				randomencountertext = {"Mionn is about to give in.","She's losing the will to fight"}
			else
				randomencountertext = {"Mionn gave up."}
			end
			encountertext = randomencountertext[math.random(#randomencountertext)]
		else
			encountertext = "Won't trust you anymore."
		end
	else
		if enemies[1].GetVar('canspare') == true then
			encountertext = "Mionn gave up."
		else
			if DET < -7 then
				randomencountertext = {"Mionn is breaking down.","Mionn barely wants to stop you\ranymore."}
			elseif DET < -3 then
				randomencountertext = {"Mionn is starting to break down.","Mionn still wants to stop you."}
			elseif DET < 0 then
				randomencountertext = {"Mionn is surprised at you.","She seems a bit confused."}
			else
				if hp >= maxhp*0.75 then
					randomencountertext = {"Mionn is standing firm.","Mionn will stop you."}
				elseif hp >= maxhp*0.50 then
					randomencountertext = {"Mionn is a bit shaken.","Mionn seems weaker, but more\rdetermined."}
				elseif hp >= maxhp*0.25 then
					randomencountertext = {"She's starting to lose a\rlot of blood.","Mionn is shaking, but getting\rmore and more determined."}
				else
					randomencountertext = {"She's almost dead.","Mionn is filled with\rdetermination.","It will be over soon."}
				end
			end
			encountertext = randomencountertext[math.random(#randomencountertext)]
		end
	end
	action_this_turn = nil
	if enemies[1].GetVar('canspare') == true then
		enemies[1].SetVar('commands',{"Check","Flirt"})
	elseif DET < -10 then
		enemies[1].SetVar('commands',{"Check","Remind","Brag","Mock","Apologise","Flirt"})
	else
		enemies[1].SetVar('commands',{"Check","Remind","Brag","Mock","Apologise"})
	end
end

spare = 0
after_spare = 0

spare_state = 0 -- 0 = didn't do anything; 
-- 1 = started sparing before hitting;
-- 2 = started hitting before sparing;
-- 3 = started hitting, than sparing, then hitting agian

function HandleSpare()
	if enemies[1].GetVar('can_be_spared') == true then
		hit = 0
		DET = DET - 1
	end
	if spare_state == 0 and enemies[1].GetVar('can_be_spared') == true then
		spare_state = 1
	end
	spared_before = true
	action_this_turn = "spare"
    State("ENEMYDIALOGUE")
end

function DoCanSpare()
	enemies[1].SetVar('canspare',true)
	enemies[1].SetVar('def',-666666666)
	cry_anim = true
	Animate(Anim_From_Fight)
end

function Liftoff()
	liftoff = 1
	flash = 200
	Encounter.Call("CreateFlash")
	Audio.PlaySound("flash")
end

function Kill()
	enemies[1].Call("Kill")
end

function LegHero()
	enemies[1].SetVar("def", enemies[1].GetVar("def") - 4 )
end

-- MOSTLY ITEM MENU STUFF

function ChangePage() -- changes the pages. Check "items.lua" in the Monsters folder and scroll to the bottom to see this in action.
	enemies[2].Call("SwapTables")
	itemoverlay.Remove()
	State("ITEMMENU")
end

function OnHit(bullet)
	if bullet.GetVar("safe") == nil then -- so that the "PAGE 1"/"PAGE 2" does not hurt you
		Player.Hurt(1,1/30)
	end
end

function ActivateDummy(bool)
	enemies[2].Call("SetActive",bool)
end

function EnteringState(newstate,oldstate) -- Thanks lvkuln for this amazing function! This wouldn't be possible without it.
	currentstate = newstate
	if newstate == "DEFENDING" then
		if limit ~= true and DET >= 100 and DET < 200 then
			BattleDialog({"[noskip][novoice][starcolor:ff0000][color:ff0000][waitall:2]She has reached her [func:Liftoff]limit."})
			limit = true
		end
		if ending == "flirt" and shattering ~= true then
			CreateFlash()
			BattleDialog({"[noskip][novoice][starcolor:ff0000][color:ff0000][waitall:4]But nobody came.[w:60][func:Destroy]"})
		end
	end
	if oldstate == "DEFENDING" then
		if Flame_Active ~= false then
			Flame_Refresh()
			Flame_mid_y = 300
		end
		if action_this_turn == "flirt" then
			ending = "flirt"
			BattleDialog({"[novoice][starcolor:ff0000][color:ff0000]The deed is done."})
		elseif DET >= 100 then
			ending = "DET"
			BattleDialog({"[novoice][starcolor:ff0000][color:ff0000]The deed is done."})
		end
	end
	if newstate == "DEFENDING" or newstate == "ENEMYDIALOGUE" or newstate == "ATTACKING" then
		in_menus = false
		Remove_Menu_Attacks()
	end
	if newstate == "ACTIONSELECT" and ending == nil then
		in_menus = true
	end
	if newstate == "MERCYMENU" and Fun == 666 then
		Fun_Spec_Fafic()
		State("ACTIONSELECT")
	end
	if newstate == "ITEMMENU" then -- if the player selected "ITEMS"
		if Fun == 666 then
			Fun_Spec_Fafic()
			State("ACTIONSELECT")
		else
			if itemmenu == false then -- if it's the first time they're going in (because it technically also goes to this state when you change page)
				local commands = enemies[2].GetVar("commands")
				local commands2 = enemies[2].GetVar("commands2")
				if #commands <=3 and #commands2 > 0 then --This block will search for missing slots in page 1 and fill them in with items from page 2.
					for i=1,4-#commands do
						if #commands2 > 0 then
							local item = commands2[1]
							table.remove(commands2,1)
							table.insert(commands,item)
						end
					end
				end
				enemies[2].SetVar("commands",commands) --puts the "balanced" command tables into the monster file.
				enemies[2].SetVar("commands2",commands2)
			end
			if #enemies[2].GetVar("commands") + #enemies[2].GetVar("commands2") > 0 then -- if there are any items there
				enemies[1].Call("SetActive",false)
				enemies[2].Call("SetActive",true)
				itemmenu = true
				local alt = 0
				if enemies[2].GetVar("alt")%2 ~= 0 then --this bit gets which page the items menu is on.
					alt = 2
				else
					alt = 1
				end
				itemoverlay = CreateProjectileAbs("items_"..alt,320,240) -- creates the overlay reading "Page 1" or "Page 2"
				itemoverlay.SetVar("safe",1)
				State("ACTMENU")
			else
				State("ACTIONSELECT") --You can't enter the item menu if you don't have any items.
			end
		end
	elseif newstate == "ENEMYDIALOGUE" then --Since it always happens right after BATTLEDIALOG, we can use it to reset the tables.
		local alt = enemies[2].GetVar("alt")
		if alt%2 ~= 0 then
			enemies[2].Call("SwapTables")
		end
	elseif oldstate == "ACTMENU" then -- if the player is leaving the "item menu" (it's actually an act menu) then
		enemies[1].Call("SetActive",true)
		enemies[2].Call("SetActive",false)
		if itemmenu == true and newstate == "ENEMYSELECT" then --if this code wasn't here, pressing Z within the item menu would display the item monster.
			itemmenu = false
			State("ACTIONSELECT")
		end
	end
end