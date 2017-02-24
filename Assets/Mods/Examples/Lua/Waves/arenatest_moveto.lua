function Update()
	Arena.MoveTo(320 + 120 * math.cos(Time.time * 5.0), 240 - Arena.realy/2 + 120 * math.sin(Time.time * 5.0), true)
end