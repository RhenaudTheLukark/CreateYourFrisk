timer = 0
timer2 = 0

Arena.Resize(155, 130)

function Update()
	timer = timer + Time.dt
	while timer >= 1/60 do
		timer = timer -1/60
		timer2 = timer2 + 1
		if timer2 >= 60 * 1 and Encounter['shattering'] ~= true then
			EndWave()
		end
	end
end