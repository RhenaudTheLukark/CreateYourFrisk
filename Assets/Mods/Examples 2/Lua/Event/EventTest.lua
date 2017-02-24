function EventPage1()
	SetAnimOW("SetAnimOW", "Player", "Up")
	MoveEventToPoint("MoveEventToPoint", "Player", 113, 280)
	SetDialog("SetDialog", {"Erm[waitall:3]...[waitall:1][w:10] Why do you\ngo here ?"}, true, {"papyrus_mugshot_2"})
end

function EventPage2()
    SetDialog("SetDialog", {"Here's an example of\nevent that you can do !", "Please check the event\nonce it is finished."}, true, {"papyrus_mugshot","papyrus_mugshot"})
	TeleportEvent("TeleportEvent", "Player", 113, 287)
	SetDialog("SetDialog", {"This is a good idea !", "Yeah, it is.", "Error ! Or maybe not ?"}, true, {"papyrus_mugshot", "rtl_happy", "papyrus_mugshot_2"})
	SetChoice("SetChoice", "You can\ndo it", "Oh no you\ndon't")
	if GetGlobal("ChoiceID1") == 0 then
	    SetDialog("SetDialog", {"Yes you can !"}, true, {"rtlukark_determined"})
	elseif GetGlobal("ChoiceID1") == 1 then
		SetDialog("SetDialog", {"Too bad that you can't !"}, true, {"rtlukark_pity"})
	end
	if GetGlobal("yes") == null then
	    SetGlobal("yes", 0)
	end
	SetReturnPoint("SetReturnPoint", 1)
    SetChoice("SetChoice", "Are you DETERMINED ?", "Heck yeah !", "Heck no !", 2)
	if GetGlobal("ChoiceID2") == 1 then
	    SetDialog("SetDialog", {"I knew it !"}, true, {"rtlukark_=3"})
	elseif GetGlobal("ChoiceID2") == 0 then
		SetGlobal("yes", GetGlobal("yes") + 1)
		if GetGlobal("yes") == 1 then
			SetDialog("SetDialog", {"Are you sure ?"}, true, {"rtlukark_perv"})
			GetReturnPoint("GetReturnPoint", 1)
		elseif GetGlobal("yes") == 2 then
			SetDialog("SetDialog", {"Are you [w:10][letters:6]REALLY[w:10] sure ?"}, true, {"rtlukark_perv"})
			GetReturnPoint("GetReturnPoint", 1)
		elseif GetGlobal("yes") == 3 then
			SetDialog("SetDialog", {"Oh ok,[w:5] I trust you !","If you're [w:10][letters:2]SO[w:10] determined\nright now,[w:5] I bet you can\nkill my friend !", 
									"Wanna try ?"}, true, {"rtlukark_seriously", "rtlukark_determined","rtlukark_=3"})
			SetBattle("SetBattle", "04 - Animation")
		end
	end
end

function EventPage3()
	if (GetGlobal("GoAway") == null) then
		SetGlobal("GoAway", 0)
	else
		SetGlobal("GoAway", GetGlobal("GoAway") + 1)
	end
	if (GetGlobal("GoAway") == 0) then
		SetDialog("SetDialog", {"Nothing to see here ![w:10]\nJust go away."}, true, {"rtlukark_determined"})
	elseif (GetGlobal("GoAway") == 1) then
		SetDialog("SetDialog", {"Erm[waitall:3]...[waitall:1] [w:10]I told you to\nleave ? [w:10]So please go."}, true, {"rtlukark_normal"})
	
	elseif (GetGlobal("GoAway") == 2) then
		SetDialog("SetDialog", {"Please[waitall:3]...[waitall:1] [w:10]Just go[waitall:3]..."}, true, {"rtlukark_sorry"})
	elseif (GetGlobal("GoAway") == 3) then
		SetDialog("SetDialog", {"..."}, true, {"rtlukark_pity"})
	elseif (GetGlobal("GoAway") == 4) then
		SetDialog("SetDialog", {"So,[w:5] yeah.[w:10] There's one more\nthing I have to show you.", 
								"You know,[w:5] I was with\nsome friends a few days\nago,[w:5] and now[waitall:3]...[waitall:1][w:10] they\nall disappeared.",
								"The only memory I kept\nfrom them is this picture\nof us,[w:5] a few instants\nafter exitting Mt Ebott.",
								"If I can show it to\nyou ?[w:10] [mugshot:rtlukark_pity]Yeah,[w:5] if you really\nwant to[waitall:3]..."}, true, 
								{"rtlukark_pity", "rtlukark_pity", "rtlukark_pity", "rtlukark_surprised"})
		SetToneWithAnim("SetToneWithAnim",0,0,0,128)
		DispImg("DispImg","photo",1,320,240,436,256)
		WaitForInput("WaitForInput")
		RotateEvent("RotateEvent","Image1",0,90,0,1)
		DispImg("DispImg","photoback",1,320,240,436,256)
		RotateEvent("RotateEvent","Image1",0,270,0)
		RotateEvent("RotateEvent","Image1",0,0,0,1)
		WaitForInput("WaitForInput")
		SupprImg("SupprImg", 1)
		SetToneWithAnim("SetToneWithAnim",0,0,0,0)
	elseif (GetGlobal("GoAway") < 8) then
		SetDialog("SetDialog", {"There's no more to see\nhere,[w:5] kiddo. You can go."}, true, {"rtlukark_normal"})
	elseif (GetGlobal("GoAway") < 13) then
		SetDialog("SetDialog", {"You're so patient,[w:5]\nI love this !"}, true, {"rtlukark_=3"})
	elseif (GetGlobal("GoAway") == 13) then
		SetDialog("SetDialog", {"You're boring me right\nnow.[w:10] If you don't stop\nthis,[w:5] I'll be forced to\nuse my special attack."}, true, 
		                       {"rtlukark_normal","rtlukark_normal"})
	elseif (GetGlobal("GoAway") == 14) then
		SetDialog("SetDialog", {"JUST. [w:10]STOP. [w:10]This is\nyour final warning."}, true, {"rtlukark_angry"})
	elseif (GetGlobal("GoAway") == 15) then
		SetDialog("SetDialog", {"Ok,[w:5] here you go ![w:10][next]"}, true, {"rtlukark_angry"})
		GameOver("GameOver")		
	end
end

function EventPage4()
	GameOver("GameOver", "I told you !\rYou should have\nstopped that, heh.")
end