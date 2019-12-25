returnscene = "test"
returnpos = {347, 769}
returndir = 2

music = "mus_shop"

buylist = {
    { "Starfait", "Testing Dog", "Empty Gun", "Cowboy Hat" },
    { "Delicious!", "Barks a lot.", "It's empty,\nbut still\npacks a\npunch!", "Gives off\nfar west\nvibes." },
    { -1, 50, 100, 100 }
}
if GetRealGlobal("OWShopDogBought") then buylist[3][2] = 0 end
if GetRealGlobal("OWShopGunBought") then buylist[3][3] = 0 end
if GetRealGlobal("OWShopHatBought") then buylist[3][4] = 0 end

talklist = {
    { "Job", "Hobbies", "Threaten", "Sell?" },
    {
        { "Me?", "I'm just a shopkeeper." },
        "Just a shopkeeper.",
        { "Threats? I'm not impressed, I'm just a shopkeeper." },
        { "So you have items to sell?",
          "I guess some of them could be useful to me...",
          "Alright, I'll see what you have next time you want to sell something to me!" }
    }
}

maintalk = "Hello there![w:10]\nGlad to meet you,[w:5] I'm just a shopkeeper who lives out of bounds."
buytalk = "Want to look\nat my wares?\nI'm just a\nshopkeeper."
selltalk = { "Sorry, but we're not a pawn shop here, I'm just a shopkeeper.",
             "If you want to sell items you can go to the Temmie Village, Temmies love to collect items.",
             "Where is it you say?",
             "[waitall:5]...[waitall:1]I don't know!" }
selltalk2 = { "Get out with your junk!" }
talktalk = "So you want\nto talk? Ok,\nI'm just a\nshopkeeper."
exittalk = { "Have a good day, I'm a shopkeeper." }

frame = 0
sellTried = false
sold = false
canSell = false

acted = false

function Start()
    background.Set("Overworld/DummyBackground")
end

function EnterBuy()
    if not acted then
        buytalk = "Want to look\nat my wares?\nI'm just a\nshopkeeper."
    else
        acted = false
    end
end

function EnterSell()
    if not canSell then
        if not sellTried then
            Interrupt(selltalk, "MENU")
            sellTried = true
        else
            Interrupt(selltalk2, "MENU")
        end
    end
end

function EnterTalk() end

function EnterMenu() end

function EnterExit() end

function OnInterrupt(nextState) end
function SuccessBuy(item)
    acted = true
    sold = true
    buytalk = "Thanks for\nthe purchase!\nI'm just a\nshopkeeper."
    if item == "Testing Dog" then
        buylist[3][2] = 0
        SetRealGlobal("OWShopDogBought", true)
    elseif item == "Empty Gun" then
        buylist[3][3] = 0
        SetRealGlobal("OWShopGunBought", true)
    elseif item == "Cowboy Hat" then
        buylist[3][4] = 0
        SetRealGlobal("OWShopHatBought", true)
    end
end

function FailBuy(buyerror)
    acted = true
    if buyerror == "full" then
        buytalk = "You can't\ncarry any\nmore items..."
    elseif buyerror == "gold" then
        buytalk = "You don't\nhave enough\nmoney to\nbuy this!"
    end
end

function ReturnBuy()
    if sold then
        maintalk = "Thanks again for\rthe purchase! I'm\rjust a shopkeeper."
    else
        maintalk = "What else? I'm\rjust a shopkeeper."
    end
end

function SuccessSell(item) end

function ReturnSell() end

