comments = {"Mionn is."}
commands = {}
randomdialogue = {"[effect:none][voice:mionn]I'm talking."}
sprite = "Mionn/transit_to_fight1"
name = "Mionn"
hp = 3000
atk = 20
def = -15
check = ""
dialogbubble = "rightlargeminus"
cancheck = false
canspare = false
voice = "mionn"
timer = 0

function Update()
	timer = timer + Time.mult
	while timer >= 1 do
		timer = timer - 1
		RunAnimations()
	end
end

function HandleAttack(attackstatus)
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function HandleCustomCommand(command) end

anim_running = false
anim_frame = 1
anim_timer = 0
anim_fps = 7
anim_timer_max = math.ceil(60/anim_fps)
anim_timer = anim_timer_max
anim_length = 1
anim_frames = {}
anim_rep = false

function Animate()
	anim_fps = 10
	anim_frames = {"Mionn/transit_to_fight1", "Mionn/transit_to_fight2", "Mionn/transit_to_fight3", "Mionn/transit_to_fight4"}
	anim_rep = false
	anim_length = 4
	anim_frame = 1
	anim_timer_max = math.ceil(60/anim_fps)
	anim_timer = 0
	anim_running = true
end

function RunAnimations()
	if anim_running == true then
		if anim_timer <= 0 then
			anim_timer = anim_timer_max
			SetSprite(anim_frames[anim_frame])
			anim_frame = anim_frame + 1
			if anim_frame > anim_length then
			    anim_running = false
			end
		end
		anim_timer = anim_timer - 1
	end
end