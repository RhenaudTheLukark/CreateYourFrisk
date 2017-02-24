-- Updates in 0.2s : +-12

-- Arms pos x : 43, 11
SetGlobal("wavetimer", 10.0)
-- Coordonates of the bullets
-- 1, 3, 5, 7, 9 = x values | 2, 4, 6, 8, 10 y values
movetable = {43,152, 35,150, 27,150, 19,152, 11,156}
-- The wave's launching time
balisebegin = Time.time
-- "Lukark" is a variable that animates the enemy
SetGlobal("Lukark", "waveballbegin")
-- Last positions of arms
lastposleft = 1
lastposright = 1
-- Did we just changed our position ?
changesideright = false
changesideleft = false
-- We've already chosen our way, kiddo
chosen = false
bullets = {}
-- It'll help for getting the original "y" of bullets
last_temp_anims = {}
-- Two switches that'll help me for bullet's moving
tempanimchangestop1 = false
tempanimchangestop2 = false	
-- Variables that'll contain the coordonates of the bullets
tempx1 = 0
tempy1 = 0
tempx2 = 0
tempy2 = 0
-- We'll use this to don't check the bullet's speed lots of times
didit = 0
-- Used to make sure that the direction is chosen once per anim
updates = 0
-- I'll use this for some purpose
arraylength = 0

