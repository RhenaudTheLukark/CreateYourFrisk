spawntimer = 1
faster = 1.01
linez = {}
function OnHit(money2)
    Player.Hurt(1, 0.3)
end
function Update()
    spawntimer = spawntimer + 1
    if(spawntimer % 50 == 0 ) then
		faster = faster + 0.01
		corner = math.random(4)
		xHolder = 0
		yHolder = 0
		if corner == 1 then
			xOffset = 80
			yOffset = 104
			yPlacer = -10
			xPlacer = 0
		elseif corner == 2 then
			xOffset = 120
			yOffset = 64
			yPlacer = 0
			xPlacer = -10
		elseif corner == 3 then
			xOffset = -120
			yOffset = -64
			yPlacer = 0
			xPlacer = 10
		else
			xOffset = -80
			yOffset = -104
			yPlacer = 10
			xPlacer = 0
		end
		numbullets = 7
		for i=1, numbullets do
			line = CreateProjectile('money2', xOffset + (yHolder + yPlacer), yOffset + (xHolder + xPlacer))
			xHolder = xHolder + xPlacer
			yHolder = yHolder + yPlacer
			line.SetVar('velx',xPlacer)
			line.SetVar('vely',yPlacer)
			table.insert(linez,line)
		end
	end
	
	for i=1,#linez do 
		local Line = linez[i]
		local velx = Line.GetVar('velx')
		local vely = Line.GetVar('vely')
		local newposx = Line.x + velx / 3.5 * faster
		local newposy = Line.y + vely / 3.5 * faster
		Line.MoveTo(newposx,newposy)
	end
end