temp_anim = 0

hair = CreateSprite("Lukark/hair/1")
head = CreateSprite("Lukark/headnormal")
legs = CreateSprite("Lukark/legs")
torso = CreateSprite("Lukark/torso")
arms = CreateSprite("Lukark/arms/1")

arms.SetAnimation({"Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1","Lukark/arms/1",
				   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2",
				   "Lukark/arms/3","Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3",
				   "Lukark/arms/3","Lukark/arms/4","Lukark/arms/4","Lukark/arms/3","Lukark/arms/3",
				   "Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2","Lukark/arms/2"}
				   , 1/25)
hair.SetAnimation({"Lukark/hair/1","Lukark/hair/2","Lukark/hair/3",
				   "Lukark/hair/4","Lukark/hair/3","Lukark/hair/2"}
				   , 1/5)
				   
legs.x = 120   legs.y = 340
arms.x = 320   arms.y = 340
hair.x = 320   hair.y = 340
head.x = 320   head.y = 340
torso.x = 320  torso.y = 340

head.SetPivot(0.5, 0.5)
arms.SetPivot(0.5, 0.5)
torso.SetPivot(0.5, 0.5) torso.SetAnchor(0.5, 0.5)
legs.SetPivot(0.5, 0.5)
hair.SetPivot(0.5, 0.5)

function Animate(animation)
    head.Set("Lukark/head" .. animation)
end

function AnimateLukark()	
	legs.Scale(1, 1+0.05*math.sin(temp_anim*2))
	legs.MoveTo(320, 340+(5.15*math.sin(temp_anim*2)))
	arms.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
	head.MoveTo(321, 342+(5.15*math.sin(temp_anim*2)))
	hair.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
	torso.MoveTo(320, 341+(5.15*math.sin(temp_anim*2)))
	temp_anim = temp_anim + Time.dt	
end