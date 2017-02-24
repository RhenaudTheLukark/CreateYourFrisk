comments = {"Smells like a small loan\rof a million dollars.", "Trump is urging you to vote\rfor him.", "Trump's face does...\nSomething.", "Trump's golden hair flows in\rthe wind.", "Trump is deep in thought[w:5].[w:5].[w:5].[w:5]\nNevermind! He was just sleeping."}
angrycomments = {"Smells like a revolution.", "You feel money crawling on your\rback.", "Trump's face looks...\nWell, it won't be graced with\ra description.", "Trump seems to be thinking\rabout your destruction."}
commands = {"Vote", "Dumben", "Insult", "Resist"}
randomdialogue = {"We're all a little\nchubby.", "We have to have\na wall!\nWe have to have\na border!", "I mean, part of my\nbeauty is that I'm\nvery rich.", "Tiny children are not\nhorses!", "I whine and whine until\nI win."}
ragedialogue = {"[effect:none]Prepare yourself.", "[effect:none]I've already won...", "[effect:none]Give up now..."}
insultdialogue = {"You told Trump that he will\rnever be able to win.", "You told Trump that he's\rracist.", "You told Trump that his whole\rcampaign was a joke."}

sprite = "trumptestic" 
name = "Donald Trump"
hp = 2
atk = 3
def = 99
xp = 2
gold = 1000000
Player.name = "USA"
check = "Do not insult his hair."
angrycheck = "That might've been a mistake."
dialogbubble = "rightwide"

SetGlobal("rage",0)
SetGlobal("sell",0)
SetGlobal("spareable",0)
hpold = 300

function HandleCustomCommand(command)
	local rage = GetGlobal("rage")
	local sell = GetGlobal("sell")
    if command == "VOTE" then
		BattleDialog({"But you still have dignity."})
		if rage >= 1 then
			currentdialogue = {"[effect:none]I don't need your\nvote!"}
		elseif sell ~= 2 then
			currentdialogue = {"We need a strong\nleader!"}
		else
			currentdialogue = {"Tell your friends!"}
			BattleDialog({"You vote for Trump.\n[w:3]It still feels stupid."})
			canspare = true
			SetGlobal("spareable",1)
		end	
	elseif command == "DUMBEN" then
		if rage >= 1 then
			currentdialogue = {"[effect:none]I don't need\nyour vote!"}
			BattleDialog({"You remove your brain.","Your movements slow."})
		elseif sell == 0 then
			BattleDialog({"You remove your brain.","You find Trump more appealing."})
			currentdialogue = {"Trump 2016!"}
			SetGlobal("sell",1)
		else
			currentdialogue = {"Go ahead and vote!"}
			BattleDialog({"You can't get stupider\rthan this."})
		end	
    elseif command == "INSULT" then
		if rage == 1 then 
			currentdialogue = {"[effect:none][waitall:5]You've made a\nhorrible\nmistake."}
			BattleDialog({"You tell Trump that he\rhas never done anything\rmeaningful in his life."})
			SetGlobal("rage",2)
		elseif rage == 2 then
			currentdialogue = {"[effect:none][waitall:5]Don't continue\nwith this."}
			BattleDialog({"You tell Trump that he is\rimmature!","The bullets get faster!"})
			SetGlobal("rage",3)
		elseif rage == 6 then
			currentdialogue = {"[effect:none][waitall:5]Intimidated?"}
			BattleDialog({"You better not say anything else."})
		elseif rage >= 3 then
			currentdialogue = {"[w:10]I'm offended!"}
			if rage == 3 then
				BattleDialog({"You try to think of a less\roffensive thing to say.[w:20]\nYou talk about politics."})
				SetGlobal("rage",rage+1)
			elseif rage == 4 then
				BattleDialog({"You try to think of a less\roffensive thing to say.[w:20]\nYou talk about the news."})
				SetGlobal("rage",rage+1)
			elseif rage == 5 then
				BattleDialog({"You try to think of a less\roffensive thing to say.[w:20]\nYou could apologize."})
				SetGlobal("rage",rage+1)
			end		
		else	
			BattleDialog({insultdialogue[math.random(#insultdialogue)]})
			currentdialogue = {"[noskip][effect:none][func:Stop][waitall:5]What did you\njust say?[func:Load,megalovania]"}
			SetSprite("trumptastic")
			canspare = false
			SetGlobal("sell",0)
			SetGlobal("spareable",0)
			SetGlobal("rage",1)
			comments = angrycomments
			randomdialogue = ragedialogue
			check = angrycheck
			atk = 1000000
			def = -2
			SetGlobal("spareable",0)
		end
    elseif command == "RESIST" then
		if rage >= 1 then
			currentdialogue = {"[effect:none]Not for long!"}
		else	
			currentdialogue = {"It's me or Hillary..."}
		end	
		BattleDialog({"You resist the Trumptation!"})
    end
end

function HandleAttack(attackstatus)
	rage = GetGlobal("rage")
    if (attackstatus ~= -1 and rage == 0) then
        currentdialogue = {"[effect:none][waitall:5]No."}
    end
	if ((hp <= 200 and hpold > 200) or (hp <= 100 and hpold > 100) or (hp <= 50 and hpold > 50)) then
		SetGlobal("rage",rage+1)
	end
	hpold = hp
end

function Stop()
	Audio.Stop()
end

function Load(file)
	Audio.LoadFile(file)
end