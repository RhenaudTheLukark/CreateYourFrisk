cerebbody = CreateSprite("Cereb/Cerebbodysmaller")
stagelefthand = CreateSprite("Cereb/stagelefthand")
stagerighthand = CreateSprite("Cereb/stagerighthand")
stagelefthand.SetParent(cerebbody)
stagerighthand.SetParent(cerebbody)
cerebbody.y = 359
stagelefthand.y = 0
stagerighthand.y = 0
stagelefthand.SetPivot(0.5, 0.5)
stagelefthand.SetAnchor(0.5, 0.5)
stagerighthand.SetPivot(0.5, 0.5)

function AnimateCereb()
    cerebbody.MoveTo(320, 4*math.sin(Time.time) + 359)
end