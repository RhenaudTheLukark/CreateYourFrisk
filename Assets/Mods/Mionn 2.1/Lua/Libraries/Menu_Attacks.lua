menu_h_bullets = {}
menu_c_bullets = {}
in_menus = false

menu_bullet_vx = 2 - (2-difficulty) * 0.3
menu_bullet_vR = 0.5 - (2-difficulty) * 0.1
menu_bullet_R_max = 80
menu_bullet_vrot = 1 - (2-difficulty) * 0.2
menu_bullet_N_c = 18
menu_bullet_delay = 30 + (2-difficulty) * 2
menu_bullet_delay_h = 30 + (2-difficulty) * 2

menu_bullet_next_circle = 1

menu_center_x = {
48, -- FIGHT option
202, -- ACT option
361, -- ITEM option
515 -- MERCY option
}

menu_open_centers = {0,0,0,0}

function Menu_Spawn_Horizontal()
	if timer2%menu_bullet_delay_h == 0 and timer2/menu_bullet_delay_h%5 ~= 0 then
		local bullet = CreateProjectileAbs("bigfball",-5,190)
		bullet.sprite.color = {0.9,0,0}
		table.insert(menu_h_bullets,bullet)
	end
end

function Menu_Move_Horizontal()
	for i = 1,#menu_h_bullets do
		local bullet = menu_h_bullets[i]
		if bullet.isactive then
			bullet.Move(menu_bullet_vx,0)
			if bullet.y > 650 then bullet.Remove() end
		end
	end
end

function Menu_Spawn_Circle()
	if timer2%menu_bullet_delay == 0 then
		if menu_open_centers[menu_bullet_next_circle] == 0 then
			for i = 1, menu_bullet_N_c do
				local center_x = menu_center_x[menu_bullet_next_circle]
				local center_y = 25
				local angle = 360/menu_bullet_N_c * i
				local rangle = math.rad(angle)
				local x = math.cos(rangle) * menu_bullet_R_max
				local y = math.sin(rangle) * menu_bullet_R_max
				local bullet = CreateProjectileAbs("fball", center_x+x , center_y+y )
		bullet.sprite.color = {0.9,0,0}
				bullet.SetVar('R',menu_bullet_R_max)
				bullet.SetVar('ang',angle)
				bullet.SetVar('center',menu_bullet_next_circle)
				table.insert(menu_c_bullets,bullet)
			end
			menu_open_centers[menu_bullet_next_circle] = 1
			if menu_bullet_next_circle < 4 then
				menu_bullet_next_circle = menu_bullet_next_circle + 1
			else
				menu_bullet_next_circle = 1
			end
			
		end
	end
end

function Menu_Move_Circle()
	for i = 1,#menu_c_bullets do
		local bullet = menu_c_bullets[i]
		if bullet.isactive then
			local R = bullet.GetVar('R')
			local ang = bullet.GetVar('ang')
			local center = bullet.GetVar('center')
			local center_x = menu_center_x[center]
			local center_y = 25
			R = R - menu_bullet_vR
			ang = ang + menu_bullet_vrot
			local rangle = math.rad(ang)
			local x = math.cos(rangle) * R + center_x
			local y = math.sin(rangle) * R + center_y
			bullet.MoveToAbs(x,y)
			bullet.SetVar('R',R)
			bullet.SetVar('ang',ang)
			if R <= 0 then
				bullet.Remove()
				menu_open_centers[center] = 0
			end
		end
	end
end

function Run_Menu_Attacks()
	if in_menus == true and DET >= 75 then
		Menu_Spawn_Horizontal()
		Menu_Move_Horizontal()
		Menu_Spawn_Circle()
		Menu_Move_Circle()
	end
end

function Remove_Menu_Attacks()
	menu_bullet_next_circle = 1
	menu_open_centers = {0,0,0,0}
	for i = 1, #menu_h_bullets do
		local bullet = menu_h_bullets[i]
		if bullet.isactive then bullet.Remove() end
	end
	for i = 1, #menu_c_bullets do
		local bullet = menu_c_bullets[i]
		if bullet.isactive then bullet.Remove() end
	end
	menu_h_bullets = {}
	menu_c_bullets = {}
end


-- VALUES:
-- absx: D = 154 ; 256
  -- 48 - FIGHT option
  -- 202 - ACT option
  -- 361 - ITEM option
  -- 515 - MERCY option
  -- 65 - COLLUMN 1
  -- 321 - COLLUMN 2
-- absy:
  -- 25 - FIGHT/ACT/ITEM/MERCY
  -- 190 - ROW 1
  -- 160 - ROW 2