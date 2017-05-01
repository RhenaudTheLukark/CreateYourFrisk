bullet = CreateProjectile("bullet",0,0)
bullet.sprite.Scale(1,1)
bullet.sprite.x = 400
sprite = CreateSprite("bullet")
sprite.x = 240

function OnHit(bullet)
end

function Update()
    if Input.GetKey("B") == 1 then bullet.sprite.Dust(true, true) end
    if Input.GetKey("S") == 1 then sprite.Dust(true, true) end
end