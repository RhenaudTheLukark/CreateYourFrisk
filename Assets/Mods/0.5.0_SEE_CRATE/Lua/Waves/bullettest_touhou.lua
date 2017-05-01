local bg = CreateSprite("UI/sq_white", "BelowUI")
bg.Scale(640/4,480/4)
bg.x = 320
bg.y = 240
bg.color = {0,0,0}
if enemies[1]["name"] ~= "Punderbolt" then
	local buttoncover = CreateSprite("UI/sq_white", "BelowArena")
	buttoncover.Scale(640/4,49/4)
	buttoncover.MoveTo(320,24.5)
	buttoncover.color = {0,0,0}
	local namecover = CreateSprite("UI/sq_white", "BelowArena")
	namecover.SetPivot(0,0.5)
	namecover.Scale((#Player.name*13)/4 + ((#Player.name*2)/4),15/4)
	namecover.MoveTo(30,70)
	namecover.color = {0,0,0}
	nextwaves = {"bullettest_chaserorb"}
	State("DEFENDING")
else
	cover = CreateSprite("UI/sq_white", "Top")
	cover.Scale(640/4, 459/4)
	cover.MoveTo(320,0)
	cover.color = {0,0,0}
end
fade.SendToTop()