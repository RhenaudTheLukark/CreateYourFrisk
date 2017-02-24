-- Variable Area --
AdvPlayerLib = {}
AdvPlayerLib.Arena = {}
AdvPlayerLib.Controls = {}
AdvPlayerLib.Speed = {}
AdvPlayerLib.Souls = {}
AdvPlayerLib.Souls.FakeSouls = {}
AdvPlayerLib.Souls.Count = 0
AdvPlayerLib.Size = {}
ir = 0
temppulsecount = 0
rtf = true

-- Speed --
function AdvPlayerLib.Speed.Faster(integer) -- Make Player Faster.
    if Input.Left > 0 then
        Player.MoveTo(Player.x-integer, Player.y, false)
    end
    if Input.Up > 0 then
        Player.MoveTo(Player.x, Player.y+integer, false)
    end
    if Input.Down > 0 then
        Player.MoveTo(Player.x, Player.y-integer, false)
    end
    if Input.Right > 0 then
        Player.MoveTo(Player.x+integer, Player.y, false)
    end
end
function AdvPlayerLib.Speed.Slower(integer) -- Make Player Slower
    integer = integer + 4

    if Input.Left > 0 then
        Player.MoveTo(Player.x+integer, Player.y, false)
    end
    if Input.Up > 0 then
        Player.MoveTo(Player.x, Player.y-integer, false)
    end
    if Input.Down > 0 then
        Player.MoveTo(Player.x, Player.y+integer, false)
    end
    if Input.Right > 0 then
        Player.MoveTo(Player.x-integer, Player.y, false)
    end
end

-- Controls --
function AdvPlayerLib.Controls.Invert() -- Invert Player Controls
    integer = 4
    Player.SetControlOverride(true)
    if Input.Left > 0 then
        Player.MoveTo(Player.x+integer, Player.y, true)
    end
    if Input.Up > 0 then
        Player.MoveTo(Player.x, Player.y-integer, true)
    end
    if Input.Down > 0 then
        Player.MoveTo(Player.x, Player.y+integer, true)
    end
    if Input.Right > 0 then
        Player.MoveTo(Player.x-integer, Player.y, true)
    end
end

-- Arena --
function AdvPlayerLib.Arena.Collision(tf,speed)
    if tf == false then
        rtf = true
    end
    if tf == true then
        rtf = false
    end
    Player.SetControlOverride(rtf)
    if Input.Up > 0 then
        Player.MoveTo(Player.x ,Player.y+speed, tf)
    end
    if Input.Down > 0 then
        Player.MoveTo(Player.x ,Player.y-speed, tf)
    end
    if Input.Left > 0 then
        Player.MoveTo(Player.x-s ,Player.y, tf)
    end
    if Input.Right > 0 then
        Player.MoveTo(Player.x+s ,Player.y, tf)
    end
end

-- Rotation --
function AdvPlayerLib.Rotate(degrees)
    Player.sprite.rotation = degrees
end

ps = 0
-- Size --
function AdvPlayerLib.Size.Pulse()
    ps = ps + 0.1
    Player.sprite.Scale(1+math.sin(ps)*0.2, 1+math.sin(ps)*0.2)
end

-- Size fuction with Player's HP --
function AdvPlayerLib.Size.PulseByHP()
	temppulsecount = temppulsecount + 1
	if temppulsecount >= math.sqrt(20 / Player.hp) then
        ps = ps + 0.1
        Player.sprite.Scale(1+math.sin(ps)*0.2, 1+math.sin(ps)*0.2)
		temppulsecount = temppulsecount - math.sqrt(20 / Player.hp)
	end
end

-- Fake Souls --

function AdvPlayerLib.Souls.AddFake(canhurt, rgbtable, positiontable, returnnumber) -- How to use : AdvPlayerLib.Souls.AddFake(true or false, {255, 255, 255}, {0, 0}, true or false)
    AdvPlayerLib.Souls.Count = AdvPlayerLib.Souls.Count + 1
    local fakesoul = CreateProjectile("ut-heart", positiontable[1], positiontable[2])
    fakesoul.sprite.color = {rgbtable[1]/255, rgbtable[2]/255, rgbtable[3]/255}
    if canhurt == true then
        table.insert(AdvPlayerLib.Souls.FakeSouls, fakesoul)
    elseif canhurt == false then

    end
    if returnnumber == true then
        return AdvPlayerLib.Souls.Count
    end
end
function AdvPlayerLib.Souls.GetFake(number)
    return AdvPlayerLib.Souls.FakeSouls[number]
end
