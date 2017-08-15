function EventPage1()
    Event.MoveToPoint("Player", 320, 400, false)
    hp = Player.GetHP()
    General.SetDialog({"Erm[waitall:3]...[waitall:1][w:10] Why did you go here with your " ..hp .." HP?"}, true, {"papyrus_mugshot_2"})
end

function EventPage2()
    General.SetDialog({"Here's an example of event that you can do!", "Please check the event once it is finished."}, true, {"papyrus_mugshot","papyrus_mugshot"})
	Event.Teleport("Player", 113, 287)
	General.SetDialog({"This is a good idea!", "Yeah, it is.", "Error! Or maybe not?"}, true, {"papyrus_mugshot", "rtl_happy", "papyrus_mugshot_2"})
	General.SetChoice({"You can\ndo it", "Oh no you\ndon't"})
	if lastChoice == 0 then
	    General.SetDialog({"Yes you can!"}, true, {"rtlukark_determined"})
	elseif lastChoice == 1 then
		General.SetDialog({"Too bad that you can't!", "Here, I'll open a shop to help you!"}, true, {"rtlukark_pity", "rtlukark_determined"})
        General.EnterShop("Dummy")
	end
    
    for i = 1, 3 do
        General.SetChoice({"Heck yeah!", "Heck no!"}, "Are you DETERMINED ?")
        if lastChoice == 1 then
            General.SetDialog({"I knew it!"}, true, {"rtlukark_=3"})
            break
        elseif lastChoice == 0 then
            if i == 1 then
                General.SetDialog({"Are you sure?"}, true, {"rtlukark_perv"})
            elseif i == 2 then
                General.SetDialog({"Are you [w:10][letters:6]REALLY[w:10] sure?"}, true, {"rtlukark_perv"})
            elseif i == 3 then
                General.SetBattle("#04 - Animation", true, true)
            end
        end
    end
end

function EventPage3()
	if (GetRealGlobal("GoAway") == null) then	
		SetRealGlobal("GoAway", 0)
	else									
		SetRealGlobal("GoAway", GetRealGlobal("GoAway") + 1)
	end
	if (GetRealGlobal("GoAway") == 0) then		General.SetDialog({"Nothing to see here![w:10] Just go away."}, true, {"rtlukark_determined"})
	elseif (GetRealGlobal("GoAway") == 1) then	General.SetDialog({"Erm[waitall:3]...[waitall:1] [w:10]I told you to leave? [w:10]So please go."}, true, {"rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") == 2) then	General.SetDialog({"Please[waitall:3]...[waitall:1] [w:10]Just go[waitall:3]..."}, true, {"rtlukark_sorry"})
	elseif (GetRealGlobal("GoAway") == 3) then	General.SetDialog({"..."}, true, {"rtlukark_pity"})
	elseif (GetRealGlobal("GoAway") == 4) then
		General.SetDialog({"So,[w:5] yeah.[w:10] There's one more thing I have to show you.", 
						   "You know,[w:5] I was with some friends a few days ago,[w:5] and now[waitall:3]...[waitall:1][w:10] they all disappeared.",
						   "The only memory I kept from them is this picture of us,[w:5] a few instants after exitting Mt Ebott.",
						   "If I can show it to you?[w:10] [mugshot:rtlukark_pity]Yeah,[w:5] if you really want to[waitall:3]..."}, true, 
						  {"rtlukark_pity", "rtlukark_pity", "rtlukark_pity", "rtlukark_surprised"})
		Screen.SetTone(true, true, 0, 0, 0, 128)
		Screen.DispImg("photo", 1, 320, 240, 224, 130, 40, 255)
		General.WaitForInput()
		Event.Rotate("Image1", 0, -90, 0)
		Screen.DispImg("photoback", 1, 320, 240, 255, 255, 255, 255)
		Event.Rotate("Image1", 0, 90, 0, false)
		Event.Rotate("Image1", 0, 0, 0)
		General.WaitForInput()
		Screen.SupprImg(1)
		Screen.SetTone(true, true, 0, 0, 0, 0)
	elseif (GetRealGlobal("GoAway") < 8)   then	General.SetDialog({"There's no more to see here,[w:5] kiddo. You can go."}, true, {"rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") < 13)  then	General.SetDialog({"You're so patient,[w:5] I love this !"}, true, {"rtlukark_=3"})
	elseif (GetRealGlobal("GoAway") == 13) then	General.SetDialog({"You're boring me right\nnow.[w:10] If you don't stop this,[w:5] I'll be forced to use my special attack."}, true, 
													              {"rtlukark_normal","rtlukark_normal"})
	elseif (GetRealGlobal("GoAway") == 14) then	General.SetDialog({"JUST. [w:10]STOP. [w:10]This is your final warning."}, true, {"rtlukark_angry"})
	elseif (GetRealGlobal("GoAway") == 15) then	
		General.SetDialog({"Ok,[w:5] here you go![w:10][next]"}, true, {"rtlukark_angry"})
		General.GameOver({ "I told you!", "You should have stopped that." }, "mus_zz_megalovania")		
	end
end

--This event page is a big mash-up test page.
function EventPage4()
	Misc.ShakeScreen(3, 3, true)
    General.Wait(180)
	Screen.Flash(60, 255, 0, 0, 255)
	--These following lines were used for Quaternion tests.
	--You can activate them if you want to
    --/!\ Not usable anymore /!\
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
	--General.GameOver({ "[voice:v_sans]Wazzup bro?", "[voice:v_sans]I love this music, don't you?" }, "mus_zz_megalovania")
end

--[[function EventPage666()
	Event.SetAnimSwitch("Player", "Chara")
	if(GetGlobal("Chara") == false or GetGlobal("Chara") == nil) then
		SetGlobal("Chara", true)
		General.PlayBGM("mus_zzz_c", 1)
	else
		SetGlobal("Chara", false)
		General.PlayBGM("mus_anothermedium", 1)
	end
end]]