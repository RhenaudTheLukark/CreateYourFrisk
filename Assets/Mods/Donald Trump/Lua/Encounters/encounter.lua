music = "trumptastic"
encountertext = "Trump runs for president."
nextwaves = {"bullet_money"}
wavetimer = 6.0
arenasize = {155, 130}
enemies = {"custom"}
enemypositions = {
{0, 0}
}

ShowedDialog = false

possible_attacks = {"bullet_ragestreaming", "bullet_ragetrig", "bullet_lines", "bullet_tornado"}

function EncounterStarting()
    -- If you want to change the game state immediately, this is the place.
    Player.lv = 3
    Player.hp = 28
    if isCYF then
	    SetPPCollision(true)
	end
end

function EnemyDialogueStarting()
	
end

function EnemyDialogueEnding()
	if GetGlobal("rage") == 0 then
		nextwaves = {"bullet_streaming"}
		DEBUG(nextwaves[1])
		if GetGlobal("sell") == 1 then
			nextwaves = {"bullet_money"}
			SetGlobal("sell",2)
		elseif GetGlobal("sell") == 2 then
			SetGlobal("sell",3)
		end
	else
		wavetimer = 8.0
		nextwaves = {possible_attacks[math.random(#possible_attacks)]}
		DEBUG(nextwaves[1])
	end
end

function DefenseEnding()
	encountertext = RandomEncounterText()
	if (GetGlobal("rage") > 0 and ShowedDialog == false) then
		ShowedDialog = true
		encountertext = "Looks like Trump let his\rdefense down."
	end	
	if GetGlobal("spareable") == 1 then
		encountertext = "Trump is one step closer to\rwinning.[w:15]\nMaybe."
	end	
	if GetGlobal("sell") == 3 then
		SetGlobal("sell",0)
	end	
end

function HandleSpare()
    if GetGlobal("rage") > 0 then
		BattleDialog({"Trump won't let you go until\ryou've payed for your sins."})
	elseif GetGlobal("spareable") == 1 then
		BattleDialog({"This is where you'd spare him.\nGood job."})
	else
		BattleDialog({"It's rude to just leave without\rvoting for someone."})
    end
end

function HandleItem(ItemID)
    BattleDialog({"No dogs will help you in this\rbattle."})
end