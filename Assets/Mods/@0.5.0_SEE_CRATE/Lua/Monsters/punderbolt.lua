comments = {"Punderbolt."}
commands = {"Pun", "der", "bolt"}
randomdialogue = {"Punderbolt."}
sprite = "emptypunder" --Always PNG. Extension is added automatically.
name = "Punderbolt"
hp = 1000
atk = 2
def = 0
check = "The Overworld Creator.\nCalled RTL too."
dialogbubble = "rightwide" -- See documentation for what bubbles you have available.
cancheck = true
canspare = false
voice = "punderbolt"

function HandleAttack(attackstatus) Animate("death1") end

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

function Pause()   Audio.Pause()   end

function LaunchAnim()
	Encounter["enemies"][3].Call("Hide")
	Encounter["enemies"][1]["monstersprite"].SetAnimation({"WDSpecial/1", "WDSpecial/2", "WDSpecial/3", "WDSpecial/2"}, 1/2)
	Encounter["enemies"][1]["monstersprite"].x = 20.5
end

function EndAnim()
	Encounter["enemies"][3].Call("Show")
	Encounter["enemies"][1]["monstersprite"].StopAnimation()
	Animate("happy")
	Encounter["enemies"][1]["monstersprite"].x = 0
end

function SetBubble(bubble) dialogbubble = bubble end

function GetCloser()  Encounter["getcloser1"] = true end
function GetCloser2() Encounter["getcloser2"] = true end
function GetFurther() Encounter["getfurther"] = true end

function Happening() Audio.PlaySound("happening") end

function Animate(animation) Encounter["enemies"][1].Call("SetSprite","Punderbolt/" .. animation) end

function forceattack(number, damage) Player.ForceAttack(number, damage) end

function AnimEnd()  
	Audio.PlaySound("happening")
    Encounter.Call("LaunchFade", {false, true})
end

function WindowClose() Audio.PlaySound("hitsound") Misc.DestroyWindow() end

function HandleCustomCommand(command) end