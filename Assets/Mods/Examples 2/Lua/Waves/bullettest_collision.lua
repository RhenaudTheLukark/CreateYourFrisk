--[[bullets = {}
n = 4
local rotationcoeff = 0
if math.floor(n/4) == 0 then rotationcoeff = 90
else                         rotationcoeff = 90 / math.floor(n/4)
end
rotationcoeff = math.floor(n/4) == 0 and 90 or 90 / math.floor(n/4)
--timer = 0;
for i = 1, n do
    local bullet
    if i % 4 == 1 then	    bullet = CreateProjectile('Cross', Arena.width*9/16, Arena.height*9/16)
	elseif i % 4 == 2 then  bullet = CreateProjectile('Cross', -Arena.width*9/16, Arena.height*9/16)
	elseif i % 4 == 3 then  bullet = CreateProjectile('Cross', -Arena.width*9/16, -Arena.height*9/16)
	else                    bullet = CreateProjectile('Cross', Arena.width*9/16, -Arena.height*9/16)
	end
	bullet.sprite.rotation = rotationcoeff * math.floor(i/4)
	table.insert(bullets, bullet)
end

function Update()
    for i = 1, #bullets do
	    bullets[i].sprite.rotation = bullets[i].sprite.rotation + 1
		if timer % 120 == 0 then
		    bullets[i].sprite.SetAnimation({"fball+", "Cross"}, 1/20)
		elseif timer % 120 == 60 then
		    bullets[i].sprite.StopAnimation()
		end
	end
    --timer = timer + 1
end]]

bullet = CreateProjectile('fball+', Arena.width/4, Arena.height/4)
bullet.sprite.Scale(0.5, 0.5)

toggleRot = true

function Update()
    if toggleRot then bullet.sprite.rotation = bullet.sprite.rotation + 1 end
	if Input.GetKey("G") == 1 then
	    bullet.sprite.Scale(2,2)
	elseif Input.GetKey("L") == 1 then 
	    bullet.sprite.Scale(0.5,0.5)
	elseif Input.GetKey("A") == 1 then  bullet.sprite.Scale(1, 1)
	elseif Input.GetKey("Z") == 1 then  bullet.sprite.Scale(1, -1)
	elseif Input.GetKey("Q") == 1 then  bullet.sprite.Scale(-1, 1)
	elseif Input.GetKey("S") == 1 then  bullet.sprite.Scale(-1, -1)
	elseif Input.GetKey("R") == 1 then  toggleRot = not toggleRot
	end
end

function OnHit(bullet)
    if not Player.isHurting then
        --Player.hp = Player.hp/10
		--Player.Hurt(0, 1)
		Player.Hurt(5)
    end
end