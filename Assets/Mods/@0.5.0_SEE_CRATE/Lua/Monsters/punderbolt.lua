comments = {"Punderbolt."}
commands = {"Pun", "der", "bolt"}
randomdialogue = {"Punderbolt."}
sprite = "emptypunder" --Always PNG. Extension is added automatically.
name = "Punderbolt"
hp = 1000
atk = 2
def = 0
check = "One of the Overworld Creator's\nmany forms.\nCalled RTL too."
dialogbubble = "rightwide" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
voice = "punderbolt"

function HandleAttack(attackstatus)
	Animate("Death/1")
	Encounter["enemies"][2].Call("SetBubbleOffset", {0, 6})
end

function LaunchFade(begin) Encounter.Call("LaunchFade", begin) end

function Unpause()
	if not unpaused then
		Audio.LoadFile("thechoice")
		unpaused = true
		Audio.Play()
	else
		Audio.Unpause()
	end
end

function Pause() Audio.Pause() end

function SetBubble(bubble) dialogbubble = bubble end

function GetCloser()  Encounter["getcloser1"] = true end
function GetCloser2() Encounter["getcloser2"] = true end
function GetFurther() Encounter["getfurther"] = true end

function Happening() Audio.PlaySound("happening") end

function Animate(animation)
	if safe and string.sub(animation, 1, 5) == "Death" then
		local split = string.split(animation, "/")
		animation = split[1] .. "/Safe/" .. split[2]
	end
	Encounter["enemies"][1].Call("SetSprite", "Punderbolt/" .. animation)
end

function forceattack(number, damage) Player.ForceAttack(number, damage) end

function AnimEnd()  
	Audio.PlaySound("happening")
	Encounter.Call("LaunchFade", {false, true})
end

function WindowClose() Audio.PlaySound("hitsound") Misc.DestroyWindow() end

function HandleCustomCommand(command) end

function string.split(inputstr, sep)
	local t = { }
	for str in string.gmatch(inputstr, "([^" .. sep .. "]+)") do
		table.insert(t, str)
	end
	return t
end
