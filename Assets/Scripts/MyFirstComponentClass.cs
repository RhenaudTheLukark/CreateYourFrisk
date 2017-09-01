//LESSON 1
/*
So, here we begin
We'll take this class as an example
Tell me what you wanted to say
I just clicked back to the window 
:PX)
Bad start
Erm... First of all, all C# scripts are contained in classes (sorry for the wordreference)
np :)
There is a big difference between Lua and C# : Lua is a compiled language (that means that it don't need classes and such) and C# is a class-oriented language (it need classes)
Understood ?
somewhat; let's start with what classes actually do
Classes are objects that contains functions and variables. It's like an entire Lua script.
I see
Here, we can see these keywords : public (that means that the class is accessible from everywhere) and class, and then the class name.
Here, you'll not use "end" and "then" keywords : it's only { and }.
The good thing with Visual Studio is that it corrects all your mistakes, example
Don't worry about making mistakes, these red bastards will help you a lot :P
So they act similar to grammar mistakes in regular text editors
Exact
Plus, C# variables are initialized with this "schema" : TYPE name = (new) value. The new keyword is used when you initialize complex variables, such as arrays or custom classes.
UNderstood ?
I think a concrete example will help here; I understand regular variables and how the type stuff works I think
Okay, so I4ll ask you something : create a string named myFirstSentence with "yay" as value. :P
anywhere I assume
IN Something()
Sir yes sir!
I almost forgot, C# uses semicolons x)
Yes, I just remembered that :P
Visual Studio helps a lot with this kind of mistakes :P
brb
kk
back
kay, I'm here too
Okay, so you're good for now :3
MAy we try something harder ?
let's discuss what void does for example
void is the return type of the function : you can replace it by int, float, string, LuaINventory etc..
As the function returns nothing, the return type is "void"
ok, so if I understand correctly, everything that is 'void' will run, but not return, and everything that is not will?
Eveything that is not "void" will need a return value!
Example :
Here it works, 
:P
There is two types of classes : normal classes, that need to be instantiated to be used, like LuaInventory
And there's another class type, "static" classes, like Inventory.
We can use it directly, everywhere, at every moment
Do you see now where you're going ?
I'm starting to grasp it
  
To sum-up :
- Classes are the basis element of C# coding.
- There is two type of classes : normal classes that needs an initialization to be used and static classes that can be used directly from everywhere.
- Variables follows this schema : TYPE name = (new) value (new is needed for complex variables)
- Function has return types, like 'int' or 'LuaInventory'. You can replace it per 'void' if you want the function to return nothing

Is it more clear now ?
C# is like boxes in boxes, as you can see here (nvm for the search)
ok, this boxes in boxes idea works well with me; I already use functions within functions left and right in Unitale, and require libraries all the time; it's somewhat like that
Yup
So, let's get to the point. Normally, you'll need one file for each class, but as it is an example, you'll use this file.

Can you make a normal class, with two functions called "DoesAThing" and "HANDZ" : the first returns nothing and the second an object "Handz", 
one variable called "yes" that is an int and "no" that is a boolean. Here you go!
The nae of the class will be "ANormalCClass"

one final question: by object we mean? - I'll probably jump around a lot at first; and it will be a public class
Object  = class, do as I did with variables (LuaInventory luai = ...) LuaInventory is an object/class
Check your errors, red things are your friends
ah!
???
it appears I'm still confused with returning an object
You can't make classes in classes. I can show it to you, but it'll not be nice
Just show me an example where it is done and I'll go from thre
Here you go!
AH! I get it now; I think :D
ok, so I know how to return a single value, that's no problem, but I still have difficulty understanding how to return two different types
Wait, I'll do sth
Do I have to show you the answer ?
So, here's the answer

Here's your class! Good job!
I think I must have misunderstood the task it seems
Yeah, I must havebeen more clear about that, soory :/
I thought I had to put
I know, but if I wanted you to do so, I'd have said : and put two arguments...
But hey, that was a great start ^^ i have to go eat
yes and no in to Handz

public class Handz { }

public class ANormalCClass {
    int yes;
    bool no;
    public string Something() {
        string myFirstSentence = "yay";
        return myFirstSentence;
    }
    public void DoesAThing() { }

    public Handz HANDZ() {
        return new Handz();
    }
}*/

