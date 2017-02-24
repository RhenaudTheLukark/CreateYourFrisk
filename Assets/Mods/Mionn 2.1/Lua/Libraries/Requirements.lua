function Initialize()
	require "Libraries/CommonEncounter_Geno"
	require "Libraries/Intro"
	require "Libraries/Fun_Stuff"
	require "Libraries/Mionn_Anims"
	require "Libraries/Soul_Shatter"
	require "Libraries/Flame_Effect"
	require "Libraries/Menu_Attacks"
		
	Player.lv = 19
	Player.hp = 92
	Player.name = 'CHARA'
	DoIntro()
	Audio.Pause()
end