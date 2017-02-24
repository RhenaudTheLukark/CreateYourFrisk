bullet = CreateProjectile("line_lazor2",0,0)
bullet.sprite.Scale(1,1)

function OnHit(bullet)
    DEBUG("PP COLLISION")
end

function Update()
    if Input.GetKey("Z") == 1 then      bullet.sprite.yscale = bullet.sprite.yscale * 2
	elseif Input.GetKey("X") == 1 then  bullet.sprite.yscale = bullet.sprite.yscale / 2
	elseif Input.GetKey("D") == 1 then  bullet.sprite.xscale = bullet.sprite.xscale * 2
	elseif Input.GetKey("Q") == 1 then  bullet.sprite.xscale = bullet.sprite.xscale / 2
	elseif Input.GetKey("O") == 2 then  bullet.sprite.rotation = bullet.sprite.rotation + 1
	elseif Input.GetKey("P") == 2 then  bullet.sprite.rotation = bullet.sprite.rotation - 1
	elseif Input.GetKey("T") == 2 then  bullet.sprite.Scale(1, 1)
	elseif Input.GetKey("Y") == 2 then  bullet.sprite.Scale(1, -1)
	elseif Input.GetKey("G") == 2 then  bullet.sprite.Scale(-1, 1)
	elseif Input.GetKey("H") == 2 then  bullet.sprite.Scale(-1, -1)
	end
    DEBUG("Placeholder")
end