//LESSON 2
/*
Hey
h0i, it's so smooth :D
200Mb/s man :P
Okay, so
We'll start with unity's basic elements
let me know if you need confirmation at any point
kk
Unity is organised by some basic elements : we'll start with the most basic element : Scenes
Scenes are...like...a graphic space where you put other elements : 
you can create scenes, load them one by one or load some asynchronously...
In this case, my scene is named "TransitionOverworld", but ig i'll call other scenes.
Example : now I'm on test2
Understood ?
So far so good.
Okay
I'll show you these elements, and then you'll try to create your first scene
Is the music perturbing you ?
actually, I turned off sound last time :D
D:
Now we're on ModSelect etc...
Okay, next step
So, these scenes contains other elements : GameObjects
On Unity, we can see them here
We can see that GameObjects works in an hierarchy : 
Canvas contains all GameObjects under it
I don't think that you have a gameObject limit, but don't put too much 
gameObjects, or it'll not be memory-friendly
The gameObjects have 2 types : active and inactive.
IN this case, FightUI is an inactive gameObject, and TextManager is active
An inactive gameObject doesn't intefere with the game.
Do I have to repeat myself ?
I don't think so, so far it's quite simple to grasp.
Unity is like "boxes in boxes" (again)
Project --> Scenes --> GameObjects --> Components
And here we go for the components
So, in the inspector, we can see the different components of a GameObject.
For example, Background contains 4 components : a RectTransform, 
a Canvas Renderer, a Background Loader and an Image.
These components can be active or not active too, as the switch tells us 
next to the component's name
(These logs modify the project, so I have to reload the project each time 
I start some tests)
So, as we can say, if I go to Background and I disable the Image component...
No more background :P
gasp, I have to say
And if I enable it again, the background returns!
There is lots and lots of Component types, and you can even create your own types! 
But we'll see that later :P
Lots and lots of types
I think this is the kind of thing that can be learned in depth
from a documentation as well (the functionality of these types that is)
Yeah
I'll just tell you the principal types, the ones that you'll always need to use
When you create a GameObject, this one always have at least one component : Transform
This component is here to tell Unity where the object is placed on the screen
Your created component is this little circle on the scene, between the enemies
As it is nothing for now, it does nothing and appears as a circle
Sometimes, when you create gameObjects in a given gameObject, this component changes : 
now we have a RectTransform, and the GameObject is now displayed as a rectangle on the 
screen
A rectTransform is an advances Transform where you can edit things such as the width 
and height of the gameObject. It replaces Transform.
Do you get it now ?
Yup.
Nice ^^ If you want to, we can go to the next chapter : II - Unity's behaviour (C# part)
let's do that then :)
Okay ^^
So, now we'll go on Visual Studio.
There is some special functions that are autotriggered in your class : here's some examples.

public class Handz { }

public class ANormalCClass {
    int yes;
    bool no;
    public string Something() {
        string myFirstSentence = "yay";
        return myFirstSentence;
    }
    public void DoesAThing() { }

    public Handz HANDZ() {
        return new Handz();
    }

    public void Start() {
        //This function will be autotriggered when the class with be instancied
    }

    public void Update() {
        //You already know this function, don't you ?
    }

    public void OnApplicationQuit() {
        //Another example, when we close the game
    }
}


You get it ?
These are from Unity I take.
Okay
So, now we'll REALLY need to create another file
*/

