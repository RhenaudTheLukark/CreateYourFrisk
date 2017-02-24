-- A basic monster script skeleton you can copy and modify for your own creations.
comments = {"Lukark is revengeful.","Lukark prepares his\nnext attack.","Lukark frowns at you.",
			"What an irony that\nLukark will die soon","Lukark is brushing\nhis hair.","Lukark looks you\nwith intensity.",
			"Lukark is mad\nbecause of all the dust\non your clothes."}
commands = {"Pardon"}
randomdialogue = {"[noskip][font:monster][func:SetAnim,angry]Come back\nhere !", "[noskip][font:monster][func:SetAnim,angry]You can't\nescape me !",
				  "[noskip][font:monster][func:SetAnim,angry]I'll got you,\nwhatever you'll\ndo !","[noskip][font:monster][func:SetAnim,angry]Take this !"}

sprite = "empty" --Always PNG. Extension is added automatically.
name = "Lukark"
hp = 1000
atk = 5
def = 1
check = "The Overworld Creator.[w:10]\nJust destroy him."
dialogbubble = "rightwideminus" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
priercompteur = 0
attackprogression = 0
anim = "normal"
SetActive(false)

-- Happens after the slash animation but before 
function HandleAttack(attackstatus)
    if attackstatus == -1 then
        -- player pressed fight but didn't press Z afterwards
    else
        -- player did actually attack
		SetGlobal("Lukark","hurt")
		SetGlobal("hitAnimLukark", true)
    end
end
 
-- This handles the commands; all-caps versions of the commands list you have above.
function HandleCustomCommand(command)
    if command == "PARDON" then
		if priercompteur == 0 then
			BattleDialog({"You tells Lukark that\nyou'll stop killing everyone."})
			currentdialogue = {"[noskip][font:monster][func:SetAnim,angry]You REALLY\nthink that\nI'll trust\nyou ?"}
		elseif priercompteur == 1 then
			BattleDialog({"You continue to tell\nLukark that you'll\nstop this."})
			currentdialogue = {"[noskip][font:monster][waitall:5]...[waitall:0][wait:10][func:SetAnim,smiling]I still\ndon't believe\nyou."}
		elseif priercompteur == 2 then
			BattleDialog({"Once again,[w:10] you tells\nLukark that you'll stop\nthis slaughter."})
			currentdialogue = {"[noskip][font:monster][waitall:5]......[waitall:0][func:SetAnim,happy]No,\n[w:10]please stop."}
		else
			BattleDialog({"You ask Lukark's forgiveness\nonce more[waitall:5]...",
						  "But he ignores you."})
		end
		priercompteur = priercompteur + 1
    end
end

function OnDeath()
	hp = 1
	Audio.Pause()
	currentdialogue = {"[noskip][font:monster]" .. "No,[w:20] I knew it.[w:40][next]",
					   "[noskip][font:monster]" .. "I should have\nbeen more\ncareful.[w:10].[w:10].[w:40][next]",
					   "[noskip][font:monster][func:SetAnim,happy]" .. "[waitall:5]Wh-What a j-je[func:FadeKill][func:Kill][next]"}
end

--Permet de pouvoir lancer l'anim de fade
function FadeKill()
	SetGlobal("LukarkDead", true)
end

--Permet de changer l'anim
function SetAnim(nomAnim)
	SetGlobal("Lukark", nomAnim)
end

--Permet de stopper la musique
function Pause()
	Audio.Pause()
end

--Permet de relancer la musique
function Unpause()
	Audio.Unpause()
end

--Permet de jouer des sons
function PlaySE(filename)
	Audio.PlaySound(filename)
end

function Activate()
	SetActive(true)
end