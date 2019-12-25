Encounter["cover"].MoveTo(Encounter["cover"].x, Encounter["cover"].y-150)
bullet1 = CreateProjectileAbs("UI/Buttons/fightbt_0", 320-155, 160, "BelowPlayer")
bullet2 = CreateProjectileAbs("UI/Buttons/mercybt_0", 320+155, 160, "BelowPlayer")
Arena.MoveToAndResize(320, 90, 565, 130, false, true)
finish = false
inButton = false

function Update()
    if not bullet1.isColliding() and not bullet2.isColliding() then inButton = false end
	if bullet1.isColliding() then
	    bullet1.sprite.Set("UI/Buttons/fightbt_1")
		if not inButton then
		    inButton = true
			Audio.PlaySound("menumove")
		end
		if Input.Confirm == 1 then
			Audio.PlaySound("menuconfirm")
		    Encounter["enemies"][2]["currentdialogue"] = {"[noskip][func:Animate,happy]Goo...[func:Animate,surprised]what?![w:10][next]",
			                                              "[noskip][func:Animate,bracing][func:forceattack,1," .. 54302+math.random(32592) .. "][w:80][next]",
														  "[noskip][func:Animate,death2]I[waitall:5]...[w:20][waitall:1]I am this engine's creator and you just killed me[waitall:5]...?[w:40][next]",
														  "[noskip][func:Animate,deathangry]What kind of psycho are you?![w:40][next]",
														  "[noskip][func:Animate,deatheyesclosed][waitall:5]...[w:20][waitall:1][func:Animate,deathsmile]heh.[w:40][next]",
														  "[noskip][func:Animate,deathnormal]I have one last trick[waitall:5]...[w:40][next]",
														  "[noskip][waitall:2]Just for you[waitall:6]...[w:40][next]",
														  "[noskip][func:Animate,deathhurt][waitall:3]Before I die[waitall:7]...[w:40][next]",
														  "[noskip][func:Animate,deathcontorted][waitall:4]I won't like it,[w:15] but[waitall:8]...[w:40][next]",
														  "[noskip][func:Animate,deathdeath][waitall:5]Take th[func:AnimEnd]is![w:999]"}
            bullet1.Remove()
            bullet2.Remove()
			finish = true
	        Audio.Pause()
		    State("ENEMYDIALOGUE")
			Encounter["cover"].MoveTo(Encounter["cover"].x, Encounter["cover"].y+150)
		end
	else bullet1.sprite.Set("UI/Buttons/fightbt_0")
	end
	if bullet2.isColliding() and not finish then
	    bullet2.sprite.Set("UI/Buttons/mercybt_1")
		if not inButton then
		    inButton = true
			Audio.PlaySound("menumove")
		end
		if Input.Confirm == 1 then
			Audio.PlaySound("menuconfirm")
		    Encounter["enemies"][2]["currentdialogue"] = {"[noskip][func:Animate,happy]Good![w:40][next]",
			                                              "[noskip]Thanks for sparing me![w:40][next]",
			                                              "[noskip][func:Animate,normal][waitall:5]...[w:40][next]",
			                                              "[noskip][func:Animate,pensive]What would have happened if you had killed me?[w:40][next]",
			                                              "[noskip][func:Animate,dunno]I dunno.[w:40][func:WindowClose][w:999]"}
            bullet1.Remove()
            bullet2.Remove()
	        Audio.Pause()
		    State("ENEMYDIALOGUE")
			Encounter["cover"].MoveTo(Encounter["cover"].x, Encounter["cover"].y+150)
		end
	else bullet2.sprite.Set("UI/Buttons/mercybt_0")
	end
end

function OnHit(bullet) end