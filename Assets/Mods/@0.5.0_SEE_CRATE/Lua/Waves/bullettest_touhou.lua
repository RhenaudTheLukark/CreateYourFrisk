local bg = CreateSprite("UI/sq_white", "BelowUI")
bg.Scale(640/4,480/4)
bg.x = 320
bg.y = 240
bg.color = {0,0,0}
if enemies[1]["name"] ~= "Punderbolt" then
	local buttoncover = CreateSprite("UI/sq_white", "BelowArena")
	buttoncover.Scale(640/4,50/4)
	buttoncover.MoveTo(320,25)
	buttoncover.color = {0,0,0}
	local namecover = CreateSprite("UI/sq_white", "BelowArena")
	namecover.SetPivot(0,0.5)
	namecover.Scale((#Player.name*13)/4 + ((#Player.name*2)/4),16/4)
	namecover.MoveTo(30,71)
	namecover.color = {0,0,0}
	nextwaves = {"bullettest_chaserorb"}
	State("DEFENDING")
	update = Update
	function Update()
		local x = (Input.Right > 0 and Input.Right or 0) - (Input.Left > 0 and Input.Left or 0)
		local y = (Input.Up > 0 and Input.Up or 0) - (Input.Down > 0 and Input.Down or 0)
		local speed = Input.Cancel < 1 and 2 or 1
		Player.Move(x*speed, y*speed, false)
		update()
	end
else
	cover = CreateSprite("UI/sq_white", "Top")
	cover.Scale(640/4, 460/4)
	cover.MoveTo(320,0)
	cover.color = {0,0,0}
end
fade.SendToTop()