bullets = {} --Small bullet array
bigbullets = {} --Big bullet array

bulletcount = 6 --How many bullets spawn around the player.
bulletspeed = 2 --How fast said bullets move inwards.

bigbulletspeed = 1.8 --[o] Speed of formed bullets. Optional, if you don't want them to spawn anything and just want them to plop and disappear. Remove anything commented with an [o]

bulletdistance = 80 --How far the bullets are from you, in pixels.

extradistance = 0 --Extra distance, in pixels, the bullets should travel from the center point before becoming kill. Could be a negative for less duration.

bulletspawn = 70 --The time it takes before spawning the bullets.
spawntimer = 0 --Spawntimer

Arena.resize(360, 180) --Make the arena big because a small one with this attack is stupid and if you even consider it please reconsider.

function Update() --If-you-don't-know-what-this-does-then-please-look-at-the-documentation-for-christ-sake
	spawntimer = spawntimer + 1 --Adds 1 to the spawntimer per frame.
	if spawntimer%bulletspawn == 0 then --If the remainder of spawntimer divided by bulletspawn is 0, continue.
		for i = 0, bulletcount do --All credit to EmeliaK for the foundations of this code.
			local angle = ((2*math.pi)/bulletcount) * i --Some radian stuff or w/e I dunno ask EmeliaK.
			local velx = bulletspeed*math.cos(angle) --Set the x velocity of the small bullet based on the angle and how many bullets we're spawning.
			local vely = bulletspeed*math.sin(angle) --Same but for the y velocity.
			local posX = Player.x + (velx * bulletdistance) --Get the position of the player, add bulletdistance * velx to it, and set it as the x position.
			local posY = Player.y + (vely * bulletdistance) --Same but for the y axis.
			local bullet = CreateProjectile('bullet2',posX, posY) --Create projectile using those previous variables.
			bullet.SetVar('id', i) --Set the id, for spawning stuff.
			bullet.SetVar('velx', -velx) --Set velocity to inverse of velocity from earlier, so it goes in.
			bullet.SetVar('vely', -vely) --^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			bullet.SetVar('lifetime', 0) --Set the lifetime of the bullet, for timing or w/e.
			table.insert(bullets,bullet) --Insert the bullet into the table.
		end
	end
	for i=1, #bullets do --For every bullet in the table bullets do
		local bullet = bullets[i] --Get the bullet stored in the table at [i]
        local velx = bullet.GetVar('velx') --X velocity for movement use.
        local vely = bullet.GetVar('vely') --Same but for y.
        local newposx = bullet.x + velx --New position is the bullet's x position added to the x velocity.
        local newposy = bullet.y + vely --SAME FOR FOR Y
		local lifetime = bullet.GetVar('lifetime')+bulletspeed --Some lifetime stuff
		if lifetime > (bulletdistance+extradistance)*bulletspeed then --If the lifetime is equal to the bulletdistance added to the extra distance, times bulletspeed.
			if bullet.GetVar('id') == 0 then --If the id we set earlier is 0 (only the first spawned)
				bullet.SetVar('id',1) --[o] Set the id to 1 to prevent it double spawning or pulling a Tohou by accident
				local posx = bullet.x --[o] Spawn the new bullet at the original bullet's x
				local posy = bullet.y --[o] SAME BUT FOR THE Y
				local bigbullet = CreateProjectile('bullet',posx, posy) --[o]Create bullet where old bullet was (Bigger tho, originals are 8x8 this one's 16x16)
				bigbullet.SetVar('velx', 0) --[o] Set the x velocity of the new bullet to 0 so it doesn't move.
				bigbullet.SetVar('vely', bigbulletspeed) --[o] Set the y velocity of the new bullet to bigbulletspeed so it flies up / down
				table.insert(bigbullets,bigbullet) --[o] Add the new bullet to the table
				newposx = 650 --Kill old bullet
			else
				newposx = 650 --^^^^^^^^^^^^^^^
			end
		end
		bullet.SetVar('lifetime', lifetime) --Set lifetime of bullet to updated lifetime.
        bullet.MoveTo(newposx, newposy) --Move bullet.
	end
	
	for i=1, #bigbullets do --[o] If you don't understand this code below please stop.
		local bullet = bigbullets[i] --[o] Get the bullet from the table bigbullets located at [i]
        local velx = bullet.GetVar('velx') --[o] Get x velocity
        local vely = bullet.GetVar('vely') --[o] Get y velocity
        local newposx = bullet.x + velx --[o] Move it on the x
        local newposy = bullet.y + vely --[o] Move it on the y
        bullet.MoveTo(newposx, newposy) --[o] Move it to new position
	end
end