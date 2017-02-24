SetGlobal("StopRTL",false)
SetGlobal("hitAnimRTL",false)
SetGlobal("placingRTL",-2)
hitAnimCount = 0
avancementDilate = 0
animPhaseCut = Time.time

RTLtorsohurt = CreateSprite("RTL/torso")
RTLlegshurt = CreateSprite("RTL/legshurt")
RTLheadhurt = CreateSprite("RTL/headhurt")
RTLheadangry = CreateSprite("RTL/headangry")
RTLheadangryhurt = CreateSprite("RTL/headangryhurt")
RTLheadattacked = CreateSprite("RTL/headattacked")
RTLheadhappy = CreateSprite("RTL/headhappy")
RTLheadclose = CreateSprite("RTL/headclose")
RTLtorsospared = CreateSprite("RTL/torsospared")
RTLlegsspared = CreateSprite("RTL/legsspared")
RTLheadspared = CreateSprite("RTL/headspared")
RTLhead = CreateSprite("RTL/headnormal")
RTLtorso = CreateSprite("RTL/torso")
RTLlegs = CreateSprite("RTL/legsnormal")

RTLtorsohurt.SetParent(RTLlegshurt)
RTLheadhurt.SetParent(RTLtorsohurt)
RTLheadangry.SetParent(RTLtorso)
RTLheadangryhurt.SetParent(RTLtorsohurt)
RTLheadattacked.SetParent(RTLtorso)
RTLheadhappy.SetParent(RTLtorso)
RTLheadclose.SetParent(RTLtorso)
RTLtorsospared.SetParent(RTLlegsspared)
RTLheadspared.SetParent(RTLtorsospared)
RTLtorso.SetParent(RTLlegs)
RTLhead.SetParent(RTLtorso)

RTLlegshurt.y = 240
RTLlegshurt.x = 320
RTLtorsohurt.y = -14 
RTLtorsohurt.x = 2 
RTLheadangryhurt.y = 40 
RTLheadangryhurt.x = -2 
RTLheadangry.y = 40 
RTLheadangry.x = -2 
RTLheadhurt.y = 40 
RTLheadhurt.x = -2 
RTLheadattacked.y = 40 
RTLheadattacked.x = -2 
RTLheadhappy.y = 40 
RTLheadhappy.x = -2 
RTLheadclose.y = 40 
RTLheadclose.x = -2 
RTLlegsspared.y = 240
RTLlegsspared.x = 320
RTLtorsospared.y = -14 
RTLtorsospared.x = 2 
RTLheadspared.y = 40 
RTLheadspared.x = -2
RTLlegs.y = 240
RTLlegs.x = 320
RTLtorso.y = -14 
RTLtorso.x = 2 
RTLhead.y = 40 
RTLhead.x = -2 

RTLtorsohurt.SetPivot(0.5, 0.6)
RTLlegshurt.SetPivot(0.5, 0)
RTLheadhurt.SetPivot(0.5, 0.22)
RTLheadangry.SetPivot(0.5, 0.22)
RTLheadangryhurt.SetPivot(0.5, 0.22)
RTLheadattacked.SetPivot(0.5, 0.22)
RTLheadhappy.SetPivot(0.5, 0.22)
RTLheadclose.SetPivot(0.5, 0.22)
RTLlegsspared.SetPivot(0.5, 0)
RTLtorsospared.SetPivot(0.5, 0.5)
RTLheadspared.SetPivot(0.5, 0.22)
RTLlegs.SetPivot(0.5, 0)
RTLtorso.SetPivot(0.5, 0.5)
RTLhead.SetPivot(0.5, 0.22)

blank = CreateSprite("blank")
blank.alpha = 0
blank.x = 320
blank.y = 240
blank.SendToTop()

