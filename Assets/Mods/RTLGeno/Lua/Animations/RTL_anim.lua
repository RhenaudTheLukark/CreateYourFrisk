return function()
    local self = {
        legs = CreateSprite("RTL/legsnormal"),
        torso = CreateSprite("RTL/torso"),
        head = CreateSprite("RTL/headnormal"),
        isHurt = false,
        inTransformation = false,
        hitAnimCount = 0,
		dilateProgress = 0,
		animPhaseCut = Time.time,
		stopped = false
    }

    self.legs.SetParent(enemies[1]["monstersprite"])
	self.torso.SetParent(self.legs)
	self.head.SetParent(self.torso)

	self.legs.MoveToAbs(320, 240)
	self.torso.MoveTo(1, 0)
	self.head.MoveTo(-1, 0)

	self.legs.SetPivot(0.5, 0)
	self.torso.SetPivot(0.5, 0.5)
	self.head.SetPivot(0.5, 0.22)

	self.blank = CreateSprite("blank", "Top")
	self.blank.alpha = 0
	self.blank.MoveToAbs(320, 240)

	self.SetAnimation = function(anim)
		if anim == "hurt" then
			self.legs.Set("RTL/legshurt")
		elseif anim == "spared" then
			self.legs.Set("RTL/legsspared")
			self.torso.Set("RTL/torsospared")
		end
		self.isHurt = anim == "hurt"
		self.head.Set("RTL/head" .. anim)
	end

	self.HideAnimation = function(isSpared)
		self.head.alpha = 0
		self.torso.alpha = 0
		self.legs.alpha = 0
		enemies[1]["monstersprite"].alpha = isSpared and 1 or 0
	end

	self.Animate = function()
	    -- Transformation animation
	    if self.inTransformation == true then
	        if self.dilateProgress < 100 then
	            self.head.Scale(1 + 0.04 * self.dilateProgress, 1)
	            self.torso.Scale(1 + 0.04 * self.dilateProgress, 1)
	            self.legs.Scale(1 + 0.04 * self.dilateProgress, 1)
	            self.blank.alpha = 0.02 * self.dilateProgress
	        elseif self.dilateProgress == 100 then
                self.head.Scale(1, 1)
                self.torso.Scale(1, 1)
                self.legs.Scale(1, 1)
                self.HideAnimation()
                LukarkAnim.ShowAnimation()
                self.stopped = true
                SwitchEnemies()
	        else
	            self.blank.alpha = 0.05 * (180 - self.dilateProgress)
	        end
	        self.dilateProgress = self.dilateProgress + 1
	        if self.dilateProgress == 180 then
	            self.dilateProgress = 0
	            self.blank.alpha = 0
	            self.inTransformation = false
	        end
	    end

	    -- Normal animation
	    if not self.stopped then
		    local timeCount = 5 * (Time.time - self.animPhaseCut)
	        self.torso.y = math.sin(timeCount)
	        self.head.y = 2 * math.sin(timeCount)
	        if self.isHurt then
		        self.hitAnimCount = self.hitAnimCount + 1
		        if self.hitAnimCount == 40 then
		            self.hitAnimCount = 0
		            self.isHurt = false
		        end
		    end
	    end
	end

	return self
end