//LESSON 2 - Part 2
/*
Okay, so here we are
Can you make me a basis class named like the file's name please ?
I'll try :D and I'll still be jumping back and forth ;)
Yup
I guess :/
One error
I know that we're both obscessed by **HANDZ**, but not Fists
ah, typos, the usual :D
Nice
So, I'll need to add a lil' magic to do great things
We can see it now : I made a script dependancy
We took MonoBehaviour's class functions and we can now override the functions or create new functions (or both)
So MonoBehavious is an existing class in Unity, of which we take the features of, and the thing automatically uses everything in it already (kinda like a library)
Yup, exactly
And by making functions such as Start(), Update() and OnApplicationQuit(), we're overriding MonoBehaviour's functions
Now, we'll be able to do great things
Scripts can be used as components on gameObjects, so we'll give to a gameObject this class as a component!
Can you do it ?
I guess :D
Your turn! Add your script to this new GameObject!
So I appear to have difficulties clicking a simple button :D
xD
Try again :P
Good!
So now, everything that you do here will be repercuted on the game, while this gameObject is active.
Cool, so now we'll see how to load and unload classes via script
In Unity 5.2.0, you didn't need another library to load classes, but now you do : UnityEngine.SceneManagement. Add it to your class first!
Is it like the time when we put a class in a "variable" technically?
Nope
You have to add it like we added UnityEngine
So I'm not adding it to our class directly, ok
Nice
So, the function that we'll use to load a scene will be SceneManager.LoadScene(name); <- In what format do I do name? String... does it need string before it? Only in the function, as an argument
I'll need you to create an Update() function, and whenenver we press the "R" key, we load test.
Bonus : Input.GetKeyDown(KeyCode.R)
(you can jump if you want to)
I think I'll just fail a few times now :D
:thumbs_up:
do ' not work? or did I just THINK I had an error with those?
' are used for characters : only one character
" for strings, zero to infinite characters
ok, I was used to using 'string' from lua
Yeah
So, why don't we see the result now ?
So, let's go see the result of our code
You spam 'R', nothing happened
It works!
We got to test!
And as you could see, the scene queue was visible if you spam "R"
I don't know why it didn't work for me :/
Try again :P TV is capricious
Here you go
Bwahaha, interfering with regular behaviour bwahaha
Yeah, music and such doesn't stop if you only load the scene :P
Ok, so here is your final exercice
You'll have to modify this class, and create a GameObject with a "SpriteRenderer" Component.
In the Update function, you'll move this component in a circle pattern
This one is created to make you fail, don't worry :P
Just one more thing : you'll have to deal with the compiler this time for errors :P
I don't exactly know how to set things in the other component :/
If you don't know what you need to use, first access to the object in general, and then see in the functions/variables list
The Unity part is done, you just need to script now
yes, but I don't know with what variables to acces positions :/
First access to the SpriteRenderer component of CircleMove. I gave you some bonuses.
Do you want a headstart ?
I think I'll just do something stupid and let you shout at me :D
Okay ^^
Sometimes errors will not help you, just continue what you were doing
First, haven't you forget something important ?
well I think I should put it into a variable so to speak, but yeah, I kind of forgot how to go about that XD
This is a good think, but you forgot something even more important
Nope nope nope
Where is the Update() function ?
that will come when I know what to do :D
but I can put it in there, it just won't do didly squat yet
Wait, I understand now
So, don't you know how to initialize a variable ?
Well, this kind of variable, I think not :/
The same schema
Do I need to type it ?
I'm good with standard types, but wi didn't really go over the complex variables before
So, here's the schema
TYPE name = (new) value --> new for complex variables
so what is the TYPE here? :/
The final type will be SpriteRenderer
But you'll need to get a GameObject first
lemme try
very well
2s
new are onmy used when you CREATE the variable, not when you fetch it
You'll need one here
I see
also, I like to space out my stuff :D
Heehee, trapped :P
Bonuses : 
You can't access the newly created variable just like that
well that's getting a bit more complicated than I expected :D
I think you have un-porpusely mislead me :D but no problemo, I think I get it
Here you go, you now have your SpriteRenderer object
the problem was I didn't connect "go" with GameObject at first :D so I thought it was something completely different
I was going to type the first line too, but I didn't do that because I was lazy :P
So, now, how to access positions ? First, you better create your function update()
well, there it is, standing on its own and being sad :D
You better access sr first
jumping
sr. ... boxes in boxes!
OH
SORRY
I'm wrong :P
You need to access the RectTransform Component for the position
also, you wanted to tell me I suppose, that I can just get a list of variables by 'variable.' :P
Yup
So, do the same as you just did with a RectTransform component
Ya
though I still have no idea how this particular part works :p
Vector3 type objects are 3 floats : x, y, z
I don't really understand this argument layout { get; set; }
You don't need that, just do the same as we always do : 
TYPE name = (new) value --> new for complex variables creation
- GameObject.Find(string name) to get a gameObject in script
- GameObject go = new GameObject();
  go.GetComponent<ComponentType>() to get a component
  But you can go GameObject.Find(name).GetComponent<ComponentType>()
- Mathf.Cos, Mathf.Sin returns a float
I can show you two solutions : 
- You can access the SpriteRenderer component directly by making these two functions on the same line;
- Or you can create a Start() function where you can use go
Now you have the position, nice
What will you do with it ?
Well, I'd like to know how the values are kept in it before I can answer that question :D
'x' is a new variable that just takes the value of pos.x, and is not actually pos.x, right?
Yep, same for pos
In that case I can't do anything with these. I don't need to get x coordinate, I need to set it
That's what I was trying to get out of you :D
oh ok
One more thing then: What do we want to base this circular movement on? do we have a system time or something, or do I want to make a variable which I will update?
We have Time.time
Oh ok, normally you can't just modify X directly, my bad
That's a good start, but you'll see the problem
Well, this is just a very small circle, ok?
Better, heh
Look
We forgot to do something
I guess we didn't put rt back into RectTransform
Nope, look at your left
SO ? What is it ?
I don't follow I think
If the right bar is all green, you're good
Yellow = need to save, Red = error
I don't think I know what that error means :/
do I have to set that variable somehow within the Update() again?
Normally no
ok, so all these types of values are only accessible from within functions. If this isn't a simple value, go assign them in Start()
Very little circle, and not well positionned
You never stated where to put it ;)
BWAHAHAHAHA!
ZombiMionn :P
WEEEEEEEEEEEEEEEEEEEEEEE
Well, now you're ready! This lesson is over, I'm hungry af...so I'm gonna eat :P
*/