function SuccessTalk(action)
    if action == "Job" then
        talklist[1][1] = "Shopkeeper"
        talklist[2][1] = {"[noskip]What is a shopkeeper?[w:15] It's my job,[w:10] the best job in the world![w:30][next]",
                          "[noskip]It's all about charisma,[w:10] it's very complicated.[w:30][next]",
                          "[noskip]Only a few people can become a shopkeeper,[w:10] you know?[w:15] I was lucky enough to be one of them![w:30][next]",
                          "[noskip]I think that you too can make it,[w:10] if you want to![w:30][next]",
                          "[noskip]I'll teach you the basics on how to become a shopkeeper![w:30][next]",
                          "[noskip]First of all,[w:10] you need to know about our currency,[w:10] G.[w:30][next]",
                          "[noskip]Gold is the Und[func:Drowsy]erground's main currency![w:15] It's made of 89% Gold,[w:10] 5% aluminium,[w:10] 5% zinc[w:10] and 1% tin.[w:30][next]",
                          "[noskip]It can't be made of 100% Gold,[w:10] otherwise it couldn't take the shape [novoice]of a coin...[w:120][next]",
                          "[noskip][func:Undrowsy]And so I sold this mop to that kid, and now everyone calls[novoice] him \"Mop Kid\"! He was pretty happy about it...[w:120][next]",
                          "[noskip][func:Undrowsy2]And that's all for the basics![w:15] If you want to, I can teach you how to make profit![w:30][next]",
                          "[noskip]No?[w:15] Very well then, ask me if you want to learn how to be a shopkeeper later![w:30][next]"}
    elseif action == "Shopkeeper" then
        talklist[2][1] = "[novoice](NEVER AGAIN)"
    elseif action == "Sell?" then
        talklist[1][4] = nil
        talklist[2][4] = nil
        canSell = true
    end
end


function Drowsy()
    eyeLidTop = CreateSprite("px", "Top")
    eyeLidTop.color = { 0, 0, 0 }
    eyeLidTop.absx = 320
    eyeLidTop.absy = 600
    eyeLidTop.Scale(640, 240)
    eyeLidBottom = CreateSprite("px", "Top")
    eyeLidBottom.color = { 0, 0, 0 }
    eyeLidBottom.absx = 320
    eyeLidBottom.absy = -120
    eyeLidBottom.Scale(640, 240)
    eyeLidEffect = CreateSprite("px", "Top")
    eyeLidEffect.color = { 0, 0, 0 }
    eyeLidEffect.absx = 320
    eyeLidEffect.absy = 240
    eyeLidEffect.Scale(640, 480)
    eyeLidEffect.alpha = 0
    maintext = CreateText({"[font:uidialoglilspace][novoice][noskip][waitall:2]You're feeling drowsy..."}, {340, 440}, 320, "Top", 100)
    maintext.progressmode = "auto"
    maintext.SetAutoWaitTimeBetweenTexts(80)
    maintext.SetEffect("none", -1)
    maintext.HideBubble()
    maintext.alpha = 0.5
end

function Undrowsy()
    frame = 2000
end

function Undrowsy2()
    frame = 4000
end

function Update()
    if maintext and maintext.allLinesComplete then
        if frame < 160 then
            eyeLidEffect.alpha = math.abs(.5 * math.sin(frame * math.pi / 80))
        elseif frame < 240 then
            eyeLidBottom.absy = eyeLidBottom.absy + 1.75
        elseif frame < 320 then
            eyeLidBottom.absy = eyeLidBottom.absy + 1.75 * math.cos((frame - 240) * math.pi / 80)
        elseif frame < 400 then
            eyeLidBottom.absy = eyeLidBottom.absy - 1.75 * math.cos((frame - 320) * math.pi / 80)
        elseif frame >= 520 and frame < 760 then
            eyeLidBottom.absy = eyeLidBottom.absy + (120 - eyeLidBottom.absy) * 0.02
        elseif frame == 760 then
            eyeLidBottom.absy = 120
            Audio.Pause()
        elseif frame >= 2000 and frame < 2010 then
            eyeLidBottom.absy = eyeLidBottom.absy - 15
            if frame == 2000 then
                NewAudio.SetVolume("src", 0.75)
                Audio.Unpause()
            end
        elseif frame >= 2120 and frame < 4000 and eyeLidBottom.absy < 120 then
            eyeLidBottom.absy = eyeLidBottom.absy + 2
        elseif frame >= 2120 and frame < 4000 and Audio.IsPlaying then
            Audio.Pause()
        elseif frame >= 4000 and eyeLidBottom.absy > -120 then
            eyeLidBottom.absy = eyeLidBottom.absy - 15
            if frame == 4000 then
                NewAudio.SetVolume("src", 0.75)
                Audio.Unpause()
            elseif eyeLidBottom.absy <= -120 then
                maintext = nil
            end
        end
        if frame >= 160 then
            eyeLidTop.absy = 480 - eyeLidBottom.absy
            eyeLidEffect.alpha = eyeLidBottom.absy / 180
            NewAudio.SetVolume("src", math.min(0.75, 0.75 - ((600 - eyeLidTop.absy) / 330)))
        end
        frame = frame + 1
    end
end