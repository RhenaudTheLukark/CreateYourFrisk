--For usage, check out the encounter Lua's EncounterStarting() and Update() functions.

-- First, we can create the torso, legs and head.
sanstorso = CreateSprite("sans/sanstorso")
sanslegs = CreateSprite("sans/sanslegs")
sanshead = CreateSprite("sans/sanshead1")

--We parent the torso to the legs, so when you move the legs, the torso moves too. 
--We do the same for attaching the head to the torso.
sanstorso.SetParent(sanslegs)
sanshead.SetParent(sanstorso)

--Now we adjust the height for the individual parts so they look more like a skeleton and less like a pile of bones.
sanslegs.y = 240
sanslegs.x = 320
sanstorso.y = -5 --The torso's height is relative to the legs they're parented to.
sanshead.y = 40 --The head's height is relative to the torso it's parented to.

--We set the torso's pivot point to halfway horizontally, and on the bottom vertically, 
--so we can rotate it around the bottom instead of the center.
sanstorso.SetPivot(0.5, 0)

--We set the torso's anchor point to the top center. Because the legs are pivoted on the bottom (so rescaling them only makes them move up),
--we want the torso to move along upwards with them.
sanstorso.SetAnchor(0.5, 1)
sanslegs.SetPivot(0.5, 0)

--Finally, we do some frame-by-frame animation just to show off the feature. You put in a list of sprites,
--and the time you want a sprite change to take. In this case, it's 1/2 of a second.
sanshead.SetAnimation({"sans/sanshead1", "sans/sanshead2", "sans/sanshead3"}, 1/2)

function AnimateSans()
    sanslegs.Scale(1, 1+0.1*math.sin(Time.time*2))
    sanshead.MoveTo(2*math.sin(Time.time), 40 + 2*math.cos(Time.time))
    sanshead.rotation = 10*math.sin(Time.time + 1)
    sanstorso.rotation = 10*math.sin(Time.time + 2)
end
	

