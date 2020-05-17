-- A basic encounter script skeleton you can copy and modify for your own creations.

-- music = "shine_on_you_crazy_diamond" --Either OGG or WAV. Extension is added automatically. Uncomment for custom music.
encountertext = "Poseur strikes a pose!" --Modify as necessary. It will only be read out in the action select screen.
nextwaves = {"bullettest_chaserorb"}
wavetimer = 4.0
arenasize = {155, 130}

enemies = {
"poseur"
}

enemypositions = {
{0, 0}
}

-- A custom list with attacks to choose from. Actual selection happens in EnemyDialogueEnding(). Put here in case you want to use it.
possible_attacks = {"bullettest_bouncy", "bullettest_chaserorb", "bullettest_touhou"}

function EncounterStarting()
	Audio.Stop()
	--vid = CreateVideoPlayer("Test/Pirouette+", true)
	--vid = CreateVideoPlayer("Test/Second_Dream_MV")
	--vid.Prepare()
	--vid.Play()
	--vid.islooping = false

	--DEBUG(vid.isinfront)
	--DEBUG(vid.aspectratio)
	--vid.aspectratio = "FitOutside"

	--enemies[1]["monstersprite"].SetShader("AdditiveColor")
	--enemies[1]["monstersprite"].SetShaderProperty("_R", 0.5)
	--enemies[1]["monstersprite"].SetShaderProperty("_B", 1)

	--enemies[1]["monstersprite"].SetAnimation({"poseur", "posette"}, 1)
	--enemies[1]["monstersprite"].Set("posette")

	enemies[1]["monstersprite"].SetShader("TheUndying")
	enemies[1]["monstersprite"].SetShaderProperty("_Limit", 0.5)

	Discord.SetName("Super Duper Totally Not Fake Name!")
	Discord.SetDetails("I'm rich, BOIIIII!")
	--Discord.SetElapsedTime(1)

	--Misc.ScaleScreen(1, -1)
    -- If you want to change the game state immediately, this is the place.
end

timer = 0
function Update()
	timer = timer + 1
	--Misc.screenrotation = Misc.screenrotation + 5
	if (timer % 3 == 0) then
		--Misc.RotateScreenAdvanced(math.random(-70, 70), math.random(-70, 70), math.random(-180, 180))
	end
	--if (timer < 30 and (not vid.isactive)) then
	--	DEBUG("Flipped Da Screen")
	--	Misc.ScaleScreen(1, -timer/15 + 1)
	--end
	--DEBUG(vid.currentframe)
	--vid.alpha = math.sin(math.rad(timer*3))*0.5 + 0.5
	--if (Input.GetKey("W") == 1) then
	--	vid.currentframe = 10
	--end
	--if (Input.GetKey("P") == 1) then
	--	vid.Play()
	--end
	--if (Input.GetKey("D") == 1) then
	--	if (vid.isactive) then
	--		vid.Remove()
	--		timer = 0
	--		DEBUG("Removed!")
	--	end
	--end
	NewAudio.SetVolume("src", 0.2)
end

function EnemyDialogueStarting()
    -- Good location for setting monster dialogue depending on how the battle is going.
end

function EnemyDialogueEnding()
    -- Good location to fill the 'nextwaves' table with the attacks you want to have simultaneously.
    nextwaves = { possible_attacks[math.random(#possible_attacks)] }
end

function DefenseEnding() --This built-in function fires after the defense round ends.
    encountertext = RandomEncounterText() --This built-in function gets a random encounter text from a random enemy.
end

function HandleSpare()
    State("ENEMYDIALOGUE")
end

function HandleItem(ItemID)
    BattleDialog({"Selected item " .. ItemID .. "."})
end