Arena.resize(155,96)
spawntimer = 0
Moneys = {} -- wait is that even a word
Bills = {}
dir = 2*math.random(2) - 3 -- generates either 1 or -1

function Update()
	spawntimer = spawntimer + 1
    if spawntimer%30 == 0 then
		local lr = math.random(2)-3/2 -- generates random number, either 0.5 ot -0.5
        local posX = (Arena.width-32) * lr -- x value is either 16 px right from the left side or 16 px left from the right side
        local posY = Arena.height/2 + 96 -- always spawns 96 pixels over the top border
        local money = CreateProjectile("money2",posX,posY)
        money.SetVar("velX",-4 * lr) -- x-speed for the bullet is either -2 (if spawned on right side) or 2 (if spawned on left side)
        money.SetVar("velY",0) -- no y-speed yet
		money.SetVar("CollGround",math.random(2)-1) -- so the height it bounces back at is variable
		money.SetVar("grounded",0) -- isnt grounded
        table.insert(Moneys,money)
    end
    for i=1,#Moneys do
        local money = Moneys[i] -- gets each bullet
        local velX = money.GetVar("velX")
        local velY = money.GetVar("velY")
		local CollGround = money.GetVar("CollGround")
		local newposX = money.x + velX -- sets the new position
		local newposY = money.y + velY
        if money.x < -Arena.width/2 + 8 then -- if it is too far to the left
			velX = -velX -- it reverses
			newposX = -Arena.width/2 + 8 -- it gets placed back inside the arena
		elseif 	money.x > Arena.width/2 - 8 then
			velX = -velX
			newposX = Arena.width/2 - 8
		end
		if money.y < -Arena.height/2 + 8 then -- if it goes below the arena
			newposY = -Arena.height/2 + 8 -- it gets placed back up
			if CollGround < 2 then -- if it has collided less than 2 times w/ the ground
				velY = 4 - 2*CollGround -- that way it bounces less every time it collides with the ground
			else
				velY = 0 -- if its collided with it more than thrice it gets a y-speed of 0
				money.SetVar("grounded",1) -- it is now permanently on the ground
			end
			money.SetVar("CollGround",CollGround + 1) -- it collided with the ground once more than before (hey that rhymed)
		end
        if money.GetVar("grounded") == 0 then
			velY = velY - 0.04 -- if it's not already on the ground, gravity is applied
		end
        money.MoveTo(newposX,newposY) -- it gets moved
        money.SetVar("velX",velX)
		money.SetVar("velY",velY)
    end
    if spawntimer%105 == 0 then
		local posX = (-Arena.width/2 - 16)*dir -- either the left or the right side of the arena
		local posY = -Arena.height/2 + 8 -- just above the bottom
		local bill = CreateProjectile("money",posX,posY) -- creates a bill there
		table.insert(Bills,bill)
	end
	for i=1,#Bills do
		local bill = Bills[i]
		if dir * bill.x > Arena.width/2 + 16 then
			bill.MoveTo(1000,1000) -- make it disappear (aka move it out of the screen)
		else
			bill.Move(2*dir,0) -- move it
		end
	end
end