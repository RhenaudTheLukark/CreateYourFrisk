returnscene = "test"
returnpos = {640, 480}
returndir = 6music = "mus_shop"

buylist = { 
    { "Butterscotch Pie", "The Locket", "Real Knife", "Testing Dog" }, 
    { "Heals ALL HP.\nI like pie.", "Can't open it.", "Ouch", "Bark" }, 
    { 500, 1, 99999, -1 } 
} 

talklist = { { "Job", "Hobbies", "Threaten" },
           { {"Me?", "I'm just a shopkeeper." }, "Just a shopkeeper.", { "Threats? I'm not impressed, I'm just a shopkeeper." } } }
maintalk = "Hello there! Glad\rto meet you, I'm\rjust a shopkeeper."
buytalk = "Want to look\nat my wares?\nI'm just a\nshopkeeper."
selltalk = {"Sorry but we're not a pawn shop here, I'm just a shopkeeper.",
            "If you want to sell items you can go to the Temmie Village, Temmies love to collect items.",
            "Where is it you say?",
            "[waitall:5]...[waitall:1]I don't know!"}
selltalk2 = {"Get out with your junk!"}
talktalk = "So you want\nto talk? Ok,\nI'm just a\nshopkeeper."
exittalk = { "Have a good day, I'm a shopkeeper." }

frame = 0
selldone = false
sold = false

acted = false

function Start()
    background.Set("DummyBackground")
end

function EnterBuy()
    if not acted then
        buytalk = "Want to look\nat my wares?\nI'm just a\nshopkeeper."
    else
        acted = false
    end
end

function EnterSell()
    --if not selldone then 
    --    Interrupt(selltalk, "MENU")
    --    selldone = true
    --else                 
    --    Interrupt(selltalk2, "MENU")
    --end        
end

function EnterTalk() end

function EnterMenu() end

function EnterExit() end

function OnInterrupt(nextState) end
function SuccessBuy(item)    acted = true    sold = true    buytalk = "Thanks for\nthe purchase!\nI'm just a\nshopkeeper."end
function FailBuy(buyerror)    acted = true    if buyerror == "full" then        buytalk = "You can't\ncarry any\nmore items..."    elseif buyerror == "gold" then        buytalk = "You don't\nhave enough\nmoney to\nbuy this!"    endend

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
        --[[Lol that thing will never be used
        talklist["death"] = {"[noskip]What are you doing?",
                             "[noskip]Don't go near me! I won't let y-[next]",
                             "[noskip][func:KillAnim][w:60][next]",
                             "[noskip][waitall:5]...No...[w:60][func:Kill]"}]]
        talklist[2][1] = {"[noskip]What is a shopkeeper?[w:20] It's my job,[w:10] the best job in the world![w:60][next]",
                          "[noskip]It's all about charisma,[w:10] it's very complicated.[w:60][next]",
                          "[noskip]Only a few people can become a shopkeeper,[w:10] you know?[w:20] I was lucky enough to be one of them![w:60][next]",
                          "[noskip]I think that you too can make it,[w:10] if you want to![w:60][next]",
                          "[noskip]I'll teach you the basics on how to become a shopkeeper![w:60][next]",
                          "[noskip]First of all,[w:10] you need to know about our currency,[w:10] G.[w:60][next]",
                          "[noskip]Gold is the Und[func:Drowsy]erground's main currency![w:20] It's made of 89% of Gold,[w:10] 5% of aluminium,[w:10] 5% of zinc and 1% of tin.[w:60][next]",
                          "[noskip]It can't be made of 100% of Gold,[w:10] otherwise it couldn't take the shape[novoice] of a coin...[w:120][next]",
                          "[noskip][func:Undrowsy]And so I sold this mop to that kid, and now everyone calls[novoice] him \"Mop Kid\"! He was pretty happy about it...[w:120][next]",
                          "[noskip][func:Undrowsy2]And that's all for the basics![w:20] If you want to, I can teach you how to make profit![w:60][next]",
                          "[noskip]No?[w:20] Very well then, ask me if you want to learn how to be a shopkeeper later![w:60][next]"}
    elseif action == "Shopkeeper" then
        talklist[2][1] = "[novoice](NEVER AGAIN)"
    end
end


function Drowsy() 
    eyeLidTop = CreateSprite("px", "Top")
    eyeLidTop.absx = 320
    eyeLidTop.absy = 600
    eyeLidTop.Scale(640, 240)
    eyeLidBottom = CreateSprite("px", "Top")
    eyeLidBottom.absx = 320
    eyeLidBottom.absy = -120
    eyeLidBottom.Scale(640, 240)
    eyeLidEffect = CreateSprite("px", "Top")
    eyeLidEffect.absx = 320
    eyeLidEffect.absy = 240
    eyeLidEffect.Scale(640, 480)
    eyeLidEffect.alpha = 0
    maintext = CreateText({"[font:uidialoglilspace][novoice][noskip][waitall:2]You're feeling drowsy..."}, {360, 400}, 320, "Top", 100)
    maintext.progressmode = "auto"
    maintext.SetAutoWaitTimeBetweenTexts(80)
    maintext.SetEffect("none", -1)
    maintext.HideBubble()
end

function Undrowsy() 
    frame = 2000
end

function Undrowsy2() 
    frame = 4000
end

function Update()
    if maintext and maintext.IsTheTextFinished() then
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
            Audio.Play()
        elseif frame >= 2120 and frame < 4000 and eyeLidBottom.absy < 120 then  
            eyeLidBottom.absy = eyeLidBottom.absy + 2
        elseif frame >= 2120 and frame < 4000 and Audio.IsPlaying() then  
            Audio.Pause()
        elseif frame >= 4000 and eyeLidBottom.absy > -120 then  
            eyeLidBottom.absy = eyeLidBottom.absy - 15
            Audio.Play()
        end
        if frame >= 160 then
            eyeLidTop.absy = 480 - eyeLidBottom.absy 
            eyeLidEffect.alpha = eyeLidBottom.absy / 180
        end
        frame = frame + 1
    end
end