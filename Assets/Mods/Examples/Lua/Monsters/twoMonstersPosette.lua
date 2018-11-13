comments = {"Smells like stars and platinum.", "Posette is standing there quietly.", "Posette is flexing."}
commands = {"Pose", "Stand", "Insult"}
randomdialogue = {"Gimme a break.", "...", "Ora Ora Ora Ora"}

sprite = "posette" --Always PNG. Extension is added automatically.
name = "Posette"
hp = 100
atk = 3
def = 0
check = "The next in a long line of respected mannequins."
dialogbubble = "right" -- See documentation for what bubbles you have available.
canspare = false
xp = 30
gold = 40

this_must_be_the_work_of_an_enemy_stand = 0

-- Happens after the slash animation but before the animation.
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
        currentdialogue = {"Weak."}
    else
        -- player did actually attack
        if hp > 30 then
            currentdialogue = {"I felt that."}
        else
            currentdialogue = {"Now I'm angry."}
        end
    end
end
 
-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "POSE" then
        BattleDialog({"You struck your best pose,[w:7] but Posette remained unimpressed.[w:20][next]"})
    elseif command == "STAND" then
        if this_must_be_the_work_of_an_enemy_stand == 0 then
            currentdialogue = {"Stand-\noff?\nAlright."}
            BattleDialog({"You just kind of stand there."})
        elseif this_must_be_the_work_of_an_enemy_stand == 1 then
            currentdialogue = {"Agh..."}
            BattleDialog({"Your standing intensifies.[w:30][next]", "Posette won't hold on much longer."})
        else
            canspare = true
            table.insert(comments, "There's still a faint rumbling.")
            currentdialogue = {"I\ngive\nup."}
            BattleDialog({"You stand there intently. You hear a faint rumbling."})
        end
        this_must_be_the_work_of_an_enemy_stand = this_must_be_the_work_of_an_enemy_stand + 1
    elseif command == "INSULT" then
        currentdialogue = {"Awful."}
        BattleDialog({"You make a scathing remark about Posette's pose."})
    end
end