difficulty = 2
filfy_casul = true
	
	music = "Bastards"
	encountertext = "Mionn will stop you.\nOr die trying."
	nextwaves = {""}
	wavetimer = math.huge
	arenasize = {565,135}

	enemies = {
	"Mionn_Genocide_hard",
	"items",
	"mionn"
	}

	enemypositions = {
	{0,0},
	{500,500},
	{500,500}
	}

function EncounterStarting()
	require "Libraries/Requirements"
	Initialize()
end

-- Most other things in common library