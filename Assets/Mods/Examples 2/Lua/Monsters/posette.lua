comments = {"Smells like stars and platinum.", "Posette is standing there quietly.", "Posette is flexing."}
commands = {"Pose", "Stand", "Insult"}
randomdialogue = {"Gimme\na\nbreak.", "...", "Ora\nOra\nOra\nOra"}

sprite = "posette" --Always PNG. Extension is added automatically.
name = "Posette"
hp = 60
atk = 4
def = 2
check = "The next in a long line of\rrespected mannequins."
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
            currentdialogue = {"I felt\nthat."}
        else
            currentdialogue = {"Now\nI'm\nangry."}
        end
    end
end
 
-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "POSE" then
        currentdialogue = {"It's\nalright."}
        BattleDialog({"You struck your best pose,[w:7]\rbut Posette remained unimpressed."})
    elseif command == "STAND" then
        if this_must_be_the_work_of_an_enemy_stand == 0 then
            currentdialogue = {"Stand-\noff?\nAlright."}
            BattleDialog({"You just kind of stand there."})
        elseif this_must_be_the_work_of_an_enemy_stand == 1 then
            currentdialogue = {"Agh..."}
            BattleDialog({"Your standing intensifies."})
        else
            canspare = true
            table.insert(comments, "There's still a faint rumbling.")
            currentdialogue = {"I give\nup."}
            BattleDialog({"You stand there intently.\nYou hear a faint rumbling."})
        end
        this_must_be_the_work_of_an_enemy_stand = this_must_be_the_work_of_an_enemy_stand + 1
    elseif command == "INSULT" then
        currentdialogue = {"Awful."}
        BattleDialog({"You make a scathing remark about\rPosette's pose."})
    end
end