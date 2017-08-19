function EventPage0()
    Event.SetPage(Event.GetName(), -1)
end

function EventPage1()
	Inventory.AddItem("Butterscotch Pie")
	General.SetDialog({"You pickup the Butterscotch Pie."}, true)
	Event.SetPage(Event.GetName(), -1)
    --Event.SetPage(Event.GetName(), 2) --You shouldn't make "removing" pages directly. Instead, set -1 as the page of the event!
end

--You shouldn't if you want the event to never come back. Use Event.SetPage(Event.GetName(), -1) to make so the event disappears once and for all!
function EventPage2()
	Event.Remove(Event.GetName())
end

--Test auto mode
function EventPage3()
    --local ev = Event.GetName()
	General.SetDialog({"[voice:monsterfont]Oh hi, I'm a talking pie!", 
                       "[voice:monsterfont]Sorry for disturbing, I didn't wanted to annoy you, but...", 
                       "[voice:monsterfont]You know, I've always been a fan of you.",
                       "[voice:monsterfont]Maybe that you haven't noticed me, but I always was here...",
                       "[voice:monsterfont]It was me, on the table, that you rejected because you were full.",
                       "[voice:monsterfont]It was me, under the bench, who was waiting for you to pick me up.",
                       "[voice:monsterfont]It was me, in the bin, who prayed that you'd eat me while looking at me in disgust.",
                       "[voice:monsterfont]But at the end, it's not that important.",
                       "[voice:monsterfont]The most important is that we're now together.",
                       "[voice:monsterfont]And I'd love you to eat me."}, true)
    General.SetChoice({"Yes", "No way!"}, "Will you eat the pie?")
    if lastChoice == 0 then
        General.SetDialog({"[voice:monsterfont]Finally! I'll finally get in your belly!", 
                           "You ate the pie[waitall:10]... [waitall:1][health:Max]Your HP has been maxed out."}, true)
	    Event.SetPage(Event.GetName(), -1)
    else
        General.SetDialog({"[voice:monsterfont]After all I endured, you'll just [noskip][w:10][letters:3]NOT[w:20][noskip:off] eat me?"}, true)
        General.SetChoice({"It was a joke", "Indeed!"}, "Will you NOT eat the pie?")
        if lastChoice == 0 then
            General.SetDialog({"[voice:monsterfont]Pfew, you got me here[waitall:10]...[waitall:1] Open your mouth!", 
                               "You ate the pie[waitall:10]... [w:20][waitall:1][health:Max]Your HP has been maxed out."}, true)
	        Event.SetPage(Event.GetName(), -1)
        else
            General.SetDialog({"[voice:monsterfont][noskip] [waitall:10]...",
                               "[voice:monsterfont][noskip] [effect:shake][waitall:2]Do you think this is a game?"}, true)
            General.SetChoice({"Ye", "No no"}, "Is this a game for you?")
            if lastChoice == 0 then
                General.SetDialog({"[voice:monsterfont][effect:none]Alright.",
                                   "[voice:monsterfont][effect:none]I guess that this will do nothing, then?"}, true)
		        General.GameOver({ "Oops, it did!\nEat me, the next time." })
            else
                General.SetDialog({"[voice:monsterfont]Good. Then eat me.", 
                                   "You ate the pie[waitall:10]... [w:20][waitall:1][health:Max]Your HP has been maxed out."}, true)
	            Event.SetPage(Event.GetName(), -1)
            end
        end
    end
end