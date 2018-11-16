encountertext = "Your path is blocked by two mannequins!" --Modify as necessary. It will only be read out in the action select screen.

wavetimer = 4
arenasize = {155, 130}
nextwaves = {"bullettest_touhou"}
autolinebreak = true

enemies = {"twoMonstersPoseur", "twoMonstersPosette"}
enemypositions = { {-180, 0}, {120, 0} }

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
    -- Sets up an inventory!
	Inventory.AddCustomItems({"TEST", "TEST2", "Shotgun", "Bandage", "PsnPotion", "Life Roll", "Nothing", "Pie", "Snails"}, {0, 0, 1, 1, 1, 1, 0, 0, 0, 3, 0, 0})
	Inventory.SetInventory({"TEST", "TEST2", "Shotgun", "Butterscotch Pie", "Bandage", "PsnPotion", "Life Roll", "Real Knife"})
end

function EnemyDialogueStarting()
    -- Good location for setting monster dialogue depending on how the battle is going.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
    State("ENEMYDIALOGUE")
end

-- This function handles the items' effects!
function HandleItem(ItemID)
    if ItemID == "TEST" then
        -- Makes this item stay in your bag
		Inventory.NoDelete = true
		BattleDialog({"This is a test of a persistent\ritem. If it succeeded, the item\rmust be in the inventory in\rthe next turn!"})
        return
	elseif ItemID == "TEST2" then
		BattleDialog({"This is a test of a normal item.\rIt should disappear from your bag.\nThe test succeeded!"})
	elseif ItemID == "SHOTGUN" then
        -- Equips a weapon with this amount of damage
		Inventory.SetAmount(16777215)
        return
	elseif ItemID == "BANDAGE" then
		BattleDialog({"This is an example of a replaced\robject. If you see this text, that\rmeans it works!"})
	elseif ItemID == "PSNPOTION" then
        -- Instant kill
		BattleDialog({"[effect:rotate]You drink the Poison Potion.","[noskip][waitall:10]...[waitall:1][w:20]\rThat was a bad idea.[w:20][health:kill]"})
	elseif ItemID == "LIFE ROLL" then
        -- Sets your HP to 1 then kills you
		BattleDialog({"Your HP goes to 1[waitall:10]...[waitall:1][health:1, set]now.[w:20]\rNow, byebye![w:20][health:-1, killable]"})
	elseif ItemID == "NOTHING" then
		BattleDialog({"You use Nothing.[w:10]Did you really think something would happen?"})
	elseif ItemID == "PIE" then
        BattleDialog({"You ate the Pie. \nIt reminds you of Frisk."}) 
        Player.Heal(99)
    elseif ItemID == "SNAILS" then
        BattleDialog({"You ate the Snails. Slimy..."}) 
        Player.Heal(15)
    end
end