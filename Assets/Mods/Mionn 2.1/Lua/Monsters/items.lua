alt = 2 --Determines what page it is on. The code uses "alt%2", so this would return "0". If it was an odd number, it'd return "1".
commands = {"Pie","C. Bun","L. Hero","L. Hero"}
commands2 = {"SnowPiece","SnowPiece","MeatBun"}
-- This is VERY important:
-- You can ONLY have 4 items per page!
-- "commands" handles the CURRENT page. "commands2" handles the page that is off-screen.
-- If you have more than 4 on EITHER ONE, things could get glitchy! Watch out.
-- If you want to have less than 8, subtract from "commands2" first!
-- Otherwise my code will do it automatically.
sprite = "empty" --doesn't have to be THIS sprite, just a blank one.
name = "null" --doesn't matter anyway, you will never see it.
hp = 1 --once again, doesn't matter.
def = math.huge --still doesn't matter.
SetActive(false) -- but THIS does.
function HandleCustomCommand(command)
	Encounter["in_menus"] = false
	Encounter.Call("Remove_Menu_Attacks")
	if command == "L. HERO" then -- Not all items have to be food!
		BattleDialog({"You ate the Legendary Hero.[w:10]\n"..HealAndReturnString(40,"heal").."[w:10]\nATTACK increased by 4!"})
		Encounter.Call("LegHero")
		RemoveCommandByName(command)
	elseif command == "BUTTSPIE" or command == "PIE" then --These are essentially just ACT commands.
		BattleDialog({"You ate the Butterscotch Pie.[w:10]\n"..HealAndReturnString(99,"heal")}) --"HealAndReturnString": explained below, in its section.
		RemoveCommandByName(command) --Also explained in its section.
	elseif command == "MEATBUN" then
		BattleDialog({"You ate half of the Meatbun.[w:10]\n"..HealAndReturnString(20,"heal")}) --The bisicle is an example of swapping items.
		SwapCommandByNames("MEATBUN","HalfBun")
	elseif command == "HALFBUN" then
		BattleDialog({"You ate the other half of the\rMeatbun[w:10]\n"..HealAndReturnString(20,"heal")})
		RemoveCommandByName(command)
	elseif command == "CINNABUN" or command == "C. BUN" then
		BattleDialog({"You ate the Cinnamon Bunny.[w:10]\n"..HealAndReturnString(22,"heal")})
		RemoveCommandByName(command)
	elseif command == "SNOWPIECE" then
		BattleDialog({"You ate the Snowman Piece.[w:10]\n"..HealAndReturnString(45,"heal")})
		RemoveCommandByName(command)
	end
end
function HealAndReturnString(num,sound)
	local string = nil -- This code here determines whether or not to say "Your hp was maxed out" or "You recovered <num> hp".
	if Player.hp + num >= GetMaxHP() then
		string = "Your HP was maxed out!"
	else
		string = "You recovered "..num.." HP!"
	end
	Player.hp = Player.hp + num --If we just use "Player.heal(num)", it plays a singular healing sound.
	Audio.PlaySound(sound) --This allows for custom sounds, for things like the Sea Tea and Legendary Hero that have pre-merged healing sounds.
	return string
end
function GetMaxHP() --Does just what it says on the tin.
	if Player.lv < 20 then
		return 16 + (4 * Player.lv)
	elseif Player.lv == 20 then
		return 99
	end
end
function RemoveCommandByName(command) -- Also does just what it says on the tin.
	for k,v in pairs(commands) do
		if string.upper(v) == string.upper(command) then
			table.remove(commands,k)
		end
	end
end
function SwapCommandByNames(command,command2)
	local found = false -- essentially, it finds the first instance of "command"...
	for k,v in pairs(commands) do
		if string.upper(v) == command and found == false then
			found = true
			commands[k] = command2 -- and replaces it with "command2".
		end
	end
end
function SwapTables() --Once again, the title explains it. Swaps "commands" with "commands2".
	local storage = commands2
	commands2 = commands
	commands = storage
	alt = alt + 1
end