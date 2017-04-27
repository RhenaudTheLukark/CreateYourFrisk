local xDir = (Input.Left>0 and Input.Right>0) and 0 or Input.Right>0 and 1 or Input.Left>0 and -1 or 0
local yDir = (Input.Up>0 and Input.Down>0) and 0 or Input.Up>0 and 1 or Input.Down>0 and -1 or 0
heart_mask=CreateProjectile("ut+heart",0,0)
Encounter["wavetimer"]=math.huge

function Update()
    heart_mask.MoveToAbs(Player.absx,Player.absy)
end

function OnHit()

end