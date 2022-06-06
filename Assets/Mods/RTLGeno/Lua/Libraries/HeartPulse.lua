return function()
    local self = {
        hpPulseCoeffs = {
            { percent = .05, value = 1.3  },
            { percent = .1,  value = 0.7  },
            { percent = .15, value = 1.15 },
            { percent = .2,  value = 1    },
            { percent = 1,   value = 1    },
        },
        playerHPPulseCycleTime = 40,
        frameCount = 0
    }

    self.GetCoeffFromPercent = function(percent)
        local lowPercent = nil
        local highPercent = 0
        local lowValue = nil
        local highValue = 1
        local id = 1
        repeat
            lowPercent = highPercent
            lowValue = highValue
            highPercent = self.hpPulseCoeffs[id].percent
            highValue = self.hpPulseCoeffs[id].value
            id = id + 1
        until highPercent >= percent
        highPercent = highPercent - lowPercent
        percent = percent - lowPercent
        return highValue * (percent / highPercent) + lowValue * (1 - (percent / highPercent))
    end

    self.PulseByHP = function()
        if self.frameCount < self.playerHPPulseCycleTime then
            local scale = self.GetCoeffFromPercent(self.frameCount / self.playerHPPulseCycleTime)
            Player.sprite.Scale(scale, scale)
            self.frameCount = self.frameCount + 1
        else
            self.playerHPPulseCycleTime = 40 + (1 - (Player.hp / Player.maxhp)) * 140
            self.frameCount = 0
        end
    end

    return self
end