function EventPage1()
	AddItem("Butterscotch Pie")
	SetDialog({"You pickup the Butterscotch Pie."}, true)
	SetEventPage("Bpie", 2)
end

function EventPage2()
	RemoveEvent("Bpie")
end

--[[
Okay, so, now you can use my PC too
You can keep your messages here, it's done for that :P
so do i open unity?
We'll all make on my computer
ok
So, first of all, do you know how to use Tiled ?
i dont think i do
Hmmm, then let's go
Okay, so here is the interface
I see, is this program freeware or do i have to pay for it?
Totally free
cool
I'll make a new map
How many tiles ?
lets go for something simple
Okay, so here is what you have when you've created your map
alright
Now, the first step is to set our current "tileset", the collection of tiles that we'll use
Image files
do these files have to be a determined size and
its elements arranged in a certaiin way?
Well, all that you'll need is tiles of 
TileWidth*TileHeight long, and these tiles
have to be sticked together, no space
ok
Oh wait
You can actually put a space between tiles, 
aka spacing
alright
Okay, so now we have our tileset
Platform/20
oh so like RPG maker, right?
Yup, the same
But here you have unlimited layers
that's great!
Look
Layer
Here, our thing in the second layer
And then...
We don't see this layer anymore
Ahh that's nice. ok so
I assume this all depends on how creative
i get building a tile map. How do I start
workign with it as a game enviorment. and
are there any other tools i should know about
in tiles.
Well... You can make Object Layers
Literally the colliders of the Unity map
Like this :
These rectangles are the colliders of my map
I see
To create them, you do like this:
Here
i see 
are these buttons the colliders?
Yup
Ye
ok.
Then you zoom in to place them well
Use the coordinates ! 
That helps a lot too
will dO
Ctrl + Scroll
woops
alright, i see custom properties,
what does that do?
I never used that, so I don't know
Then
Let's see that map on unity
(rotating mouse = waiting)
ok
Here it is
First of all, with Tiled2Unity, you have to select your Tiled file
No errors, all's good
sorry lost connetion for a while there
Oh, okay
Do you want me to do it again ?
sure, if you want
First of all, with Tiled2Unity, you have to select your Tiled file
No errors, all's good
gotcha
Then, you select your Unity project's Tiled2Unity file
Here we are
Let's preview our map...
This is what we wanted, I guess
ok by unity project i assume a new one or the one already in CYF?
CYF open-source version
ok
Then, click that Big Ass Export Button
DO IT
Done
That's it :D
oh, cool
Now, you need to do one more thing
Unity is long to load
Here's the project, where there are all your files
To find your newly created map, go here : 
But...there might be a lil' problem
The camera doesn't fits.
To fix it, remove the Camera object, and put the one in Assets/Ressources/Prefab
The Main Camera OW one
ok so i assume opening my tiled object opens anew scne, right?
Nope, you have to create that scene
It's just a prefab, a group of GameObjects
ok
Already better, but still
WE SEE SOMETHING
ok maybe i missed somehting though, when did we create the scene this prefab is in?
That's it
ah of course, :V
:P
Then, you have a lot of tricks to do
This map was made on Tiled, as you could see
uh huh
Okay, so there are static values to put for the Z value of the map
141 = Background, 140 = Map, -1 = Foreground
i see so i can make several tileds then
You have to
One per map
Plus...you'll have to copy something
A Background GameObject is ALWAYS needed
yeah i meant that, a tiled for bacground, one for the map and a foreground
i was actually gonna ask in which layer the player was gonan appear but i think that
solves it
Well...the thing is that the GameObject Background is used in my overworld script
So, if you make a map, you HAVE to havee a gameobject named like this
Plus, it'll not be a normal GameObject.
You'll have to copy the Snowdin Background object.
ok
Then, you'll have to adapt it to your map.
First of all, all the objects need a x2 scale
Adapt the position of the map in consequence
Remove the main container
And then try to copy on test3 8)
main container?
The gameObject test-1
This one
No
Don't delete the prefab
that one?
Yup, but now byebye childs
didnt i delete the child objects as well or you mean these?
You deleted what interested us
Tile Layer 1, Tile Layer 2, Object Layer 1
ok so delete again?
Do you know how to move a GameObject ?
to adifferent folder? i think i do
ok so like that?
Yup, but please no parent
alright
Good, so, next part (lemme check something)
Okay
So the next part will be to actually create your map data
First of all, you remove that yurky image
And you transfer a layer to it (if possible, the 140 layer)
Here we are
So, next step : real map data
In MapInfos...
Id = ID of the map. Let's put it at 666
Music : music played, easy
Mod To Load : name of the folder that the mod will try to load. For example, 
in test (Snowdin), it's Examples 2
Here, we'll take... Mionn 2.0 Release.
The music that you chose earlier have to be in Default or in this mod
Understood ?
yes
Cool
Oh, one more thing...
You better want to set all these values to 0
0 everywhere for Sorting Layer Exposed.Order in Layer
And we don't forget the Z values
All position modifications have to be done on all elements
And voilà ! Here's your map. Add events, etc.
Oh wait, one last thing
All objects to Background
Alright, and tile layer 1 is inside tile layer 2?
Tile Layer 1 = Background
ah
How about we test the map ?
sure

Okay, so one last thing : DO NOT KEEP THE CAMERA
The thing has already its own camera, we don't need two of them
ok so i shoudl delete it then
Do it!
Never forget to add your new scene to the build index
ok, and how do i do that?
Go on your map, then File --> Build Settings --> Add Open Scenes
I have to edit something
Okay, so now we go
I'll let you test it
it sounds like there is a problem somewhere
the layers seem to all be above Frisk
Ye
I don't see the problem
Oh, ok
Now test
AAAAAAAAAAAAAAAAAAAAAAA
A BUG
I hope that you don't mind some sounds
i cant hear anything anyways
?i hear it now
:P TeamViewer is so good for this
However...
Here, Map music
I found a bug
I'm so, so disappointed now
Are you sure it's not just our test doing this?
Yup, see
it SHOULD work now
YAY
:D
That feeling when you find your bug like this on a live
yeah, but why is the map above firsk again?
I'll see that
LOL
-140
140-140 = 0
heehee
Frisk columhead
:thumbsup:
So now here we are, with our new map !
You'll just have to add your events and that's it
one more question 
?
how would I link this map to, say, the hotlands lobby
Well... TPs
Now you have Snowdin's TPs
But they're quite unreachable...
I see
And now...
Yeeeeeeeeeeeeeeeeee
nice
I wonder thoufh why it takes so much to load
It just loaded the mod folder Examples 2
oh so, if techically all the stuff from all the maps were in
the same mod folder, woudl it load faster?
It'll not load faster
It'll load ONCE
cool
But it'll take ages to load
I see. And I assume ,a´emcpnters picks from all the mod's encounters at random
so that woudlnt work if I didnt want boss encounters to show up at random
lol, you never read the disclaimer, do you ?
oh, i missed that completeley  in the rush of testing my encounter
im dumb
x)
Unselectionable
Ahh. Well so if hypithetically i were to put all ofmy encouters in a single mod file
how would I assign speicif encounters to appear in onlys pecific maps.
For endgame stuff like the hardmode enemies in The COre and such
I see, but that's not possible for now.
I can work around that with multiple mod folders i guess
This is why I used the system of mod folders
Well
Do you want to know how to make an event ?
sure
kk
Here you are
/me flees
So, now you can do much whatever you want to do, but the events always follow the same
scheme.
Sprite, Rigidbody2D, BoxCollider2D, EventOW, Transform
That's the minimum
alright
Here we are :3
lol 
I was about to comment how i could see forever.
I even saw Cereb's eye in there
You can see all the sprites you have, everywhere
EVERYTHING
So, next part
Yeee, place it and modify its BoxCollider
And then...
Edit EventOW.
First of  all, you'll need the name of the Event script you'll use.
Aka Lua/Event/
Will The Kid be a pie ? A save point ? Or a normal Event ? 
Here's the REAL question
Normal event I suppose
Errr... I putted a restriction about the event pages...
Well
But
For EACH page, you'll have to set a trigger type.
0 = Press Confirm, 1 = Touch, 2 = Auto
We'll stay with Press Confirm
wait, how does the page affect this?
Hm ?
When you mentioned the restrictiona bout pages. What exactly do tey do?
for the event EventPage, I usually press "H" to skip between pages
But meh, The "H" key only works on test2
Okay, we're good now
Tests !
Shit, I screwed up with my text function
Woo, my computer hates screen sharing
dodging projectiles with lag due to sharedscreens too :P
Byebye
Mionn: Fuck this shit Im out
Wait
And it goes like Weeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
heheh what
Rotating dead mionn ftw
MIONNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN
so is this what happens when? or is it just an easter egg
Easter egg in the code, that's it
It was during Banzay's C# lessons, he made that
ahhh
And, that's it I think
alright
so i can call for encounters via events, that's pretty cool
Yup, SetBattle(string encounterName, bool quickAnim = false, bool ForceNoFlee = false)
cool. also you shoudl upload that small fix you did today involving the battel window
The arena ? Yeah, I should
But I have another bug to fix, wait a sec...
Bug : SOUL invisible when going directly to ENEMYDIALOGUE
I wonder what it means, but someone told me that
ahhh
Meh, this bug won't last long. This bug seems easy to solve
brb
kk
back
so is there anything i shoudl know?
Well, you pretty much know everything now, I guess
At least I see nothing more to know
alright
I'll code a bit, if you want to see, stay, or else, exit the stream :P
I may be able to put some music
It's alright. I wanted to put what I learnt into practice. 
I would appreciate if you sent me this "chat log"
Thanks for the lesson, it was fun!
Np ;) I'll sent it to you rn
]]



