temp_stopanimtime = 0
function AnimateRTL()
	--Séquence de mort
	if GetGlobal("RTLDead") == true then 
		--Si après "tf"
		if GetGlobal("avancement") > 0 and RTLheadangryhurt != 0 then
			if RTLheadangryhurt.alpha == 1 then
				SetGlobal("StopRTL",true)
			end
			RTLheadhappy.alpha = RTLheadhappy.alpha - 0.1
			RTLtorso.alpha = RTLtorso.alpha - 0.1
			RTLlegs.alpha = RTLlegs.alpha - 0.1
		--Si avant "tf"
		else
			if RTLheadattacked.alpha == 1 then
				SetGlobal("StopRTL",true)
			end
			RTLheadattacked.alpha = RTLheadattacked.alpha - 0.1
			RTLtorso.alpha = RTLtorso.alpha - 0.1
			RTLlegshurt.alpha = RTLlegs.alpha - 0.1
		end		
	end
	--Si spare
	if GetGlobal("sparedRTL") == true then
		SetGlobal("StopRTL",true)
		RTLheadhurt.alpha = 0
		RTLheadangry.alpha = 0
		RTLheadattacked.alpha = 0
		RTLheadhappy.alpha = 0
		
		RTLheadclose.alpha = 0
		RTLhead.alpha = 0
		RTLtorso.alpha = 0
		RTLlegs.alpha = 0
		RTLheadangryhurt.alpha = 0
		RTLtorsohurt.alpha = 0
		RTLlegshurt.alpha = 0
	end
	--Changement de sprite par transparence de sprite
	--Utilisable : angry, angryhurt, attacked, close, happy, hurt, normal
	if GetGlobal("RTL") != "" then
		if GetGlobal("RTL") == "hurt" or GetGlobal("RTL") == "angryhurt" then	
			RTLtorsohurt.alpha = 1
			RTLlegshurt.alpha = 1
			RTLtorso.alpha = 0
			RTLlegs.alpha = 0
			RTLheadangry.alpha = 0
			RTLheadattacked.alpha = 0
			RTLheadclose.alpha = 0
			RTLheadhappy.alpha = 0
			RTLhead.alpha = 0
			if GetGlobal("RTL") == "hurt" then
				RTLheadhurt.alpha = 1
				RTLheadangryhurt.alpha = 0
			else
				RTLheadangryhurt.alpha = 1
				RTLheadhurt.alpha = 0
			end
		else	
			RTLtorsohurt.alpha = 0
			RTLlegshurt.alpha = 0
			RTLtorso.alpha = 1
			RTLlegs.alpha = 1
			
			RTLheadangryhurt.alpha = 0
			RTLheadhurt.alpha = 0
			if GetGlobal("RTL") == "angry" then
				RTLheadangry.alpha = 1
				RTLheadattacked.alpha = 0
				RTLheadclose.alpha = 0
				RTLheadhappy.alpha = 0
				RTLhead.alpha = 0
			elseif GetGlobal("RTL") == "attacked" then
				RTLheadangry.alpha = 0
				RTLheadattacked.alpha = 1
				RTLheadclose.alpha = 0
				RTLheadhappy.alpha = 0
				RTLhead.alpha = 0
			elseif GetGlobal("RTL") == "close" then
				RTLheadangry.alpha = 0
				RTLheadattacked.alpha = 0
				RTLheadclose.alpha = 1
				RTLheadhappy.alpha = 0
				RTLhead.alpha = 0
				RTLheadspared.alpha = 0
				RTLtorsospared.alpha = 0
				RTLlegsspared.alpha = 0
			elseif GetGlobal("RTL") == "happy" then
				RTLheadangry.alpha = 0
				RTLheadattacked.alpha = 0
				RTLheadclose.alpha = 0
				RTLheadhappy.alpha = 1
				RTLhead.alpha = 0
			elseif GetGlobal("RTL") == "normal" then
				RTLheadangry.alpha = 0
				RTLheadattacked.alpha = 0
				RTLheadclose.alpha = 0
				RTLheadhappy.alpha = 0
				RTLhead.alpha = 1
			end
		end			
		SetGlobal("RTL", "")
	end
	--Animation de "tf"
	if GetGlobal("animDilate") == true then
		if avancementDilate < 101 then
			RTLheadangryhurt.Scale(1+0.04*avancementDilate, 1)
			RTLtorsohurt.Scale(1+0.04*avancementDilate, 1)
			RTLlegshurt.Scale(1+0.04*avancementDilate, 1)
			blank.alpha = (1/50)*avancementDilate
		else
			if avancementDilate == 101 then
				RTLheadangryhurt.Scale(1, 1)
				RTLtorsohurt.Scale(1, 1)
				RTLlegshurt.Scale(1, 1)
				RTLheadangryhurt.alpha = 0
				RTLtorsohurt.alpha = 0
				RTLlegshurt.alpha = 0
				SetGlobal("Lukark","normal")
				SetGlobal("revived", true)
			end
			blank.alpha = (1/50)*(200-avancementDilate)
		end
		blank.SendToTop()
		avancementDilate = avancementDilate + 1
		if avancementDilate == 200 then
			SetGlobal("animDilate", false)
			avancementDilate = 0
			SetGlobal("StopLukark", false)
			SetGlobal("revived", true)
			blank.alpha = 0
		end
	end
	temp_anim = Time.time - animPhaseCut
	--Si l'animation n'est pas stoppée
	if not GetGlobal("StopRTL") and not GetGlobal("hitAnimRTL") then
		RTLheadhurt.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLheadangry.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLheadangryhurt.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLtorsohurt.MoveTo(0, 1*math.sin(5*temp_anim))
		RTLheadattacked.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLheadhappy.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLheadclose.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLhead.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLtorso.MoveTo(0, 1*math.sin(5*temp_anim))
		RTLheadspared.MoveTo(0, 2*math.sin(5*temp_anim))
		RTLtorsospared.MoveTo(0, 1*math.sin(5*temp_anim))
	--Animation de touche
	elseif not GetGlobal("StopRTL") and GetGlobal("hitAnimRTL") then
		if (hitAnimCount/4) % 2 >= 1 and (hitAnimCount/4) % 2 < 2 then
			RTLlegs.MoveTo(320-(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLlegshurt.MoveTo(320-(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLlegsspared.MoveTo(320-(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLheadhurt.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadangry.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadangryhurt.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorsohurt.MoveTo(0, 1*math.sin(5*temp_anim))
			RTLheadattacked.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadhappy.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadclose.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLhead.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorso.MoveTo(0, 1*math.sin(5*temp_anim))
			RTLheadspared.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorsospared.MoveTo(0, 1*math.sin(5*temp_anim))
		else
			RTLlegs.MoveTo(320+(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLlegshurt.MoveTo(320+(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLlegsspared.MoveTo(320+(10-hitAnimCount/4), 240+2*math.sin(5*temp_anim))
			RTLheadhurt.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadangry.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadangryhurt.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorsohurt.MoveTo(0, 1*math.sin(5*temp_anim))
			RTLheadattacked.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadhappy.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLheadclose.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLhead.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorso.MoveTo(0, 1*math.sin(5*temp_anim))
			RTLheadspared.MoveTo(0, 2*math.sin(5*temp_anim))
			RTLtorsospared.MoveTo(0, 1*math.sin(5*temp_anim))
		end
		hitAnimCount = hitAnimCount + 1
		if hitAnimCount == 40 then
			hitAnimCount = 0
			SetGlobal("hitAnimRTL", false)
		end
	end
	if GetGlobal("placingRTL") >= 0.1 and (temp_anim % (2*math.pi/5) >= (GetGlobal("placingRTL") - 0.2) and temp_anim % (2*math.pi/5) <= (GetGlobal("placingRTL") + 0.1))then
	    SetGlobal("StopRTL", true)
		temp_stopanimtime = GetGlobal("placingRTL")
		SetGlobal("placingRTL",-2)
	elseif GetGlobal("placingRTL") == -1 then
		SetGlobal("StopRTL", false)
		animPhaseCut = Time.time - temp_stopanimtime
		SetGlobal("placingRTL",-2)
	end
end