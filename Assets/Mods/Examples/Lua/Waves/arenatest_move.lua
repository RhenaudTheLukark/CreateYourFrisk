if isCYF then 
    a = CreateSprite("bullet", "WEEHEE")
    b = CreateSprite("bullet") 
	b.x = 200
	b.y = 240
    c = CreateSprite("bullet") 
	c.x = 440
	c.y = 240
	b.layer = "BelowUI"
	c.layer = "Top"
    bullet = CreateProjectile("bullet", Arena.width/2, 0)
	bullet.sprite.color32 = {0, 64, 255}
	bullet.layer = "After"
    timer = 0
else
    bullet = CreateProjectile("bullet", Arena.width/2, 0)
end

function Update()
	if isCYF then 
	    Arena.Move(1, 0, true, true)
	    --Arena.Move(1, 0, false)
        timer = timer + Time.dt
		if timer % 1 < 0.5 then
		    SetFrameBasedMovement(true)
			a.layer = "BelowArena"
		else
		    SetFrameBasedMovement(false)
			a.layer = "01001"            --Will do nothing
		end
		bullet.sprite.Scale(bullet.sprite.xscale + 0.02, bullet.sprite.xscale + 0.02)
		--bullet.sprite.rotation = bullet.sprite.rotation + 2
	end
end

function OnHit(bullet)
	if isCYF then 
        Player.setMaxHPShift(10, 0.0, true)
		--if not Player.isHurting then
		--    Player.setMaxHPShift(10, 1/5, false, true)
		--end
	end
end