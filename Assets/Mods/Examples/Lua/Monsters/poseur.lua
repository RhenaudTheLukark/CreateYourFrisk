-- A basic monster script skeleton you can copy and modify for your own creations.
comments = {"Smells like the work of an enemy stand.", "Poseur is posing like his life depends on it.", "Poseur's limbs shouldn't be moving in this way."}
commands = {"Check", "Pose", "Stand", "Insult"}
randomdialogue = {"Check it out."}

sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 100000
atk = 3
def = 0
check = "Do not insult its hair."
dialogbubble = "right" -- See documentation for what bubbles you have available.
canspare = false
cancheck = false
xp = 70
gold = 80
wat = 0
again = 0

posecount = 0
--dac97760 bee6bcfc 8ba3f2c2 caf144b2 68302eae
function HandleAttack(attackstatus)
    dialogbubble = "right"
    if attackstatus == -1 then
        dialogbubble = "rightwideminus"
        currentdialogue = {"You don't want to go past this awesome text of mine, would [func:test2]you?[w:20][next:skipover][func:skipTextNoHit:skiponly]", "How convenient!"}
    else
        if hp > 30 then
            currentdialogue = {"You're strong!", "How convenient!"}
        else
            currentdialogue = {"Too strong ..."}
        end
    end
end

function skipTextNoHit()
    currentdialogue = {"What? You REALLY skipped my last line? It's not nice, dude :c[w:20][next:skipover][func:skipTextNoHit2:skiponly]"}
    Encounter["enemies"][2]["currentdialogue"] = {""}
    State("ENEMYDIALOGUE")
end

function skipTextNoHit2()
    currentdialogue = {"Again? Okay then, I won't release you until you stop skipping my text![w:20][next:skipover][func:skipTextNoHit" .. (again < 5 and 2 or 3) .. ":skiponly]"}
    Encounter["enemies"][2]["currentdialogue"] = {""}
    again = again + 1
    State("ENEMYDIALOGUE")
end

function skipTextNoHit3()
    currentdialogue = {"I'm saying random things\n[noskip][instant:stopall]Now you'll listen to me" .. (not safe and " little shit" or "") .. ".[w:20][next]", 
                       "[noskip][letters:5]STOP [w:30][letters:9]SKIPPING [w:30][letters:3]MY [w:30][letters:5]TEXT.[w:60][next]",
                       "[noskip][waitall:10]Hmm.... [noskip:off][waitall:1]Now you can skip my text if you want to.[next]"}
    Encounter["enemies"][2]["currentdialogue"] = {""}
    again = again + 1
    State("ENEMYDIALOGUE")
end

function test()
    DEBUG("TEST!")
end

function test2()
    DEBUG("TEST2!")
end

CheckText = {
    normal1 = {"Check text line 1.\nCheck text line 2.", 14, 7},
    normal2 = {"Check text line 3.\nCheck text line 4.", nil, 0},
    normal3 = {"Check text line 5.\nCheck text line 6.", nil, nil},
    normal4 = {"Check text line 7.\nCheck text line 8.", 5, nil}
}

function HandleCustomCommand(command)
    if command == "CHECK" then
        -- Choose your key here, you can add conditions to choose a key automatically
        wat = wat % 4 + 1
        key = "normal" .. wat

        if CheckText[key] ~= nil then
            local spaceatk = " "; local spacedef = " "; local statseparator = " - "
    
            textToBattleDialog = "\"" .. string.upper(name) .. "\""
            if CheckText[key][2] ~= nil then 
                if CheckText[key][2] >= 10 or CheckText[key][2] <= -10 then spaceatk = "" end
                textToBattleDialog = textToBattleDialog .. statseparator .. 
                                     CheckText[key][2] .. spaceatk .. "ATK "
                statseparator = ""
            end
            if CheckText[key][3] ~= nil then 
                if CheckText[key][3] >= 10 or CheckText[key][3] <= -10 then spacedef = "" end
                textToBattleDialog = textToBattleDialog .. statseparator .. 
                                     CheckText[key][3] .. spacedef .. "DEF "
            end
            textToBattleDialog = textToBattleDialog .. "\n" .. CheckText[key][1]
        end
        if textToBattleDialog == nil then
            textToBattleDialog = "Error, no check text has been found for the tag " .. key
        end 
        BattleDialog(textToBattleDialog)
    elseif command == "POSE" then
        if posecount == 0 then
            currentdialogue = {"Not bad."}
            BattleDialog({"You posed dramatically."})
        elseif posecount == 1 then
            currentdialogue = {"Not bad at all...!"}
            BattleDialog({"You posed even more dramatically."})
        else
            canspare = true
            table.insert(comments, "Poseur is impressed by your posing power.")
            currentdialogue = {"That's\nit...!"}
            BattleDialog({"You posed so dramatically, your anatomy became incorrect."})
        end
        posecount = posecount + 1
    elseif command == "STAND" then
        currentdialogue = {"What's the hold-up?"}
        BattleDialog({"You just kind of stand there."})
    elseif command == "INSULT" then
        currentdialogue = {"But I don't have hair."}
        BattleDialog({"You make a scathing remark about Poseur's hairstyle."})
    end
end