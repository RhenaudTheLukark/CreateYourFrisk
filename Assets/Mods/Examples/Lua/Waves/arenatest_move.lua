if not isCYF then
	error("This feature only works in CYF.")
end

bullet = CreateProjectile("bullet", Arena.width/2, 0)
bullet.layer = "After"
timer = 0

function Update()
    Arena.Move(1, 0, true, true)
	bullet.sprite.Scale(bullet.sprite.xscale + 0.02, bullet.sprite.xscale + 0.02)
end