function Update()
	-- Tip : My anim repeats itself every 0.5s
	
	-- Get the time of the wave
	timesincebegin = Time.time - balisebegin
	-- It'll help later, for bullets anim
	indice = math.floor((timesincebegin / 0.5) - 0.6)* 2 + 1
	-- If we're near the end of the wave 
	if GetGlobal("wavetimer") - timesincebegin < 0.3 and chosen == false then
		SetGlobal("Lukark", "waveballend")
		chosen = true
	-- If we're at 3/5 of the timer and that we didn't went here before
	elseif (timesincebegin % 0.5) > 0.3 and chosen == false then
		local posx1 = 0
		local posy1 = 0
		if lastposleft == 1 then 
			posx1 = -43
			posy1 = 152
		else
			posx1 = -11
			posy1 = 156
		end
		-- Left bullet
        local bullet1 = CreateProjectile('largeball', posx1, posy1)
        table.insert(bullets, bullet1)
		arraylength = arraylength + 1
		table.insert(last_temp_anims, 0)
		local posx2 = 0
		local posy2 = 0
		if lastposright == 1 then 
			posx2 = 43
			posy2 = 152
		else
			posx2 = 11
			posy2 = 156
		end
		-- Right bullet
        local bullet2 = CreateProjectile('largeball', posx2, posy2)
        table.insert(bullets, bullet2)
		arraylength = arraylength + 1
		table.insert(last_temp_anims, 0)
		-- It chooses how the enemy (and the bullets) will move !
		temporarychoose = math.random(0, 4)
		if temporarychoose >= 3 then
			changesideright = true
			changesideleft = true
		elseif temporarychoose >= 2 then
			changesideleft = true
		elseif temporarychoose >= 1 then
			changesideright = true
		end
		-- Are we near the end of the wave ? Put the arms on the pos 1 if yes
		if GetGlobal("wavetimer") - timesincebegin < 1 then
			if lastposleft == 2 then
				changesideleft = true
			end
			if lastposright == 2 then
				changesideright = true
			end
		end
		-- Here we are ! Here are all my arms' movement ! 
		-- If we didn't changed our pos
		if changesideleft == false and changesideright == false then
			SetGlobal("Lukark", "ball" .. lastposleft .. "-" .. lastposright)
		-- If we changed our pos on the right
		elseif changesideleft == false and changesideright == true then
			if lastposright == 1 then
				SetGlobal("Lukark", "waveball" .. lastposleft .."-1to2")
				lastposright = 2
			else
				SetGlobal("Lukark", "waveball" .. lastposleft .."-2to1")
				lastposright = 1
			end
		-- If we changed our pos on the left
		elseif changesideleft == true and changesideright == false then
			if lastposleft == 1 then
				SetGlobal("Lukark", "waveball1to2-" .. lastposright)
				lastposleft = 2
			else
				SetGlobal("Lukark", "waveball2to1-" .. lastposright)
				lastposleft = 1
			end
		-- If we changed our pos on both sides
		else
			if lastposleft == 1 and lastposright == 1 then
				SetGlobal("Lukark", "waveball1to2-1to2")
				lastposleft = 2
				lastposright = 2
			elseif lastposleft == 1 and lastposright == 2 then
				SetGlobal("Lukark", "waveball1to2-2to1")
				lastposleft = 2
				lastposright = 1
			elseif lastposleft == 2 and lastposright == 1 then
				SetGlobal("Lukark", "waveball2to1-1to2")
				lastposleft = 1
				lastposright = 2
			else
				SetGlobal("Lukark", "waveball2to1-2to1")
				lastposleft = 1
				lastposright = 1
			end
		end
		-- We've already chosen our way !
		chosen = true
	-- Time to go for a new direction ! ^^
	elseif (timesincebegin % 0.5) <= 0.2 and updates < indice / 2 then
		chosen = false
		updates = updates + 1
	end
	-- Here are my bullet's moves
	-- If we're at 9/10 of the animation and we've not moved the bullet yet
	if GetGlobal("wavetimer") - timesincebegin < 0.3 then
		--Here goes nothing
	elseif (timesincebegin % 0.5) > 0.45 and tempanimchangestop1 == false then --and (changesideleft == true or changesideright == true) then
		if lastposright == 1 and changesideright == true then
			tempx1 = movetable[3]
			tempy1 = movetable[4]
		elseif lastposright == 2 and changesideright == true then
			tempx1 = movetable[7]
			tempy1 = movetable[8]
		elseif lastposright == 1 and changesideright == false then
			tempx1 = movetable[1]
			tempy1 = movetable[2]
		else
			tempx1 = movetable[9]
			tempy1 = movetable[10]
		end
		if lastposleft == 1 and changesideleft == true then
			tempx2 = -movetable[3]
			tempy2 = movetable[4]
		elseif lastposleft == 2 and changesideleft == true then
			tempx2 = -movetable[7]
			tempy2 = movetable[8]
		elseif lastposleft == 1 and changesideleft == false then
			tempx2 = -movetable[1]
			tempy2 = movetable[2]
		else
			tempx2 = -movetable[9]
			tempy2 = movetable[10]
		end
		tempanimchangestop1 = true
		tempanimchangestop2 = false
	-- If we're at 8/10 of the animation and we've not moved the bullet yet
	elseif (timesincebegin % 0.5) > 0.4 and tempanimchangestop2 == false then --and (changesideleft == true or changesideright == true) then
		if changesideright == true then
			tempx1 = movetable[5]
			tempy1 = movetable[6]
		elseif lastposright == 1 then
			tempx1 = movetable[1]
			tempy1 = movetable[2]
		else
			tempx1 = movetable[9]
			tempy1 = movetable[10]
		end
		if changesideleft == true then
			tempx2 = -movetable[5]
			tempy2 = movetable[6]
		elseif lastposleft == 1 then
			tempx2 = -movetable[1]
			tempy2 = movetable[2]
		else
			tempx2 = -movetable[9]
			tempy2 = movetable[10]
		end
		tempanimchangestop1 = false
		tempanimchangestop2 = true
	-- If we're at 7/10 of the animation and we've not moved the bullet yet
	elseif (timesincebegin % 0.5) > 0.35 and tempanimchangestop1 == false then --and (changesideleft == true or changesideright == true) then
		if lastposright == 1 and changesideright == true then
			tempx1 = movetable[7]
			tempy1 = movetable[8]
		elseif lastposright == 2 and changesideright == true then
			tempx1 = movetable[3]
			tempy1 = movetable[4]
		elseif lastposright == 1 and changesideright == false then
			tempx1 = movetable[1]
			tempy1 = movetable[2]
		else
			tempx1 = movetable[9]
			tempy1 = movetable[10]
		end
		if lastposleft == 1 and changesideleft == true then
			tempx2 = -movetable[7]
			tempy2 = movetable[8]
		elseif lastposleft == 2 and changesideleft == true then
			tempx2 = -movetable[3]
			tempy2 = movetable[4]
		elseif lastposleft == 1 and changesideleft == false then
			tempx2 = -movetable[1]
			tempy2 = movetable[2]
		else
			tempx2 = -movetable[9]
			tempy2 = movetable[10]
		end
		tempanimchangestop1 = true
		tempanimchangestop2 = false
	-- If we're at the beginning of the animation and we've not moved the 
	-- bullet yet
	elseif (timesincebegin % 0.5) < 0.2 and tempanimchangestop2 == false and timesincebegin > 0.5 then --and (changesideleft == true or changesideright == true)
		if lastposright == 1 then
			tempx1 = movetable[1]
			tempy1 = movetable[2]
		elseif lastposright == 2 then
			tempx1 = movetable[9]
			tempy1 = movetable[10]
		end
		if lastposleft == 1 then
			tempx2 = -movetable[1]
			tempy2 = movetable[2]
		elseif lastposleft == 2 then
			tempx2 = -movetable[9]
			tempy2 = movetable[10]
		end
		tempanimchangestop1 = false
		tempanimchangestop2 = true
	end
	-- Here are the bullet's move, in accordance to the enemy's moves
	-- temp_anim is the position variable that I used to move the enemy
	temp_anim = Time.time - GetGlobal("animPhaseCut")
	-- Our shift, for y
	animshift = 5.15*math.sin(temp_anim*2)
	i = arraylength-7
	if i < 1 then
		i = 1
	end
	while i<arraylength+1 do
		-- If there's no bullets, just GET OUT OF HERE
		if #bullets == 0 then
			break
		end
		local bullet = bullets[i]
		if (GetGlobal("wavetimer") - timesincebegin) < 0.3 then
		else
			if i >= indice then 
				if tempx1 != 0 then
					bullet.MoveTo(tempx1, tempy1 + animshift)
					tempx1 = 0
					tempy1 = 0
				elseif tempx2 != 0 then
					bullet.MoveTo(tempx2, tempy2 + animshift)
					tempx2 = 0
					tempy2 = 0
				else
					-- The movement itself, very complicated. Can't explain why
					bullet.MoveTo(bullet.x, bullet.y - 5.15*math.sin(last_temp_anims[i]*2) + animshift)
				end
			end
		end
		if i >= indice - 2 and didit < 4 then
			didit = didit + 1
			bullet.SetVar('xspeed', 0)
			bullet.SetVar('yspeed', -4)
		end
		if i < indice then
			local xspeed = bullet.GetVar('xspeed')
			local yspeed = bullet.GetVar('yspeed')
			bullet.Move(xspeed, yspeed)
			bullet.sprite.rotation = bullet.sprite.rotation + 10
		end
		-- Don't forget to store the new last shift !
		last_temp_anims[i] = temp_anim
		i = i + 1
	end
	-- We don't know if we've changed of side, didn't we ?
	if (timesincebegin % 0.5) < 0.3 and (timesincebegin % 0.5) >= 0.2 then
		changesideright = false
		changesideleft = false
		tempanimchangestop = false
		tempanimchangestop2 = false
		didit = 0
	end
end

function OnHit(bullet)
    Player.Hurt(9)
end