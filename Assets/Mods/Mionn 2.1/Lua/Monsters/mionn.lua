comments = {"[novoice]No, don't\nNo, don't\nNo, don't\nNo, don't\nNo, don't\nNo, don't\nNo, don't"}
commands = {"Check","!!!"}
randomdialogue = {}
This_Is_Not_Mionn = true
The_Game_Is_A_Lie = true
He_Is_There = false
He_Is_There = true
sprite = "empty"

name = "???"
hp = 666666
atk = 666
def = 666
check = "It is?"
dialogbubble = "right"
canspare = false
cancheck = false
SetActive(false)
Fun_Tunes = 0
function HandleAttack(attackstatus)
	Fun()
end

function HandleCustomCommand(command)
	Encounter.SetVar("FUN",true)
	if command == "CHECK" then
		BattleDialog({"[noskip]It...[w:30] is?[w:100][func:Fun][w:9999]"})
	end
    if command == "!!!" then
		Encounter.Call("Call_Fun")
		Encounter.GetVar('enemies')[1].SetVar('dialogbubble' , "rightlong")
		currentdialogue = {""}
		State("ENEMYDIALOGUE")
	end
	Right_Choice = true
end

require "Libraries/Funionn"

function FunText()
	endtext = ""
	extratext = {}
	j = 7
	extratext[1] = "\r         changed[w:30]"
	for i = 2,j do
		extratext[i] = "\r         changed[w:30]"..extratext[i-1]
	end
	endtext = extratext[j].."[func:FunText]"
	text = "[instant]Something's changed."..endtext
	BattleDialog({text})
end
