encountertext = "Your path is blocked by two mannequins! ∞.∞\nYou should look at the scripts![w:30][next]" --Modify as necessary. It will only be read out in the action select screen.
--encountertext = "[effect:rotate,5][lettereffect:shake,5]Your path is blocked by two mannequins! ∞.∞[lettereffect:none]\nYou should look at the scripts!"
wavetimer = 4
arenasize = {155, 130}
nextwaves = {"bullettest_touhou"}
flee = false
autolinebreak = true
playerskipdocommand = true
timer = 0
index = 0
DEBUG("Outside of function: Player.x = " .. Player.x)
--revive = true
--deathtext = {"You have ascended.", "Now, fulfill your [lettereffect:rotate][color:ffff00]dream[color:ffffff]."}
deathtext = {"This is a test of death text.", "And it [color:ffff00][lettereffect:shake]succeeded[color:ffffff]!"}
deathmusic = "mus_zz_megalovania"

enemies = { "poseur", "posette" }
enemypositions = { {-180, 0}, {120, 0} }

texts = {}

possiblestrings = { "I'm outta here.", "I've got shit to do.", "I've got better things to do.", "Don't waste my time.", "Fuck this shit I'm out.",
                    "Nah, I don't like you.", "I just wanted to walk a bit. Leave me alone.", "You're cute, I won't kill you :3",
                    "Better safe than sorry.", "Do as if you've never saw them and walk away.", "I'll kill you last.",
                    "Nope. Nope. Nope. Nope. Nope.", "Wait for me, Rhenaud!" };

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy" --[[, "bullettest_chaserorb", "bullettest_touhou"]]}

function EncounterStarting()
    --[[bullet = CreateProjectile("BoneCenter", 0, 100, "Top")
        bullet.sprite.Scale(1, 2)
        bullet.isPersistent = true]]
	--Types : 0 = Consumable, 1 = Weapon, 2 = Armor, else = Special (you must use 3)
	Inventory.AddCustomItems({"TEST", "TEST2", "Shotgun", "Shotgun2", "Shotgun3", "Shotgun4", "Bandage", "PsnPotion", "Life Roll", "Nothing", "Pie", "Snails"}, {0, 0, 1, 1, 1, 1, 0, 0, 0, 3, 0, 0})
	Inventory.SetInventory({"Shotgun", "Shotgun2", "Butterscotch Pie", "Bandage", "Nothing", "PsnPotion", "Life Roll", "Real Knife"})
	Player.lv = 999
	Player.ForceHP((4 * Player.lv + 19) * 1.5)
    SetPPCollision(true)
    NewAudio.CreateChannel("testmusic")
    NewAudio.CreateChannel("testvoice")
    NewAudio.CreateChannel("testsound")
    NewAudio.PlayMusic("testmusic", "mus_zz_megalovania")
    NewAudio.Stop("testmusic")
    NewAudio.PlayVoice("testvoice", "v_papyrus")
    NewAudio.Stop("testvoice")
    NewAudio.PlaySound("testsound", "slice")
    NewAudio.Stop("testsound")
end

function EnemyDialogueStarting()
    -- Good location for setting monster dialogue depending on how the battle is going.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
    --[[maintext = CreateText(
            {"[font:uidialog][novoice][waitall:3]Greetings.",
             "[novoice][waitall:3]I[w:20] am " .. Player.name .. ".",
             "[func:movetext][novoice][waitall:3][color:ff0000]Thank you.",
             "[novoice][waitall:3]Your power awakened me\nfrom death."}, {400, 99}, 320, "Top", 100)
        maintext.progressmode = "manual"
        --maintext.SetText(
        --    {"[font:uidialog][color:ff0000][novoice][waitall:3]Greetings.",
        --     "[font:uidialog][color:ff0000][novoice][waitall:3]I[w:20] am Chara.",
        --     "[font:uidialog][color:ff0000][novoice][waitall:3]Greetings.",
        --     "[novoice][waitall:3]Thank you."})
        --maintext.SetEffect("none", -1)
        maintext.HideBubble()
        State("NONE")
        if #texts == 0 then
            local text = CreateText({"Okay, this is a[color:00ffff] test.[color:000000]", "It works!"}, {540, 400}, 150)
            text.ShowBubble("up", "50%")
            text.SetEffect("shake", -1)
            table.insert(texts, text)
        else 
            texts[1].SetText({"Omg this is a second test!", "AND OMG IT REALLY WORKS I'M SO HAPPY YAY YAY YAY!"})
            texts[1].ShowBubble("down", 75)
        end]]
