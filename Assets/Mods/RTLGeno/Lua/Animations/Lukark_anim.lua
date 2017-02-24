SetGlobal("StopLukark",false)
SetGlobal("hitAnimLukark", false)
revived2nd = false
hitAnimCount = 0
SetGlobal("animPhaseCut",Time.time)

hair = CreateSprite("Lukark/hair/1")
head = CreateSprite("Lukark/headnormal")
headangry = CreateSprite("Lukark/headangry")
headmad = CreateSprite("Lukark/headmad")
headhurt = CreateSprite("Lukark/headhurt")
headhappy = CreateSprite("Lukark/headhappy")
headsmiling = CreateSprite("Lukark/headsmiling")
legs = CreateSprite("Lukark/legs")
torso = CreateSprite("Lukark/torso")
arms = CreateSprite("Lukark/arms/1")


legs.SetAnimation({"Lukark/legs", "Lukark/legs"}, 60)
torso.SetAnimation({"Lukark/torso", "Lukark/torso"}, 60)
head.SetAnimation({"Lukark/headnormal", "Lukark/headnormal"}, 60)
headangry.SetAnimation({"Lukark/headangry", "Lukark/headangry"}, 60)
headhappy.SetAnimation({"Lukark/headhappy", "Lukark/headhappy"}, 60)
headhurt.SetAnimation({"Lukark/headhurt", "Lukark/headhurt"}, 60)
headmad.SetAnimation({"Lukark/headmad", "Lukark/headmad"}, 60)
headsmiling.SetAnimation({"Lukark/headsmiling", "Lukark/headsmiling"}, 60)
arms.SetAnimation({"Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1",
				   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2",
				   "Lukark/arms/3","Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3",
				   "Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3","Lukark/arms/3",
				   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2"}
				   , 1/25)
--arms.SetAnimation({"Lukark/arms/ballmove1","Lukark/arms/ballmove2","Lukark/arms/ballmove3","Lukark/arms/ballmove4","Lukark/arms/ballmove5",
--				   "Lukark/arms/ball2","Lukark/arms/ball2","Lukark/arms/ball2","Lukark/arms/ball2","Lukark/arms/ball2",
--				   "Lukark/arms/ballmove5","Lukark/arms/ballmove4","Lukark/arms/ballmove3","Lukark/arms/ballmove2","Lukark/arms/ballmove1",
--				   "Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1"}, 1/25)
hair.SetAnimation({"Lukark/hair/1","Lukark/hair/2","Lukark/hair/3",
				   "Lukark/hair/4","Lukark/hair/3","Lukark/hair/2"}
				   , 1/5)

legs.x = 120
legs.y = 340
arms.x = 320
arms.y = 340
hair.x = 320
hair.y = 340
head.x = 320
head.y = 340
headangry.x = 320
headangry.y = 340
headmad.x = 320
headmad.y = 340
headhurt.x = 320
headhurt.y = 340
headhappy.x = 320
headhappy.y = 340
headsmiling.x = 320
headsmiling.y = 340
torso.x = 320
torso.y = 340

head.SetPivot(0.5, 0.5)
headangry.SetPivot(0.5, 0.5)
headmad.SetPivot(0.5, 0.5)
headhurt.SetPivot(0.5, 0.5)
headhappy.SetPivot(0.5, 0.5)
headsmiling.SetPivot(0.5, 0.5)
arms.SetPivot(0.5, 0.5)
torso.SetPivot(0.5, 0.5)
torso.SetAnchor(0.5, 0.5)
legs.SetPivot(0.5, 0.5)
hair.SetPivot(0.5, 0.5)


function AnimateLukark()
	--Séquence de mort
	if GetGlobal("LukarkDead") == true then 
		if headhappy.alpha == 1 then
			SetGlobal("StopLukark",true)
		end
		headhappy.alpha = headhappy.alpha - 0.1
		torso.alpha = torso.alpha - 0.1
		legs.alpha = legs.alpha - 0.1
		arms.alpha = arms.alpha - 0.1
		hair.alpha = arms.alpha - 0.1
	end
	
	--Changement de sprite par transparence de sprite
	--Utilisable : angry, happy, hurt, mad, normal, smiling
	--Utiliser "hidden" pour le cacher entièrement.
	if GetGlobal("Lukark") != "" then
		if GetGlobal("Lukark") == "hidden" then
			torso.alpha = 0
			legs.alpha = 0
			arms.alpha = 0
			hair.alpha = 0
			head.alpha = 0
			headangry.alpha = 0
			headhappy.alpha = 0
			headhurt.alpha = 0
			headmad.alpha = 0
			headsmiling.alpha = 0
		else
			if GetGlobal("Lukark") == "angry" then
				head.alpha = 0
				headangry.alpha = 1
				headhappy.alpha = 0
				headhurt.alpha = 0
				headmad.alpha = 0
				headsmiling.alpha = 0
			elseif GetGlobal("Lukark") == "happy" then
				head.alpha = 0
				headangry.alpha = 0
				headhappy.alpha = 1
				headhurt.alpha = 0
				headmad.alpha = 0
				headsmiling.alpha = 0
			elseif GetGlobal("Lukark") == "hurt" then
				head.alpha = 0
				headangry.alpha = 0
				headhappy.alpha = 0
				headhurt.alpha = 1
				headmad.alpha = 0
				headsmiling.alpha = 0
			elseif GetGlobal("Lukark") == "mad" then
				head.alpha = 0
				headangry.alpha = 0
				headhappy.alpha = 0
				headhurt.alpha = 0
				headmad.alpha = 1
				headsmiling.alpha = 0
			elseif GetGlobal("Lukark") == "normal" then
				head.alpha = 1
				headangry.alpha = 0
				headhappy.alpha = 0
				headhurt.alpha = 0
				headmad.alpha = 0
				headsmiling.alpha = 0
				torso.alpha = 1 
				legs.alpha = 1
				arms.alpha = 1
				hair.alpha = 1
				if revived2nd == false then
			        SetGlobal("revived", false)
					revived2nd = true
				end
				arms.SetAnimation({"Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1",
								   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2",
								   "Lukark/arms/3","Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3",
								   "Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3","Lukark/arms/3",
								   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2"}, 1/25)
			elseif GetGlobal("Lukark") == "smiling" then
				head.alpha = 0
				headangry.alpha = 0
				headhappy.alpha = 0
				headhurt.alpha = 0
				headmad.alpha = 0
				headsmiling.alpha = 1
			end
			if GetGlobal("Lukark") == "waveballbegin" then
				arms.SetAnimation({"Lukark/arms/ballmove1","Lukark/arms/ballmove2","Lukark/arms/ballmove3", "Lukark/arms/ball1-1"}, 1/10)
			elseif GetGlobal("Lukark") == "waveballend" then
				arms.SetAnimation({"Lukark/arms/ballmove3","Lukark/arms/ballmove2","Lukark/arms/ballmove1", "Lukark/arms/1", 
								   "Lukark/arms/1", "Lukark/arms/1", "Lukark/arms/1", "Lukark/arms/1"}, 1/10)
				
			elseif GetGlobal("Lukark") == "ball1-1" then
				arms.SetAnimation({"Lukark/arms/ball1-1","Lukark/arms/ball1-1"}, 60)
			elseif GetGlobal("Lukark") == "ball1-2" then
				arms.SetAnimation({"Lukark/arms/ball1-2","Lukark/arms/ball1-2"}, 60)
			elseif GetGlobal("Lukark") == "ball2-1" then
				arms.SetAnimation({"Lukark/arms/ball2-1","Lukark/arms/ball2-1"}, 60)
			elseif GetGlobal("Lukark") == "ball2-2" then
				arms.SetAnimation({"Lukark/arms/ball2-2","Lukark/arms/ball2-2"}, 60)
				
			elseif GetGlobal("Lukark") == "waveball1-1to2" then
				arms.SetAnimation({"Lukark/arms/ballmove1-4","Lukark/arms/ballmove1-5","Lukark/arms/ball1-2",
								   "Lukark/arms/ball1-2","Lukark/arms/ball1-2","Lukark/arms/ball1-2"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball1-2to1" then
				arms.SetAnimation({"Lukark/arms/ballmove1-5","Lukark/arms/ballmove1-4","Lukark/arms/ball1-1",
								   "Lukark/arms/ball1-1","Lukark/arms/ball1-1","Lukark/arms/ball1-1"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2-1to2" then
				arms.SetAnimation({"Lukark/arms/ballmove2-4","Lukark/arms/ballmove2-5","Lukark/arms/ball2-2",
								   "Lukark/arms/ball2-2","Lukark/arms/ball2-2","Lukark/arms/ball2-2"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2-2to1" then
				arms.SetAnimation({"Lukark/arms/ballmove2-5","Lukark/arms/ballmove2-4","Lukark/arms/ball2-1",
								   "Lukark/arms/ball2-1","Lukark/arms/ball2-1","Lukark/arms/ball2-1"}, 1/10)
								   
			elseif GetGlobal("Lukark") == "waveball1to2-1" then
				arms.SetAnimation({"Lukark/arms/ballmove4-1","Lukark/arms/ballmove5-1","Lukark/arms/ball2-1",
								   "Lukark/arms/ball2-1","Lukark/arms/ball2-1","Lukark/arms/ball2-1"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball1to2-2" then
				arms.SetAnimation({"Lukark/arms/ballmove4-2","Lukark/arms/ballmove5-2","Lukark/arms/ball2-2",
								   "Lukark/arms/ball2-2","Lukark/arms/ball2-2","Lukark/arms/ball2-2"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2to1-1" then
				arms.SetAnimation({"Lukark/arms/ballmove5-1","Lukark/arms/ballmove4-1","Lukark/arms/ball1-1",
								   "Lukark/arms/ball1-1","Lukark/arms/ball1-1","Lukark/arms/ball1-1"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2to1-2" then
				arms.SetAnimation({"Lukark/arms/ballmove5-2","Lukark/arms/ballmove4-2","Lukark/arms/ball1-2",
								   "Lukark/arms/ball1-2","Lukark/arms/ball1-2","Lukark/arms/ball1-2"}, 1/10)
								   
			elseif GetGlobal("Lukark") == "waveball1to2-1to2" then
				arms.SetAnimation({"Lukark/arms/ballmove4-4","Lukark/arms/ballmove5-5","Lukark/arms/ball2-2",
								   "Lukark/arms/ball2-2","Lukark/arms/ball2-2","Lukark/arms/ball2-2"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball1to2-2to1" then
				arms.SetAnimation({"Lukark/arms/ballmove4-5","Lukark/arms/ballmove5-4","Lukark/arms/ball2-1",
								   "Lukark/arms/ball2-1","Lukark/arms/ball2-1","Lukark/arms/ball2-1"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2to1-1to2" then
				arms.SetAnimation({"Lukark/arms/ballmove5-4","Lukark/arms/ballmove4-5","Lukark/arms/ball1-2",
								   "Lukark/arms/ball1-2","Lukark/arms/ball1-2","Lukark/arms/ball1-2"}, 1/10)
			elseif GetGlobal("Lukark") == "waveball2to1-2to1" then
				arms.SetAnimation({"Lukark/arms/ballmove5-5","Lukark/arms/ballmove4-4","Lukark/arms/ball1-1",
								   "Lukark/arms/ball1-1","Lukark/arms/ball1-1","Lukark/arms/ball1-1"}, 1/10)	   
			end	
		end
		SetGlobal("Lukark", "")
	end
	if GetGlobal("animPhaseCut") != nil then
		temp_anim = Time.time - GetGlobal("animPhaseCut")
	end
	--Si l'animation n'est pas stoppée
	if not GetGlobal("StopLukark") and not GetGlobal("hitAnimLukark") then
		legs.Scale(1, 1+0.05*math.sin(temp_anim*2))
		legs.MoveTo(320, 340+(5.15*math.sin(temp_anim*2)))
		arms.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
		head.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		headangry.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		headhappy.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		headhurt.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		headmad.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		headsmiling.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
		hair.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
		torso.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
	elseif not GetGlobal("StopLukark") and GetGlobal("hitAnimLukark") then
		if (hitAnimCount/4) % 2 >= 1 and (hitAnimCount/4) % 2 < 2 then
			legs.Scale(1, 1+0.05*math.sin(temp_anim*2))
			legs.MoveTo(320-(10-hitAnimCount/4), 340+(5.15*math.sin(temp_anim*2)))
			arms.MoveTo(320-(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
			head.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headangry.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headhappy.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headhurt.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headmad.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headsmiling.MoveTo(321-(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			hair.MoveTo(320-(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
			torso.MoveTo(320-(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
		else
			legs.Scale(1, 1+0.05*math.sin(temp_anim*2))
			legs.MoveTo(320+(10-hitAnimCount/4), 340+(5.15*math.sin(temp_anim*2)))
			arms.MoveTo(320+(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
			head.MoveTo(320+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headangry.MoveTo(321+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headhappy.MoveTo(321+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headhurt.MoveTo(321+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headmad.MoveTo(321+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			headsmiling.MoveTo(321+(10-hitAnimCount/4), 342+(5.15*math.sin(temp_anim*2)))
			hair.MoveTo(320+(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
			torso.MoveTo(320+(10-hitAnimCount/4), 341+(5.15*math.sin(temp_anim*2)))
		end
		hitAnimCount = hitAnimCount + 1
		if hitAnimCount == 40 then
			hitAnimCount = 0
			SetGlobal("hitAnimLukark", false)
		end
	end
end