return function()
    local self = {
        hair = CreateSprite("Lukark/hair/1"),
        head = CreateSprite("Lukark/headnormal"),
        legs = CreateSprite("Lukark/legs"),
        torso = CreateSprite("Lukark/torso"),
        arms = CreateSprite("Lukark/arms/1"),
        isHurt = false,
        hitAnimCount = 0,
        animPhaseCut = Time.time,
        stopped = false,
        armAnimations = {
            normal = { "Lukark/arms/1", "Lukark/arms/1", "Lukark/arms/1", "Lukark/arms/1", "Lukark/arms/1",
                       "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2",
                       "Lukark/arms/3", "Lukark/arms/3", "Lukark/arms/4", "Lukark/arms/4", "Lukark/arms/3",
                       "Lukark/arms/3", "Lukark/arms/4", "Lukark/arms/4", "Lukark/arms/3", "Lukark/arms/3",
                       "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2", "Lukark/arms/2", time = 0.04, loop = true },

            waveballbegin = { "Lukark/arms/ballmove1", "Lukark/arms/ballmove2", "Lukark/arms/ballmove3", "Lukark/arms/ball1-1" },
            waveballend =   { "Lukark/arms/ballmove3", "Lukark/arms/ballmove2", "Lukark/arms/ballmove1", "Lukark/arms/1"       },

            ["waveball1-1to2"] = { "Lukark/arms/ballmove1-4", "Lukark/arms/ballmove1-5", "Lukark/arms/ball1-2" },
            ["waveball1-2to1"] = { "Lukark/arms/ballmove1-5", "Lukark/arms/ballmove1-4", "Lukark/arms/ball1-1" },
            ["waveball2-1to2"] = { "Lukark/arms/ballmove2-4", "Lukark/arms/ballmove2-5", "Lukark/arms/ball2-2" },
            ["waveball2-2to1"] = { "Lukark/arms/ballmove2-5", "Lukark/arms/ballmove2-4", "Lukark/arms/ball2-1" },

            ["waveball1to2-1"] = { "Lukark/arms/ballmove4-1", "Lukark/arms/ballmove5-1", "Lukark/arms/ball2-1" },
            ["waveball1to2-2"] = { "Lukark/arms/ballmove4-2", "Lukark/arms/ballmove5-2", "Lukark/arms/ball2-2" },
            ["waveball2to1-1"] = { "Lukark/arms/ballmove5-1", "Lukark/arms/ballmove4-1", "Lukark/arms/ball1-1" },
            ["waveball2to1-2"] = { "Lukark/arms/ballmove5-2", "Lukark/arms/ballmove4-2", "Lukark/arms/ball1-2" },

            ["waveball1to2-1to2"] = { "Lukark/arms/ballmove4-4", "Lukark/arms/ballmove5-5", "Lukark/arms/ball2-2" },
            ["waveball1to2-2to1"] = { "Lukark/arms/ballmove4-5", "Lukark/arms/ballmove5-4", "Lukark/arms/ball2-1" },
            ["waveball2to1-1to2"] = { "Lukark/arms/ballmove5-4", "Lukark/arms/ballmove4-5", "Lukark/arms/ball1-2" },
            ["waveball2to1-2to1"] = { "Lukark/arms/ballmove5-5", "Lukark/arms/ballmove4-4", "Lukark/arms/ball1-1" }
        }
    }

    self.arms.SetAnimation(self.armAnimations.normal, 0.04)
    self.hair.SetAnimation({ "Lukark/x1/hair/1", "Lukark/x1/hair/2", "Lukark/x1/hair/3",
                             "Lukark/x1/hair/4", "Lukark/x1/hair/3", "Lukark/x1/hair/2" }, 0.2)
    self.hair.Scale(2, 2)

    self.hair.SetParent(enemies[2]["monstersprite"])
    self.head.SetParent(self.hair)
    self.legs.SetParent(enemies[2]["monstersprite"])
    self.torso.SetParent(self.legs)
    self.arms.SetParent(self.torso)

    self.legs.MoveToAbs(320, 340)
    self.arms.MoveToAbs(320, 340)
    self.hair.MoveToAbs(320, 334)
    self.head.MoveToAbs(321, 335)
    self.torso.MoveToAbs(320, 340)

    self.legs.SetPivot(0.5, 0)
    self.legs.SetAnchor(0.5, 0)
    self.torso.SetPivot(0.5, 1)
    self.torso.SetAnchor(0.5, 1)
    self.hair.SetPivot(0.5, 1)

    self.legs.MoveTo(0, 0)
    self.torso.MoveTo(0, 0)

    self.SetFace = function(face)
        self.head.Set("Lukark/head" .. face)
        self.isHurt = face == "hurt"
    end

    self.ShowAnimation = function()
        self.head.alpha = 1
        self.torso.alpha = 1
        self.legs.alpha = 1
        self.arms.alpha = 1
        self.hair.alpha = 1
    end

    self.HideAnimation = function(isKilled)
        self.torso.alpha = 0
        self.legs.alpha = 0
        self.arms.alpha = 0
        self.hair.alpha = 0
        self.head.alpha = 0
        if isKilled then enemies[2]["monstersprite"].alpha = 1 end
    end

    self.SetArmAnim = function(anim)
        if self.armAnimations[anim] then
            self.arms.SetAnimation(self.armAnimations[anim], self.armAnimations[anim].time or 0.1)
            self.arms.loopmode = self.armAnimations[anim].loop and "LOOP" or "ONESHOT"
        else
            self.arms.StopAnimation()
            self.arms.Set("Lukark/arms/" .. anim)
        end
        self.currentAnimation = anim
    end

    self.Animate = function()
        if not self.stopped then
            local timeCount = (Time.time - self.animPhaseCut) * 2
            self.legs.Scale(1, 1 + 0.05 * math.sin(timeCount))
            self.torso.y = -5 * math.sin(timeCount)
            self.hair.absy = self.torso.absy

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