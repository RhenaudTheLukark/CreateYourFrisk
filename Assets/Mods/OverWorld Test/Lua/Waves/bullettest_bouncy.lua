-- The bouncing bullets attack from the documentation example.
spawntimer = 0
bullets = {}

function Update()
    spawntimer = spawntimer + 1
    if spawntimer%30 == 0 then
        local posx = 30 - math.random(60)
        local posy = Arena.height/2
        local bullet = CreateProjectile('bullet', posx, posy)
        bullet.SetVar('velx', 1 - 2*math.random())
        bullet.SetVar('vely', 0)
        table.insert(bullets, bullet)
    end
    
    for i=1,#bullets do
        local bullet = bullets[i]
        local velx = bullet.GetVar('velx')
        local vely = bullet.GetVar('vely')
        local newposx = bullet.x + velx
        local newposy = bullet.y + vely
        if(bullet.x > -Arena.width/2 and bullet.x < Arena.width/2) then
            if(bullet.y < -Arena.height/2 + 8) then 
                newposy = -Arena.height/2 + 8
                vely = 4
            end
        end
        vely = vely - 0.04
        bullet.MoveTo(newposx, newposy)
        bullet.SetVar('vely', vely)
    end
end