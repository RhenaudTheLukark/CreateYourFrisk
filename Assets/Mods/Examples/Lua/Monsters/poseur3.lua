-- A basic monster script skeleton you can copy and modify for your own creations.
comments = {"Arena.Move, Arena.MoveTo etc...\nRead the documentation !\n(Plus he is unkillable)"}
commands = {"Pose", "Stand", "Insult", "Surprise"}
randomdialogue = {"I'm\nunkil-\nlable!", "Move\non!"}

sprite = "poseur" --Always PNG. Extension is added automatically.
name = "Poseur"
hp = 30
maxhp = 100
atk = 4
def = 1
check = "Do not insult its hair."
dialogbubble = "rightwide" -- See documentation for what bubbles you have available.
canspare = true
xp = 50
gold = 60
unkillable = true

posecount = 0

function HandleAttack(attackstatus)
    if attackstatus == -1 then
        currentdialogue = {"[voice:test]Do\nno\n[lettereffect:shake,5]harm."}
		Player.sprite.Dust()
    else
	    --DEBUG("*" ..tostring(canspare) .. "*")
		DEBUG("HP = " .. hp)
		if hp < 0 then
		    currentdialogue = {"Am I\nsupposed\n...to\nbe dead?"}
        end
    end
end

function BeforeDamageCalculation()
    DEBUG(Player.hp)
    DEBUG(Player.lasthitmultiplier)
    DEBUG(Player.lastenemychosen)
    SetDamage(1000000)
end

--function OnSpare()
--	currentdialogue = {
--	"[noskip]You spared me ![w:20]\nWeeeeeeeeeeeeeee[w:20][func:Spare]"
--	}
--	State("ENEMYDIALOGUE")
--end

function HandleCustomCommand(command)
    if command == "POSE" then
        if posecount == 0 then
            currentdialogue = {'[func:SetBubble,rightlarge]Not\nbad.', "[w:30][next]", "Or is\nit ?"}
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
    elseif command == "SURPRISE" then
	    currentdialogue = {"No... [w:10]Please...[w:20]", 
		                   "[noskip]You can't do th[func:forceattack,1,999999999][next]", 
						   "[noskip][w:80][next]", 
		                   "[waitall:5].....[waitall:1]Why?[w:10][func:Kill][next]"}
		State("ENEMYDIALOGUE")
	end
end

function forceattack(number, damage)
    Player.ForceAttack(number, damage)
end

function SetBubble(bubble)
    dialogbubble = bubble
end