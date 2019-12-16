local update = Update
function Update()
    update()
    local speed = 2
    if Input.Cancel > 0 then speed = 1 end
    
    if ppos == nil then ppos = {320, 90 + 75} end
    Player.MoveToAbs(ppos[1] + ((Input.Right > 0 and speed or 0) - (Input.Left > 0 and speed or 0)), ppos[2] + ((Input.Up > 0 and speed or 0) - (Input.Down > 0 and speed or 0)), false)
    
    if Input.Up > 0 or Input.Down > 0 or Input.Left > 0 or Input.Right > 0 then
        ppos = {Player.absx, Player.absy}
    end
end
