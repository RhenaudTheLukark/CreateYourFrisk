function Fun()
	Encounter.Call("Fun_Enfic_Start")
end

function Changed()
	Encounter['Fun_Noises'] = true
	BattleDialog({"[noskip]Something's changed.[w:10][waitall:0]\r         changed\r         changed\r         changed\r         changed\r         changed\r         changed[func:Fun][w:9999]"})
	--BattleDialog({"[noskip]Something's changed.[w:45][func:FunText]"})
end