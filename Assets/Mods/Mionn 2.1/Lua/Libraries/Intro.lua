intro = false

function DoIntro()
	BattleDialog({"[starcolor:ff0000][color:ff0000]This pathetic excuse for a\rmonster is trying to stop me.[w:30]\nHow foolish."})
end

function StartFight()
	Animate(Anim_To_Fight)
end