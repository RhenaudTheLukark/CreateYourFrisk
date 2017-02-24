music = "Bastards"
encountertext = "Mionn will stop you.\nOr die trying."
nextwaves = {""}
wavetimer = 0.0
arenasize = {565,135}
difficulty = 0

enemies = {
"Mionn_Genocide_easy",
"items"
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