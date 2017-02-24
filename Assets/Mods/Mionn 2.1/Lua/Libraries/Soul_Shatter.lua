shatter_fade_max = 6 * 60
shatter_fade = shatter_fade_max
shatter_fade_end_max = 60 * 1.5
shatter_fade_end = shatter_fade_end_max
shatter_soul_x = 320
shatter_soul_y = 280
shatter_soul_shake = 2

shatter_shake_time_max = 6.5 * 60
shatter_shake_time = shatter_shake_time_max
shatter_shake = 1.8

shatter_broken_wait_max = 90
shatter_broken_wait = shatter_broken_wait_max

shatter_g = 0.04
shatter_vel = 3.0
shatter_shards = {}

shatter_shard_state = 0
shatter_shard_number = 6
shatter_shard_wait_max = 8
shatter_shard_wait = shatter_shard_wait_max

function SetupShatter()
	shatter_flash = CreateProjectileAbs("hide_UI",320,230)
	shatter_flash.sprite.color = {0,0,0}
	shatter_flash_back = CreateSprite("flash")
	shatter_flash_back.x = 320
	shatter_flash_back.y = 240
	shatter_flash_back.color = {0,0,0}
	shatter_soul = CreateProjectileAbs("Final/mionn_soul",320,280)
	shatter_soul.sprite.alpha = 0
	player_soul = CreateProjectileAbs("ut-heart",Player.absx,Player.absy)
	player_soul.sprite.color = {1,0,0}
	shatter_flash.SetVar("safe",true)
	shatter_soul.SetVar("safe",true)
	player_soul.SetVar("safe",true)
	shattering = true
	shatter_fading = true
	shatter_shaking = true
end

-- Shatter colours: 255,255,255 - white ; 38,255,0 - green

function ShatterFade()
	if shatter_fading == true then
		shatter_fade = shatter_fade - 1
		shatter_soul.sprite.alpha = 1 - shatter_fade/shatter_fade_max
		if shatter_fade <= 0 then shatter_fading = false end
	end
end

function ShatterShake()
	if shatter_shaking == true then
		if timer2%3 == 0 then
			shatter_soul.MoveToAbs(shatter_soul_x + math.random(-shatter_shake,shatter_shake), shatter_soul_y + math.random(-shatter_shake,shatter_shake))
			shatter_shake_time = shatter_shake_time - 3
			if shatter_shake_time <= 0 then
				shatter_shaking = false
				shatter_soul.sprite.Set("Final/mionn_soul_break")
				Audio.PlaySound("heartbeatbreaker")
				shatter_broke = true
			end
		end
	end
end

function ShatterBreak()
	if shatter_broke == true then
		shatter_broken_wait = shatter_broken_wait - 1
		if shatter_broken_wait <= 0 then
			shatter_broke = false
			Audio.PlaySound("heartsplosion")
			shatter_soul.sprite.alpha = 0
			for i = 1,6 do
				local shard = CreateProjectileAbs("Final/heartshard_0",shatter_soul_x,shatter_soul_y)
				local angle = math.random(360)
				local velx = math.cos(math.rad(angle)) * shatter_vel
				local vely = math.sin(math.rad(angle)) * shatter_vel
				shard.SetVar('velx',velx)
				shard.SetVar('vely',vely)
				shard.SetVar('safe',true)
				if i <= 3 then
					shard.sprite.color = {38/255,255/255,0/255}
				end
				table.insert(shatter_shards,shard)
			end
		end
	end
end

function ShatterShardMove()
	for i = 1, # shatter_shards do
		local shard = shatter_shards[i]
		if shard.isactive then
			local velx = shard.GetVar('velx')
			local vely = shard.GetVar('vely')
			vely = vely - shatter_g
			shard.SetVar('vely',vely)
			shard.Move(velx,vely)
			if shard.absy < -500 then
				shard.Remove()
				shatter_shard_number = shatter_shard_number - 1
			end
		end
	end
	if shatter_shard_number <= 0 then shatter_fadout = true end
end

function ShatterShardAnimate()
	shatter_shard_wait = shatter_shard_wait - 1
	if shatter_shard_wait <= 0 then
		shatter_shard_wait = shatter_shard_wait_max
		if shatter_shard_state < 3 then
			shatter_shard_state = shatter_shard_state + 1
		else
			shatter_shard_state = 0
		end
		
		for i = 1, # shatter_shards do
			local shard = shatter_shards[i]
			shard.sprite.Set("Final/heartshard_"..shatter_shard_state)
		end
	end
end

function ShatterFadout()
	if shatter_fadout == true then
		shatter_fade_end = shatter_fade_end - 1
		player_soul.sprite.alpha = shatter_fade_end / shatter_fade_end_max
		if shatter_fade_end <= 0 then State("DONE") end
	end
end

function RunShattering()
	ShatterFade()
	ShatterShake()
	ShatterBreak()
	ShatterShardMove()
	ShatterShardAnimate()
	ShatterFadout()
end