comments = {"Smells like stars and platinum.", "Posette is standing there quietly.", "Posette is flexing."}
commands = {"Pose 1", "Stand", "Insult"}
--commands = {"", "Weehee", "WOT", "Ok then", "Last one"}
randomdialogue = {"Gimme a break.", "...", "Ora Ora Ora Ora"}

sprite = "posette" --Always PNG. Extension is added automatically.
name = "Posette"
hp = 100000
atk = 3000
def = 0
check = "The next in a long line of respected mannequins."
dialogbubble = "right" -- See documentation for what bubbles you have available.
xp = 90
gold = 100

this_must_be_the_work_of_an_enemy_stand = 0

-- Happens after the slash animation but before 
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        currentdialogue = {"Weak."}
    else
        if hp > 30 then
            currentdialogue = {"I felt that.", "[letters:99]Maaaaaaaaaaaaaaaaaaaaaybe"}
        else
            currentdialogue = {"Now I'm angry."}
        end
    end
end

function BeforeDamageValues()
	DEBUG(Player.lasthitmultiplier)
	--Player.changeTarget(1)
end
 
-- This handles the commands; all-caps versions of the commands list you have above.
    
commands = {"Pose 1", "Listen to me", "Insult"}

function HandleCustomCommand(command)
    error("error")
    local battleDialogText = {"error shit"}
    if command == "POSE 1" then
        battleDialogText = {"I made a pose yay"}
        --quack
    elseif command == "LISTEN TO ME" then
        battleDialogText = {"Please :c", "I have cookies :c"}
        --quackquack
    end
    BattleDialog(battleDialogText)
end
--[[
function HandleCustomCommand(command)
    if command == "POSE 1" then
        currentdialogue = {"[noskip][waitall:5].....[nextthisnow]", 
                           "[noskip]Shaddap Poseur[w:20][next]", 
                           "[noskip]I don't like you.[w:20][finished]", 
                           "" , 
                           "Yup"}
		Encounter["enemies"][1]["currentdialogue"] = {"[noskip]And it goes like [waitall:5]weeeeeeeeeeeeeeeeeeeeeee", 

                                                      "[finished]", 
                                                      "Awww, seriously ?", 
                                                      "", 
                                                      ":c", 
                                                      "[noskip]Enter DIE: [waitfor:D]D [waitfor:I]I [waitfor:E]E."}
        BattleDialog({"You struck your best pose,[w:7] but Posette remained unimpressed.[w:20][next]"})
    elseif command == "STAND" then
        if this_must_be_the_work_of_an_enemy_stand == 0 then
            currentdialogue = {"[effect:none]Stand- [charspacing:6][linespacing:10]off? [charspacing:3]Alright."}
            BattleDialog({"You just kind of stand there."})
        elseif this_must_be_the_work_of_an_enemy_stand == 1 then
            currentdialogue = {"Agh..."}
            BattleDialog({"Your standing intensifies.[w:30][next]", "Posette won't hold on much longer."})
        else
            canspare = true
            table.insert(comments, "There's still a faint rumbling.")
            currentdialogue = {"I give up."}
            BattleDialog({"You stand there intently. You hear a faint rumbling."})
        end
        this_must_be_the_work_of_an_enemy_stand = this_must_be_the_work_of_an_enemy_stand + 1
    elseif command == "INSULT" then
        currentdialogue = {"\n[charspacing:30]\n A[linespacing:-30]\n[charspacing:15] w[linespacing:22]\n[charspacing:50] f[linespacing:-30]\n[charspacing:2] u[linespacing:-30]\nl."}
        BattleDialog({"You make a scathing remark about Posette's pose."})
    end
end]]

function SetBubble(bubble)
    dialogbubble = bubble
end