end

function yay()
    DEBUG("yay")
end

function HandleSpare()
    State("ENEMYDIALOGUE")
end

function HandleItem(ItemID)
    if ItemID == "TEST" then
		Inventory.NoDelete = true
		BattleDialog({"This is a test of a persistent\ritem. If it succeeded, the item\rmust be in the inventory in\rthe next turn!"})
	elseif ItemID == "TEST2" then
		BattleDialog({"This is a test of a normal item.\rAnd it succeeded!!!"})
	elseif ItemID == "SHOTGUN" then
		Inventory.SetAmount(16777215)
	elseif ItemID == "SHOTGUN2" then
		Inventory.SetAmount(16777215)
		BattleDialog({"This is an example of equipment!"})
	elseif ItemID == "BANDAGE" then
		BattleDialog({"This is an example of a replaced\robject. If you see this text, that\rmeans that this works!"})
	elseif ItemID == "PSNPOTION" then
		BattleDialog({"You drank the Poison Potion.","[noskip][waitall:10]...[waitall:1][w:20]\rThat was a bad idea.[w:20][health:kill]"})
	elseif ItemID == "LIFE ROLL" then
		BattleDialog({"Your HP goes to 1[waitall:10]...[waitall:1][health:1, set]now.[w:20]\rNow, byebye![w:20][health:-1, killable]"})
	elseif ItemID == "NOTHING" then
		BattleDialog({"You use Nothing.[w:10]Did you really thought\rthat something would happen?"})
	elseif ItemID == "PIE" then
        BattleDialog({"You ate the Pie. \nIt reminds you of Frisk."}) 
        Player.Heal(99)
    elseif ItemID == "SNAILS" then
        BattleDialog({"You ate the Snails. Slimy..."}) 
        Player.Heal(15)
    end
    BattleDialog({"I'm blocking the text path!"})
end
	
function Heal(amount)
	Audio.PlaySound("healsound")
	Player.hp = Player.hp + amount
end

function Update()
    --[[if GetCurrentState() == "DEFENDING" then
            --DEBUG(Wave[1]["wavename"])
            if Wave[1]["wavename"] == "bullettest_touhou" then
                Wave[1].Call("OMGITWORKS")
            end
        end]]
	
    if Input.GetKey("E") == 2 then
	    enemies[1].Call("SetSliceAnimOffset", {math.random(-40, 11), math.random(-60, 61)})
        Player.ForceAttack(1, 2)
        --Player.CheckDeath()
    elseif Input.GetKey("O") == 1 then
        Player.hp = Player.hp + 1
    elseif Input.GetKey("L") == 1 then
        Player.hp = Player.hp - 1
    elseif Input.GetKey("M") == 1 then
        NewAudio.CreateChannel("Box")
        NewAudio.PlayMusic("Box","mus_zz_megalovania",true,1)
        Audio.LoadFile("mus_zz_megalovania")
    end
    
	
	--DEBUG(Misc.WindowY)
    timer = timer + 1
    --[[if windows then
        Misc.WindowY = Misc.WindowY - 1
        if timer % 60 == 0 then
            Misc.WindowName = possiblestrings[math.random(#possiblestrings)]
        end
    end]]
end