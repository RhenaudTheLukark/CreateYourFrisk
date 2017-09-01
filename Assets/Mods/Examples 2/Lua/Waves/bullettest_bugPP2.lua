-- testwave.lua --

lbeam = CreateProjectile("beam_warning", 0, 0)
lbeam.sprite.Scale(2000, 1)
lbeam.ppcollision = true
lbeam.sprite.rotation = 45
lbeam["warntimer"] = 90
lbeam["staytimer"] = math.huge
lbeam["nodamage"] = true
warncolor1 = {1, 299 / 255, 50 / 255}	-- warning colours for
warncolor2 = {1, 0.25, 0.25}			-- the laser beams
function Update()
	if lbeam.isactive then
			if lbeam["nodamage"] then
				lbeam["warntimer"] = lbeam["warntimer"] - 1
				if lbeam["warntimer"]%4 > 1 then
					lbeam.sprite.color = warncolor2
				else
					lbeam.sprite.color = warncolor1
				end
				if lbeam["warntimer"] <= 0 then
					lbeam["nodamage"] = false
					lbeam.sprite.Set("beam_texture")
					--Audio.PlaySound("titanbeam")
					lbeam.sprite.color = {1, 1, 1}
				end
			else
				if lbeam["staytimer"] > 0 then
					lbeam["staytimer"] = lbeam["staytimer"] - 1
					lbeam.sprite.yscale = 0.9 + (0.1 * math.sin(80 * Time.time))
				else
					lbeam.sprite.yscale = lbeam.sprite.yscale - 0.07
					if lbeam.sprite.yscale <= 0 then
						lbeam.Remove()
					end
				end
			end
		end
end

function OnHit(bullet)
	if not lbeam["nodamage"] then Player.Hurt(0.001) end
end