//LESSON 3
/*
Hey again
Not here yet ?
I thought we would mostly communicate in discord :D
Oh, we can do as you want ;P
but I'm here now
(At least all of this conversation will be here, if you need to find something quickly)
So, now we'll see more complex Unity mechanisms :P
We'll see "prefabs"
mkay
Okay, so these are CYKa's prefabs : these are stored GameObjects that can be used later,
with all their Components and variables.
For now it may not be useful, but they can be used for many things.
First, it'll be useful if you need to have lots of instances of this GameObject.
Example: LUAProjectile
Any question ?
none so far
Some of the prefabs here are just like "save" prefabs, like "Player", but others are used
everywhere, and can be created : this is the other use of a prefab : these can be 
instantiated.
Another Example : 
{LUAProjectile} Here, we can see that the prefab is registered in a variable, to be used later.
And...we can create a GameObject from this prefab with the command Instantiate(prefab), as shown in createPooledBullet().
It creates a new GameObject on the currnt Scene.
Do you understand ?
I think, yes.
Ok, can you sum-up prefabs' use cases ?
well, from what I understand, prefabs are gameObjects you define "outside of scripts" and can be used all around the place, and be replicated if needed.
Yup, moreless
{Here} So, now we'll create a prefab
Guess which GameObject I'll use :)
MIONNNNNNNNNNNNN?
Of course I will :D
One more info : blue gameObjects are GameObjects that have a prefab.
Here, Main Camera OW is already a prefab.
First of all, let's create the prefab
It's easy as hell
Done, ez pz 8)
the screen is jumping from time to time, so it's not too fluid.
Ok, I'll do it again
You saw it ?
All I saw that it was a regular GO and then it became a prefab out of the blue
- no pun intended
:c
Just click and drag the GameOBject to your folder
yup, that sounds easy enough :D
Well, now I'll delete MIONNNNNNNNNNNN from the scene and you'll need to instantiate it :D
(You can use BulletPool as a model)
ok, let's go
so, if I understnd correctly... are we doing this inside a function, or just out in the open? also, is this then put into a variable, right? what sort in this case?
The initialization must be done in the class, but the value must be given in a function.
You'll need : Resources.Load<GameObject>(sth);
I think you are going faster than me :D
Okay, I'll explain a bit
bPrefab = Resources.Load<LuaProjectile>("Prefabs/LUAProjectile");
Resources.Load loads a prefab from the Resources folder.
The given type between < and > is the type of the object you'll store : the gameObject will be created, but you can store a Component of this gameObject or the gameObject itself
in a variable.
So, telling Resources.Load<GameObject>(sth) will give the GameObject object of the prefab at "Resources/sth".
got it
Okay, good luck! Then, I'll tell you why it'll not work ^^
;P
what type is this again?
You wrote it
but the variable is a GO too?
Of course it'll be a GameObject too
Why can't it be a GameObject ?
I'm afraid I don'T understand the question :/
Why can't the variable be a "GameObject" type variable ?
That's a very good question :D
This was a rhetorical question :D Of course it can be a GameObject.
ah, so...
well, it's something now, that doesn't give me red lines :3
It'll work, but you'll see that it'll not work :3
woot!
You'll see ;P Let's launch the app
ERROR! No MIONNNNNNNN detected.
this is where instantiation comes in?
You instantiated MIONNNNNNNN
But
When is this function called ?
NEVAR!!!!
It is called...
by MIONNNN
And making this...
Will instantiate infinite MIONNNNNNNNNNNs
That doesn't sound bad in principle XD
x)
You're instantiating a GameOBject that contains an instantiation for a GameObject 
that contains an instantiation for a GameObject that contains an ins...
Sorry, but your instantiaton have to be done in another castle :)
But saaaad
Let's tell... 
{TransitionOverworld} Here 
But hey, rn it's bad, because the gameObject will be created, yeah, but it has no parents.
It'll be created like this :
You see ?
fun fact: Mionn doesn't have parents either
and yes, I see :D
But you need it to have "Canvas OW" as parent, because it is kept between scenes
and because it is how it works : layering
Rn, I don't think that Mionn will be displayed. LEt's check ig
Said nothing, we used a Sprite Renderer x)
If we have used an Image, it'd not be displayed.
So, we'll need to set MIONNNNNNN's GameObject parent
To do this, we'll need to go through MIONNNNNNNN's Transform component.
I'll give you an example, you'll have to recreate it
Okay, so LET'S SEE WHAT'S GOING ON
ERRORZ
Why would have been errors, in your opinion ?
well, it says something about refferencing, but I'm still not familiar with how thi
instancing works, so for now I don't have a decisive idea
Do you see now ?
?
Where is MIONNNNNNNNNN ?
well, a clone of it, as I see is on the Canvas, the original is nonexistent
Yup, so you have 2 choices : 
- Either you set the name of the GameObject,
- Either, as this script is already in MIONNNNNNNN, you can call it directly using gameObject.
Oh, we'll do both :D
How do you think we can rename a GameObject ?
I'd guess with a function maybe
Why using a function when you have a settable variable ?
You mean we just put it into another variable?
No, we'll just set another value to the GameObject's name
Well, I didn't know you had that :D it's something like GameObject.Name = sg?
Moreless
no caps and you're right :)
Tell me what's wrong
a sec
so I want to set Canvas OW as a parent, right?
Yup, and I already told you how to "Find" a GameObject.
ah; that was a while ago :D lemme look that up in my awesome class
so I'm parenting it to a component of "Canvas OW" If I understand it correctly?
Actually, GameObjects aren't parented : only Transform are parented.
makes sense - a sec, the dog wants something really bad
Okay
here I am in the meanwhile
^^
So, now we'll test this stuff
WE FORGOT SOMETHING
I'll let you do it :)
um, what did we forget again?
sooo..?
Now, the other way : accessing the gameObject directly
Try to access it!
Would I do it with 'gl'?
Here, in this script
We haven't instantiated it here, no?
I'll tell you something.
Every script that inherits from MonoBehaviour is used as a Component.
Every Component has a GameObject.
Now, find this GameObject!
Tell me
I don't think I get it yet :/
huh... so...
there I think
Yup, caps are useful here
As this script is a Component, it always has a GameObject : you can access it via "gameObject" directly.
sounds useful
It is :D
Now, even if you don't rename the GameObject, it is recognized.
question: What does .Find() do when there are several GOs with the same name?
It takes the higher one in the hierarchy.
Are you thinking about LUAProjectiles ?
Yes, mainly.
Huh :D
To access them, you know that they all are BulletPool's child.
So, you'll have to use this line : 
Okay ? As all BulletPool's child have "RectTransform" components, let's take all RectTransforms.
And I assume the "[]" makes it a table or something of the sort?
You need to store all these GameObjects in an array. Then, access them as you access arrays' content
Just asking because we still haven't covered array type of variables :P
:P
Do you want to stop here, or shall we continue ?
I think it's about time to learn about arrays and indexing :P
Okay, let's get to the point!
...but first, ~~let me take a selfie~~ I have to drink
Back
h0i
I heard this music too much lately
Okay, ready
So, what have I teached you today ?
Making and instancing prefabs from existing GO's, and a little about arrays
also, accessing a GO from its component directly
Don't worry, Dictionaries are simple :D
Well, arrays. They are initialized like normal variables, with one little exception : these are complex variables.
If you remember about the last time, how do we initialize complex variables ?
with 'new' before the value as far as I know.
Exact! Here is how you create an array :
There is 3 ways to do so : empty arrays (1), defined arrays (2) and instantied arrays (3)
You can't expand arrays, if you instancied a 5-long int array, you can't add a 6th int.
Understood ?
Yes.
Array's indexes begins at 0.
So, for numbers2, you can index 0, 1, 2, 3, 4 but not 5 :P
One quick question: how long will (1) be?
...
1 is 0-int long :D
very useful :P
Exactly x)
To access the elements of an array, it's like Lua : int a = numbers2[0] --> a = 3
Same to set it. That's moreless what you need to know about arrays.
ONe more thing : you'll often need Array.Length.
Is it clear ?
Clear, though seems a bit silly to have to index with length-1 after using LUA :D
Heehee, Lua is easy : all languages start with 0
I knew most of them do, but it is still kind of silly, even if understandable.
Okay, so now we'll see more complex "Array-like" types.
To use the next types, you'll have to include System.Collections.Generic.
First : List<type>
You can use it LIKE AN ARRAY
Well, Lists are modifiable arrays : nothing much
I prefer using Lists than array for their modifiability
After this new line, strings3[3] = "yay²"
Of course you can remove data and such, you can do what you want.
Get it ?
So again, a simple question: 'strings' is 0 in length, but can later be any length; 'strings4' is 3 long, but has no elements?
Exact : if you try to get these elements, it'll return null. (This type of List initialization is never used, but it is an example to show you that Lists are improved Arrays)
New Question: how would you fill a List with the contents of an Array?
Heehee
You can't do List<int> ints = numbers2;
So you have to enumerate all numbers2 elements in a for and add them one by one in the List
Like this
Got it ; I'll ask about the syntax of for  cycles later :D
Okay ^^
Finally : Dictionaries : you bind one value to another
You can only initialize it like this.
And add values like this. Of course, you can remove values, clear the dictionary and such.
But I assume you can't define the dictionary at initializing.
It gets much harder to do so
Embedded arrays : our next lesson
I used embedded tables before in Lua, I guess they work similarly.
Yup
Well, we have two ways to initialize arrays x)
int[][] and int[,]
Capiche.
This is an array with two arrays that are 2 in length?
Yup
Ok, so that's for nested arrays.
We'll try something.
XD So funneh
Insanity.
Like that one time I decided to have matricies of user chosen 'n' and 'm' size filled with numbers from 'l' to 'k' randomly. In about 3 or so layers :D
XD; Hey :D
Well, do you want to make an exercice about this ?
Ann then we're done ?
I think we could learn about for cycles, and then I will recreate my famous matrix in matrix formula (or I think only List in List this time for simplicity)
Okay :D
We'll make a "sprite analyser" :)
So, the goal will be to have a given double array of ints, and you'll need to print every value :D
You forgot the part about explaining 'for' syntax :P
Oh, well...
for (initialization; condition; actionEachTurn) { }
so a quick try; I will want to go from 0 to 4 in steps of 1:
Are you sure ?
I am not, but this is what I think of what you wrote
Like this ?
wait, I was just trying to do this: "I will want to go from 0 to 4 in steps of 1" nothing more
Oh ok, but yeah that's it
so that should do what I wanted to do? <- ?
Yup
Do what you want to do : i'll be here you repair the broken pots
Here you go
Imma make the second layer
So I can't get the length of a certain layer?  
Hmmm... I saw that once, lemme check
myArray.GetLength(0)  -> Gets first dimension size
myArray.GetLength(1)  -> Gets second dimension size
question #2: can the second layer only be of the same length for all elements then?
Well, good arrays must have the same length at all dimensions, but I don't know for this one :
I think, since you can do [#1,#2] the dimensions MUST have the same length, otherwise this deifinition wouldn't really work.
Yup
Now you know :D
Then it's understandable that I can't ask for a sub-array's length separately :D
Don't mind the blue link
We need to press "L" in the game to activate SpriteAnalyser
It works! Kinda
Should it be length-1? <- ? :)
Yup, but look at the code.
I'm looking.
So, you say, for nested arrays, you can't use .Length for the first layer, or that you shouldn't?
.Length counts all ints in the array
So not the number of arrays inside, got it.
for (int i = 0; i < ints.GetLength(0); i++) { --> i goes 0, 1, 2...and that's it
Got it ?
So condition is checked before i++ and the contents?
Ofc
not necessarily trivial ;)
If the condition isn't respected, it won't come in the loop. Plus, as I said earlier...
for (initialization; condition; actionEachTurnEnd) { }
It is possible to imagine a loop that would go "start, run, check, add"
???
oh, yeah
You have do...while
ok, so it should work now I think
Well, very buggy
And why is Unity crying about that ?
Now it's good
Okay, so I think that it's enough for us today
I'll show you one last thing, and then we'll stop
mkay.        
Enums : it's like arrays, but you HAVE to give the values. These are arbitrary, as long as it is a possible string.
That's it ^^
Any question ?
So these are like the bullet variables? That can go Actions.eR5f = sg?
Yup, you can do that, but sg = integer
It's automatically 0, 1, 2...but you can set a value if needed.
Plus...
{UIController} Yep, this is an enum.
Was there a question?
?
It looked like you are waiting for some response.
'So these are like the bullet variables?'
Oh, just understood
Yup, like bullet variables, but only ints
/Got it!
*/

