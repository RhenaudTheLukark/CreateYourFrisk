function EventPage1()
	SetAnimSwitch("Player", "MovingUp")
	MoveEventToPoint("Player", 320, 400, true)
	SetDialog({"Erm[waitall:3]...[waitall:1][w:10] Why did you go here?"}, true, {"papyrus_mugshot_2"})
end

function EventPage2()
    SetDialog({"Here's an example of event that you can do!", "Please check the event once it is finished."}, true, {"papyrus_mugshot","papyrus_mugshot"})
	TeleportEvent("Player", 113, 287)
	SetDialog({"This is a good idea!", "Yeah, it is.", "Error! Or maybe not?"}, true, {"papyrus_mugshot", "rtl_happy", "papyrus_mugshot_2"})
	SetChoice("You can\ndo it", "Oh no you\ndon't", 1)
	if GetRealGlobal("Choice1") == 0 then
	    SetDialog({"Yes you can!"}, true, {"rtlukark_determined"})
	elseif GetRealGlobal("Choice1") == 1 then
		SetDialog({"Too bad that you can't!"}, true, {"rtlukark_pity"})
	end
	if GetRealGlobal("yes") == null then
	    SetRealGlobal("yes", 0)
	end
	SetReturnPoint(1)
    SetChoice("Heck yeah!", "Heck no!", "Are you DETERMINED ?", 2)
	if GetRealGlobal("Choice2") == 1 then
	    SetDialog({"I knew it!"}, true, {"rtlukark_=3"})
	elseif GetRealGlobal("Choice2") == 0 then
		SetRealGlobal("yes", GetRealGlobal("yes") + 1)
		if GetRealGlobal("yes") == 1 then
			SetDialog({"Are you sure?"}, true, {"rtlukark_perv"})
			GetReturnPoint(1)
		elseif GetRealGlobal("yes") == 2 then
			SetDialog({"Are you [w:10][letters:6]REALLY[w:10] sure?"}, true, {"rtlukark_perv"})
			GetReturnPoint(1)
		elseif GetRealGlobal("yes") == 3 then
			SetDialog({"Oh ok,[w:5] I trust you!","If you're [w:10][letters:2]SO[w:10] determined right now,[w:5] I bet you can kill my friend!", 
									"Wanna try?"}, true, {"rtlukark_seriously", "rtlukark_determined","rtlukark_=3"})
			SetBattle("#04 - Animation", true, true)
		end
	end
end

function EventPage3()
	if (GetRealGlobal("GoAway") == null) then	
		SetRealGlobal("GoAway", 0)
	else									
		SetRealGlobal("GoAway", GetRealGlobal("GoAway") + 1)
	end
	if (GetRealGlobal("GoAway") == 0) then		SetDialog({"Nothing to see here![w:10] Just go away."}, true, {"rtlukark_determined"})
	elseif (GetRealGlobal("GoAway") == 1) then	SetDialog({"Erm[waitall:3]...[waitall:1] [w:10]I told you to leave? [w:10]So please go."}, true, {"rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") == 2) then	SetDialog({"Please[waitall:3]...[waitall:1] [w:10]Just go[waitall:3]..."}, true, {"rtlukark_sorry"})
	elseif (GetRealGlobal("GoAway") == 3) then	SetDialog({"..."}, true, {"rtlukark_pity"})
	elseif (GetRealGlobal("GoAway") == 4) then
		SetDialog({"So,[w:5] yeah.[w:10] There's one more thing I have to show you.", 
								"You know,[w:5] I was with some friends a few days ago,[w:5] and now[waitall:3]...[waitall:1][w:10] they all disappeared.",
								"The only memory I kept from them is this picture of us,[w:5] a few instants after exitting Mt Ebott.",
								"If I can show it to you?[w:10] [mugshot:rtlukark_pity]Yeah,[w:5] if you really want to[waitall:3]..."}, true, 
								{"rtlukark_pity", "rtlukark_pity", "rtlukark_pity", "rtlukark_surprised"})
		SetTone(true, true, 0, 0, 0, 128)
		DispImg("photo", 1, 320, 240, 436, 256, 224, 130, 40, 255)
		WaitForInput()
		RotateEvent("Image1", 0, -90, 0)
		DispImg("photoback", 1, 320, 240, 436, 256, 255, 255, 255, 255)
		RotateEvent("Image1", 0, 90, 0, false)
		RotateEvent("Image1", 0, 0, 0)
		WaitForInput()
		SupprImg(1)
		SetTone(true, true, 0, 0, 0, 0)
	elseif (GetRealGlobal("GoAway") < 8) then	SetDialog({"There's no more to see here,[w:5] kiddo. You can go."}, true, {"rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") < 13) then	SetDialog({"You're so patient,[w:5] I love this !"}, true, {"rtlukark_=3"})
	elseif (GetRealGlobal("GoAway") == 13) then	SetDialog({"You're boring me right\nnow.[w:10] If you don't stop this,[w:5] I'll be forced to use my special attack."}, true, 
													  {"rtlukark_normal","rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") == 14) then	SetDialog({"JUST. [w:10]STOP. [w:10]This is your final warning."}, true, {"rtlukark_angry"})
	elseif (GetRealGlobal("GoAway") == 15) then	
		SetDialog({"Ok,[w:5] here you go![w:10][next]"}, true, {"rtlukark_angry"})
		GameOver("I told you!\rYou should have stopped that.", "mus_zz_megalovania")		
	end
end

--This event page is a big mash-up test page.
function EventPage4()
	Rumble(3, 15, true);
	Flash(60, false, 255, 0, 0, 255);
	--These following lines were used for Quaternion tests.
	--You can activate them if you want to ^^
	--[[SetTone(true, true, 0, 0, 0, 128)
	DispImg("photo", 1, 320, 240, 436, 256, 224, 130, 40, 255)
	WaitForInput()
	RotateEvent("Image1", 360, 0, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 360, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 0, 360)
	WaitForInput()
	RotateEvent("Image1", 360, 360, 360)
	WaitForInput()
	RotateEvent("Image1", 180, 360, 0)
	WaitForInput()
	RotateEvent("Image1", 0, 180, 360)
	WaitForInput()
	RotateEvent("Image1", 360, 0, 180)
	WaitForInput()
	RotateEvent("Image1", 360, 360, 360)
	WaitForInput()
	SupprImg(1)
	SetTone(true, true, 0, 0, 0, 0)]]
	GameOver("[voice:v_sans]Wazzup bro?\r[voice:v_sans]I love this music, don't you ?", "mus_zz_megalovania")
	--GameOver("I told you !\rYou should have\nstopped that.", "mus_zz_megalovania")
end






















































































































--Please don't go further if you haven't found the "Miss" secret first.














































function EventPage666()
	SetAnimSwitch("Player", "Chara")
	if(GetGlobal("Chara") == false or GetGlobal("Chara") == null) then
		SetGlobal("Chara", true)
		PlayBGMOW("mus_zzz_c", 1)
	else
		SetGlobal("Chara", false)
		PlayBGMOW("mus_anothermedium", 1)
	end
end