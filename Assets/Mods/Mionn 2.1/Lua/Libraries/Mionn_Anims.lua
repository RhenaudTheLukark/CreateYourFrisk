-- Set of animations
-- {fps,sprite_set,repeat}

health_state = 0
function LoadAnims()

Anim_To_Fight = {10,
{"Stances"..health_state.."/transit_to_fight1",
"Stances"..health_state.."/transit_to_fight2",
"Stances"..health_state.."/transit_to_fight3",
"Stances"..health_state.."/transit_to_fight4"},
false}
Anim_From_Fight = {10,
{"Stances"..health_state.."/transit_to_fight4",
"Stances"..health_state.."/transit_to_fight3",
"Stances"..health_state.."/transit_to_fight2",
"Stances"..health_state.."/transit_to_fight1"},
false}
Anim_To_Side = {8,
{"Stances"..health_state.."/transit_to_side1",
"Stances"..health_state.."/transit_to_side2",
"Stances"..health_state.."/transit_to_side3"},
false}
Anim_To_Side_Inv = {8,
{"Stances"..health_state.."/transit_to_side3",
"Stances"..health_state.."/transit_to_side2",
"Stances"..health_state.."/transit_to_side1"},
false}
Anim_To_Down = {8,
{"Stances"..health_state.."/trans_down1",
"Stances"..health_state.."/trans_down2",
"Stances"..health_state.."/trans_down3"},
false}
Anim_To_Down_Inv = {8,
{"Stances"..health_state.."/trans_down3",
"Stances"..health_state.."/trans_down2",
"Stances"..health_state.."/trans_down1"},
false}
Anim_To_Firewall_Mid = {8,
{"Stances"..health_state.."/trans_firewall1",
"Stances"..health_state.."/trans_firewall2",
"Stances"..health_state.."/trans_firewall3"},
false}
Anim_From_Firewall_Mid = {8,
{"Stances"..health_state.."/trans_firewall3",
"Stances"..health_state.."/trans_firewall2",
"Stances"..health_state.."/trans_firewall1"},
false}
Anim_From_Firewall = {8,
{"Stances"..health_state.."/trans_firewall3",
"Stances"..health_state.."/trans_firewall2",
"Stances"..health_state.."/transit_to_side3",
"Stances"..health_state.."/transit_to_side2",
"Stances"..health_state.."/transit_to_side1"},
false}
Anim_Hurt = {8,
{"Stances"..health_state.."/hurt1",
"Stances"..health_state.."/hurt2",
"Stances"..health_state.."/hurt3",
"Stances"..health_state.."/hurt4",
"Stances"..health_state.."/hurt5",
"Stances"..health_state.."/hurt6",
"Stances"..health_state.."/hurt7",
"Stances"..health_state.."/hurt8"},
false}
Anim_Bet = {12,
{"Final/bet1",
"Final/bet2",
"Final/bet3",
"Final/bet4",
"Final/bet5",
"Final/bet6",
"Final/bet7",
"Final/bet8",
"Final/bet9",
"Final/bet10"
},
false}
Anim_Cry = {4,
{"Final/cry_an1",
"Final/cry_an2",
"Final/cry_an3",
"Final/cry_an4"
},
true}
Anim_Collapse = {8,
{"Stances3/hurt1",
"Stances3/hurt3",
"Stances3/hurt4",
"Stances3/hurt5",
"Stances3/hurt6",
"Stances3/hurt8",
"Final/collapse1",
"Final/collapse2",
"Final/collapse3",
"Final/collapse4",
"Final/collapse5",
"Final/collapse6",
"Final/collapse7",
"Final/collapse8",
"Final/collapse9",
"Final/collapse10",
"Final/collapse11",
"Final/collapse12"},
false}
Anim_Drip = {8,
{"Final/dripping1",
"Final/dripping1",
"Final/dripping1",
"Final/dripping1",
"Final/dripping1",
"Final/dripping2",
"Final/dripping3",
"Final/dripping4",
"Final/dripping5"},
true}
Anim_Float = {5,
{"Final/detend1",
"Final/detend2"},
true}

end

LoadAnims()

anim_frame = 1
anim_timer = 0
anim_fps = 7
anim_timer_max = math.ceil(60/anim_fps)
anim_timer = anim_timer_max
anim_running = false
anim_length = 1
anim_frames = {}
anim_rep = false

function StopAnimate()
	anim_running = false
end

function Animate(Animation)
	anim_fps = Animation[1]
	anim_frames = Animation[2]
	anim_rep = Animation[3]
	anim_length = #anim_frames
	anim_frame = 1
	anim_timer_max = math.ceil(60/anim_fps)
	anim_timer = 0
	anim_running = true
end

function RunAnimations()
	if anim_running == true then
		if anim_timer <= 0 then
			anim_timer = anim_timer_max
			enemies[1].Call("SetSprite",anim_frames[anim_frame])
			anim_frame = anim_frame + 1
			if anim_frame > anim_length then
				if anim_rep == true then
					anim_frame = 1
				else
					anim_running = false
					if final_anim == true then
						Animate(Anim_Drip)
					end
					if cry_anim == true then
						Animate(Anim_Cry)
						cry_anim = nil
					end
				end
			end
		end
		anim_timer = anim_timer - 1
	end
end

-- LIFTOFF

flash_max = 200
flash = 0

function CreateFlash()
	if FLASH ~= nil and FLASH.isactive then
		FLASH.Remove()
	end
	FLASH = CreateProjectileAbs("flash",320,240)
	FLASH.SetVar('safe',true)
	FLASH.sprite.alpha = math.sin(flash/flash_max * math.pi)
end

max_volume = 0.75

function RunFlash()
	if flash > 0 then
		flash = flash - 1
		CreateFlash()
		FLASH.sprite.alpha = math.sin(flash/flash_max * math.pi)
		Audio.Volume( (1 - math.sin(flash/flash_max * math.pi)) * max_volume )
		FLASH.sprite.SendToTop()
		if flash == 100 then
			if action_this_turn == "flirt" then
				nextwaves = {"FU"}
			elseif DET >= 100 then
				nextwaves = {"DET_final"} -- PLACEHOLDER
			end
			if liftoff == 1 then
				Animate(Anim_Float)
				liftoff = 2
			end
			if liftoff == 2 then
				liftoff = 3
				State("DEFENDING")
			end
			if collapse == 1 then
				if action_this_turn == "flirt" then
					melt_type = "_panic"
				else
					melt_type = ""
				end
				Flame_Remove()
				enemies[1].Call("SetSprite","Final/melt"..melt_type.."1")
				Audio.LoadFile("ded")
				max_volume = 0.2
				Audio.Pitch(-0.25)
				anim_rep = false
				collapse = 2
			end
		end
	end
end