using UnityEngine;
//using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MyFirstComponentClass : MonoBehaviour {

    //SpriteRenderer sr = GameObject.Find("CircleMove").GetComponent<SpriteRenderer>();
    RectTransform rt;
    /*int[,] megaInt = new int[2, 2] { { 1, 2 }, { 3, 4 } }; int[] numbers = new int[] { }, numbers2 = new int[] { 3, 2, 8, 4, 9 }, numbers3 = new int[5];
    List<string> strings = new List<string>(), strings2 = new List<string> { }, strings3 = new List<string> { "yay", "woohoo", "gial" }, strings4 = new List<string>(3);
    RectTransform[] rts = GameObject.Find("BulletPool").GetComponentsInChildren<RectTransform>(); Dictionary<string, int> dictStringToInt = new Dictionary<string, int>();
    List<List<string>> megaList = new List<List<string>>(); Dictionary<Dictionary<string, bool>, Dictionary<int, float>> megaDict = new Dictionary<Dictionary<string, bool>, Dictionary<int, float>>();
    public enum Actions { eR5f = 99, qsdfghjklm = -4, weeeeeeeeeeed, r2d2 }*/

    public static void SpriteAnalyser() {
        int[,] ints = new int[,] { { 1, 5, 3, 8, 2 }, { 3, 78, -3, 0, 10 }, { 3, 7, 9, 3456, -314 } };
        for (int i = 0; i < ints.GetLength(0); i++)
            for (int j = 0; j < ints.GetLength(1); j++)
                Debug.Log(ints[i, j]);
    }

    public void Start() {
        //dictStringToInt.Add("yay", 1);
        //rt = GameObject.Find("MIONNNNNNNNNNNN").GetComponent<RectTransform>();
        rt = gameObject.GetComponent<RectTransform>();
        //int a = numbers2.Length; //a = 5
        //strings3.Add("yay²");
        /*List<int> ints = new List<int>();
        int k = 0;
        /do {
            k++;
            //code
        } while (k < numbers2.Length);

        for (int i = 0; i < numbers2.Length; i++) {
            ints.Add(numbers2[i]);
        }*/
    }

    public void Update() {
        Vector3 pos = rt.position;
        pos.x = Mathf.Cos(Time.time * 20) * 240 + 320;
        pos.y = Mathf.Sin(Time.time * 20) * 240 + 240;
        rt.position = pos;
    }

    /*public void Update() {
        // bool == true and bool are the same, bool == false and !bool are the same
        // Now I'll jump around, because I don't remember the syntax exactly for cases.
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene("test");
    }*/
}

/*TransitionOverworld : 

GameObject goodluck = Resources.Load<GameObject>("Prefabs/MIONNNNNNNNNNNN");
GameObject gl = Instantiate(goodluck);
gl.name = "MIONNNNNNNNNNNN";
gl.transform.SetParent(GameObject.Find("Canvas OW").transform);
*/
