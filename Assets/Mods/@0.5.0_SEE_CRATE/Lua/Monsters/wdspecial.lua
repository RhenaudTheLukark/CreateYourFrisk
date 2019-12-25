comments = {"Thanks for the improvement"}
commands = {"Little", "Gift", "For", "You!"}
randomdialogue = {"Enjoy! ;D"}
sprite = "WDSpecial/A1" --Always PNG. Extension is added automatically.
name = "WDSpecial"
hp = 1 --Nobody will touch the little goat thing!
atk = 0 --Too innocent to damage you ;P
def = -999 --Nobody will touch the goat!
check = "A little surprise for WD200019!\nI'll kill you if you kill him."
dialogbubble = "left" -- See documentation for what bubbles you have available.
dialogueprefix = "[effect:none]"
cancheck = true
canspare = true
voice = "v_asriel"

function HandleAttack(attackstatus) Misc.DestroyWindow() end --As I said, NODOBY TOUCH THE GOAT

function Animate(animation) SetSprite("WDSpecial/" .. animation) end

function Hide() monstersprite.alpha = 0 end
function Show() monstersprite.alpha = 1 end

function GetCloser2() Encounter["enemies"][2].Call("GetCloser2") end

function SetBubble(bubble) dialogbubble = bubble end

function HandleCustomCommand(command) end