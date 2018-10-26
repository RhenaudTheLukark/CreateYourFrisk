comments = {"You should check the path\nExamples/Sprites/UI/Fonts !"}
commands = {"Pose", "Stand", "Insult"}
randomdialogue = {"Check\nit\nout\nù-ù"}

sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 60
atk = 4
def = 1
check = "Do not insult its hair."
dialogbubble = "right" -- See documentation for what bubbles you have available.
canspare = false
xp = 10
gold = 20

posecount = 0

-- Happens after the slash animation but before the animation.
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
        currentdialogue = {"Do\nno\nharm."}
    else
        -- player did actually attack
        if hp > 30 then
            currentdialogue = {"You're\nstrong!"}
        else
            currentdialogue = {"Too\nstrong\n..."}
        end
    end
end

-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "POSE" then
        if posecount == 0 then
            currentdialogue = {"Not\nbad."}
            BattleDialog({"You posed dramatically."})
        elseif posecount == 1 then
            currentdialogue = {"Not\nbad\nat\nall...!"}
            BattleDialog({"You posed even more dramatically."})
        else
            canspare = true
            table.insert(comments, "Poseur is impressed by your\rposing power.")
            currentdialogue = {"That's\nit...!"}
            BattleDialog({"You posed so dramatically,\ryour anatomy became\rincorrect."})
        end
        posecount = posecount + 1
    elseif command == "STAND" then
        currentdialogue = {"What's\nthe\nhold-up?"}
        BattleDialog({"You just kind of stand there."})
    elseif command == "INSULT" then
        currentdialogue = {"But\nI don't\nhave\nhair."}
        BattleDialog({"You make a scathing remark about\rPoseur's hairstyle."})
    end
end