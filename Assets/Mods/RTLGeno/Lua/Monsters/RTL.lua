-- A basic monster script skeleton you can copy and modify for your own creations.
comments = {"RTL looks nervous."}
commands = {"Threaten", "Insult"}
randomdialogue = {"[noskip][font:monster]H-hey[waitall:5]...[w:20][func:State,ACTIONSELECT]"}

sprite = "empty" --Always PNG. Extension is added automatically.
name = "RTL"
hp = 100
atk = 1
def = -999
check = "Just free EXP."
dialogbubble = "rightwideminus" -- See documentation for what bubbles you have available.
cancheck = false
canspare = true
sprite_placing = -1

function ChangeBubble(str)
	DEBUG("Got in !")
    if (str != null) then
        dialogbubble = str
    else
        DEBUG("The function takes one argument.\nYOU HAD ONE JOB.")
    end        
end

-- Happens after the slash animation but before 
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
		SetGlobal("RTL","close")
		DEBUG("close one!")
		currentdialogue = {"[noskip][font:monster]It was[func:ChangeBubble,left]\nclose...[w:40]",
						   "[noskip]This is a test.[func:animGo][func:State,ACTIONSELECT]"}
    else
        -- player did actually attack
		SetGlobal("RTL","hurt")
		SetGlobal("hitAnimRTL", true)
    end
end
 
-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "THREATEN" then
        BattleDialog({"You threaten RTL.\nHe looks afraid."})
        currentdialogue = {"[noskip][func:SetAnimRTL,attacked]Leave me\nalone !\nYou're\nweird ![w:40][func:State,ACTIONSELECT]"}
    elseif command == "INSULT" then
        BattleDialog({"You insult RTL."})
        currentdialogue = {"[noskip][func:SetAnimRTL,angry]Why are you\ninsulting me ?\nI did nothing\nbad ![w:40][func:State,ACTIONSELECT]"}
    end
    currentdialogue = {"[font:monster]" .. currentdialogue[1]}
end

function HandleSpare()
	SetGlobal("sparedRTL", true) 
	DEBUG("spared !") 
end

function OnDeath()
	dialogbubble = "rightlargeminus"
	hp = 30
	def = 1
	Audio.Pause()
	--currentdialogue = {"[noskip][effect:none][font:monster][func:SetAnimRTL,angryhurt]blablablablabla[func:PlaySE,Asriel TF][func:AnimDilate][w:120][func:SetLukark][func:State,ACTIONSELECT]"}
	currentdialogue = {"[noskip][effect:none][font:monster]" .. "Ah,[w:20] ah ah ah ![w:40][next]",
					   "[noskip][effect:none][font:monster]" .. "You're pathetic.\n[w:20]You REALLY\nthought that I\nwill not struggle\nagainst you ?[w:40][next]",
					   "[noskip][effect:shake][font:monster][func:SetAnimRTL,angryhurt][voice:v_floweymad]" .. "[waitall:5]WELL YOU'RE\nWRONG.[w:40][next]",
					   "[noskip][effect:none][font:monster]" .. "Now, I'll send\nyou right where\nyou came from,[w:20]\nand I'll do it\nby force if I\nhave to ![w:40][func:PlaySE,Asriel TF][func:AnimDilate][next]",
					   "[noskip][w:120][next]",
					   "[noskip][effect:shake][font:monster][voice:v_floweymad]ALL OF YOUR\nCRIMES WILL\nSTOP RIGHT\nNOW ![w:40][func:SetLukark][func:State,ACTIONSELECT]"}
					   --"[noskip][effect:none][font:monster][func:SetAnimRTL,happy][func:PlaySE,mus_dogsong_tf]" .. "Tu t'attendais\na quoi, srx ?\n[w:20]Eh ben non y'a\nrien, je vais\ncrever c'est\ntout.[w:40][func:Unpause][func:FadeKill][func:Kill][next]"}
end

--Permet de pouvoir lancer l'anim de fade
function FadeKill()
	SetGlobal("RTLDead", true)
end

--Permet de changer l'anim
function SetAnimRTL(nomAnim)
	SetGlobal("RTL", nomAnim)
end

--Permet de lancer l'anim de "tf"
function AnimDilate()
	SetGlobal("animDilate", true)
end

--Permet de faire repartir la musique
function Unpause()
	Audio.Unpause()
end

--Permet de jouer des sons
function PlaySE(filename)
	Audio.PlaySound(filename)
end

function SetLukark()
	Audio.Pitch(1)
	Audio.Volume(0.75)
	Audio.LoadFile("charafuntroncated")
	SetGlobal("revived", true)
end

function Deactivate()
	SetActive(false)
end

function animGo()
	SetGlobal("placingRTL", -1)
end