cerebbody = CreateSprite("Cereb/Cerebbodysmaller")
mask = CreateSprite("Cereb/Mask")
mask2 = CreateSprite("Cereb/Maskcrack")
mask3 = CreateSprite("Cereb/Maskbreak")
stagelefthand = CreateSprite("Cereb/stagelefthand")
stagerighthand = CreateSprite("Cereb/stagerighthand")
face = CreateSprite("Cereb/smile")
mask.SetParent(cerebbody)
mask2.SetParent(cerebbody)
mask3.SetParent(cerebbody)
stagelefthand.SetParent(cerebbody)
stagerighthand.SetParent(cerebbody)
face.SetParent(cerebbody)
mask.alpha = 0
mask2.alpha = 0
mask3.alpha = 0
cerebbody.y = 359
stagelefthand.y = 0
stagerighthand.y = 0
face.y = -13
face.alpha = 0
stagelefthand.SetPivot(0.5, 0.5)
face.SetPivot(0.5, 0.5)
stagelefthand.SetAnchor(0.5, 0.5)
stagerighthand.SetPivot(0.5, 0.5)

function AnimateCereb()
    cerebbody.MoveTo(320, 4*math.sin(Time.time) + 359)
end