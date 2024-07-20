using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoonSharp.Interpreter;

/// <summary>
/// Static placeholder inventory class for the player. Will probably get moved to something else that makes sense, like the player...or not.
/// </summary>
public static class Inventory {
    public static List<string> addedItems = new List<string>();
    public static List<int> addedItemsTypes = new List<int>();
    public static LuaInventory luaInventory;
    public static int inventorySize = 8;
    public static int itemStatAmount;
    public static Dictionary<string, string> NametoDesc = new Dictionary<string, string>(), NametoShortName = new Dictionary<string, string>();
    public static Dictionary<string, int> NametoType = new Dictionary<string, int>(), NametoPrice = new Dictionary<string, int>();
    public static bool usedItemNoDelete;
    //public static bool overworld = false;
    public static List<UnderItem> inventory = new List<UnderItem>();

    public static void SetItemList(string[] items) {
        foreach (string item in items) {
            // Make sure that the item exists before trying to create it
            string outString;
            int outInt;
            if (!addedItems.Contains(item) && !NametoDesc.TryGetValue(item, out outString) && !NametoShortName.TryGetValue(item, out outString) && !NametoType.TryGetValue(item, out outInt) && !NametoPrice.TryGetValue(item, out outInt))
                throw new CYFException("Inventory.SetInventory: The item \"" + item + "\" was not found." + (UnitaleUtil.IsOverworld ? "" : "\n\nAre you sure you called Inventory.AddCustomItems first?"));
        }

        inventory = new List<UnderItem>(new UnderItem[] { });
        for (int i = 0; i < items.Length; i++) {
            if (i == inventorySize) {
                UnitaleUtil.Warn("The inventory can only contain " + inventorySize + " items, yet you tried to add the item \"" + items[i] + "\" as item number " + (i + 1) + ".");
                break;
            }
            inventory.Add(new UnderItem(items[i]));
        }
    }

    public static void SetItem(int index, string Name) {
        if (index >= inventorySize)    throw new CYFException("The inventory can only contain " + inventorySize + " items.");
        if (index >= inventory.Count)  AddItem(Name);
        else                           inventory[index] = new UnderItem(Name);
    }

    public static bool AddItem(string Name) {
        if (inventory.Count == inventorySize)
            return false;
        // Make sure that the item exists before trying to create it
        string outString;
        int outInt;
        if (!addedItems.Contains(Name) && !NametoDesc.TryGetValue(Name, out outString) && !NametoShortName.TryGetValue(Name, out outString) &&
            !NametoType.TryGetValue(Name, out outInt) && !NametoPrice.TryGetValue(Name, out outInt))
            throw new CYFException("Inventory.AddItem: The item \"" + Name + "\" was not found." + (UnitaleUtil.IsOverworld ? "" : "\n\nAre you sure you called Inventory.AddCustomItems first?"));
        inventory.Add(new UnderItem(Name));
        return true;
    }

    public static int GetItemType(string Name) {
        int type;
        if (addedItems.Contains(Name))
            for (int i = addedItems.Count - 1; i >= 0; i--)
                if (addedItems[i] == Name)
                    return addedItemsTypes[i];
        NametoType.TryGetValue(Name, out type);
        return type;
    }

    public static bool TryCall(string func, DynValue[] param = null) {
        return !UnitaleUtil.IsOverworld && UnitaleUtil.TryCall(EnemyEncounter.script, func, param);
    }

    public static void UseItem(int ID) {
        usedItemNoDelete = false;
        itemStatAmount = 0;
        string Name = inventory[ID].Name, replacement = "";
        int type = inventory[ID].Type;
        float amount = -991;
        TryCall("HandleItem", new[] { DynValue.NewString(Name.ToUpper()), DynValue.NewNumber(ID + 1) });

        // Check if the current item has been added to the list of custom items
        bool foundCustomItem = false;
        if (addedItems.Count != 0 && !UnitaleUtil.IsOverworld)
            if (addedItems.Any(t => string.Equals(t, Name, StringComparison.CurrentCultureIgnoreCase)))
                foundCustomItem = true;

        TextMessage[] mess = null;
        // Check the standard CYF item dictionary
        if (!foundCustomItem) ItemLibrary(Name, type, out mess, out amount, out replacement);

        if (type == 1 || type == 2) {
            // Equip the item if it's a weapon or armor
            if ((int)amount != -991)
                itemStatAmount = (int)amount;
            mess = ChangeEquipment(ID);
        } else if (replacement != "") {
            // Replace the current item with another if it has a replacement item
            inventory.RemoveAt(ID);
            inventory.Insert(ID, new UnderItem(replacement));
        } else if (!usedItemNoDelete && type == 0)
            // Delete the item if it's a standard consumable
            inventory.RemoveAt(ID);

        if (!UnitaleUtil.IsOverworld) {
            if (!UIController.instance.battleDialogueStarted && mess != null)
                UIController.instance.ActionDialogResult(mess);
        } else if (mess != null) {
            GameObject.Find("TextManager OW").GetComponent<TextManager>().SetTextQueue(mess);
            GameObject.Find("TextManager OW").transform.parent.parent.SetAsLastSibling();
        }
    }

    public static void AddItemsToDictionaries() {
        NametoDesc.Add("Testing Dog", "A dog that tests something.\rDon't ask me what, I don't know.");        NametoShortName.Add("Testing Dog", "TestDog");
        NametoType.Add("Testing Dog", 3);                                                                      NametoPrice.Add("Testing Dog", 0);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        NametoDesc.Add("Bandage", "It has already been used\rseveral times.");
        NametoPrice.Add("Bandage", 5);

        NametoDesc.Add("Monster Candy", "Has a distinct, non-licorice\rflavor.");                              NametoShortName.Add("Monster Candy", "MnstrCndy");
        NametoPrice.Add("Monster Candy", 5);

        NametoDesc.Add("Spider Donut", "A donut made with Spider Cider\rin the batter.");                      NametoShortName.Add("Spider Donut", "SpidrDont");
        NametoPrice.Add("Spider Donut", 7);

        NametoDesc.Add("Spider Cider", "Made with whole spiders, not just\rthe juice.");                       NametoShortName.Add("Spider Cider", "SpidrCidr");
        NametoPrice.Add("Spider Cider", 18);

        NametoDesc.Add("Butterscotch Pie", "Butterscotch-cinnamon pie,\rone slice.");                          NametoShortName.Add("Butterscotch Pie", "ButtsPie");
        NametoPrice.Add("Butterscotch Pie", 900);

        NametoDesc.Add("Snail Pie", "Heals Some HP. An acquired taste.");
        NametoPrice.Add("Snail Pie", 899);

        NametoDesc.Add("Snowman Piece", "Please take this to the ends\rof the earth.");                        NametoShortName.Add("Snowman Piece", "SnowPiece");
        NametoPrice.Add("Snowman Piece", 300);

        NametoDesc.Add("Nice Cream", "Instead of a joke, the wrapper\rsays something nice.");                  NametoShortName.Add("Nice Cream", "NiceCream");
        NametoPrice.Add("Nice Cream", 15);

        NametoDesc.Add("Bisicle", "It's a two-pronged popsicle,\rso you can eat it twice.");
        NametoPrice.Add("Bisicle", 15);

        NametoDesc.Add("Unisicle", "It's a SINGLE-pronged popsicle.\rWait, that's just normal...");
        NametoPrice.Add("Unisicle", 8);

        NametoDesc.Add("Cinnamon Bunny", "A cinnamon roll in the shape\rof a bunny.");                         NametoShortName.Add("Cinnamon Bunny", "CinnaBun");
        NametoPrice.Add("Cinnamon Bunny", 25);

        NametoDesc.Add("Astronaut Food", "For feeding a pet astronaut.");                                      NametoShortName.Add("Astronaut Food", "AstroFood");
        NametoPrice.Add("Astronaut Food", 25);

        NametoDesc.Add("Crab Apple", "An aquatic fruit that resembles\ra crustacean.");                        NametoShortName.Add("Crab Apple", "CrabApple");
        NametoPrice.Add("Crab Apple", 25);

        NametoDesc.Add("Sea Tea", "Made from glowing marsh water.\rIncreases SPEED for one battle.");
        NametoPrice.Add("Sea Tea", 18);

        NametoDesc.Add("Abandoned Quiche", "A psychologically damaged\rspinach egg pie.");                     NametoShortName.Add("Abandoned Quiche", "Ab Quiche");
        NametoPrice.Add("Abandoned Quiche", 200);

        NametoDesc.Add("Temmie Flakes", "It's just torn up pieces of\rconstruction paper.");                   NametoShortName.Add("Temmie Flakes", "TemFlakes");
        NametoPrice.Add("Temmie Flakes", 6);

        NametoDesc.Add("Dog Salad", "Recovers HP (Hit Poodles)");
        NametoPrice.Add("Dog Salad", 10);

        NametoDesc.Add("Instant Noodles", "Comes with everything you need\rfor a quick meal!");                NametoShortName.Add("Instant Noodles", "InstaNood");
        NametoPrice.Add("Instant Noodles", 30);

        NametoDesc.Add("Hot Dog...?", "The \"meat\" is made of something\rcalled a \"water sausage.\"");       NametoShortName.Add("Hot Dog...?", "Hot Dog");
        NametoPrice.Add("Hot Dog...?", 30);

        NametoDesc.Add("Hot Cat", "Like a hot dog, but with\rlittle cat ears on the end.");
        NametoPrice.Add("Hot Cat", 30);

        NametoDesc.Add("Junk Food", "Food that was probably once\rthrown away.");
        NametoPrice.Add("Junk Food", 25);

        NametoDesc.Add("Hush Puppy", "This wonderful spell will stop\ra dog from casting magic.");             NametoShortName.Add("Hush Puppy", "HushPupe");
        NametoPrice.Add("Hush Puppy", 600);

        NametoDesc.Add("Starfait", "A sweet treat made of sparkling stars.");
        NametoPrice.Add("Starfait", 60);

        NametoDesc.Add("Glamburger", "A hamburger made of edible\rglitter and sequins.");                      NametoShortName.Add("Glamburger", "GlamBurg");
        NametoPrice.Add("Glamburger", 120);

        NametoDesc.Add("Legendary Hero", "Sandwich shaped like a sword.\rIncreases ATTACK when eaten.");       NametoShortName.Add("Legendary Hero", "Leg.Hero");
        NametoPrice.Add("Legendary Hero", 300);

        NametoDesc.Add("Steak in the Shape of Mettaton's Face", "Huge steak in the shape of\rMettaton's face.You don't feel\rlike it's made of real meat...");
        NametoShortName.Add("Steak in the Shape of Mettaton's Face", "FaceSteak");                             NametoPrice.Add("Steak in the Shape of Mettaton's Face", 500);

        NametoDesc.Add("Popato Chisps", "Regular old popato chisps.");                                         NametoShortName.Add("Popato Chisps", "PT Chisps");
        NametoPrice.Add("Popato Chisps", 25);

        NametoDesc.Add("Bad Memory", "?????");                                                                 NametoShortName.Add("Bad Memory", "BadMemory");
        NametoPrice.Add("Bad Memory", 10);

        NametoDesc.Add("Last Dream", "The goal of \"Determination\".");                                        NametoShortName.Add("Last Dream", "LastDream");
        NametoPrice.Add("Last Dream", 25);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        NametoDesc.Add("Stick", "Its bark is worse than\rits bite.");
        NametoType.Add("Stick", 3);                                                                            NametoPrice.Add("Stick", 50);

        NametoDesc.Add("Toy Knife", "Made of plastic. A rarity\rnowadays.");
        NametoType.Add("Toy Knife", 1);                                                                        NametoPrice.Add("Toy Knife", 30);

        NametoDesc.Add("Tough Glove", "A worn pink leather glove.\rFor five-fingered folk.");                  NametoShortName.Add("Tough Glove", "TuffGlove");
        NametoType.Add("Tough Glove", 1);                                                                      NametoPrice.Add("Tough Glove", 50);

        NametoDesc.Add("Ballet Shoes", "These used shoes make you\rfeel incredibly dangerous.");               NametoShortName.Add("Ballet Shoes", "BallShoes");
        NametoType.Add("Ballet Shoes", 1);                                                                     NametoPrice.Add("Ballet Shoes", 100);

        NametoDesc.Add("Torn Notebook", "Contains illegible scrawls.\rIncreases INV by 6.");                   NametoShortName.Add("Torn Notebook", "TornNotbo");
        NametoType.Add("Torn Notebook", 1);                                                                    NametoPrice.Add("Torn Notebook", 55);

        NametoDesc.Add("Burnt Pan", "Damage is rather consistent.\rConsumable items heal four more HP.");
        NametoType.Add("Burnt Pan", 1);                                                                        NametoPrice.Add("Burnt Pan", 200);

        NametoDesc.Add("Empty Gun", "An antique revolver. It has no\rammo. Must be used precisely,\ror damage will be low.");
        NametoType.Add("Empty Gun", 1);                                                                        NametoPrice.Add("Empty Gun", 350);

        NametoDesc.Add("Worn Dagger", "Perfect for cutting plants\rand vines.");                               NametoShortName.Add("Worn Dagger", "WornDG");
        NametoType.Add("Worn Dagger", 1);                                                                      NametoPrice.Add("Worn Dagger", 500);

        NametoDesc.Add("Real Knife", "Here we are!");                                                          NametoShortName.Add("Real Knife", "RealKnife");
        NametoType.Add("Real Knife", 1);                                                                       NametoPrice.Add("Real Knife", 99999);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        NametoDesc.Add("Faded Ribbon", "If you're cuter, monsters\rwon't hit you as hard.");                   NametoShortName.Add("Faded Ribbon", "Ribbon");
        NametoType.Add("Faded Ribbon", 2);                                                                     NametoPrice.Add("Faded Ribbon", 30);

        NametoDesc.Add("Manly Bandanna", "It has seen some wear.\rIt has abs drawn on it.");                   NametoShortName.Add("Manly Bandanna", "Bandanna");
        NametoType.Add("Manly Bandanna", 2);                                                                   NametoPrice.Add("Manly Bandanna", 50);

        NametoDesc.Add("Old Tutu", "Finally, a protective piece\rof armor.");
        NametoType.Add("Old Tutu", 2);                                                                         NametoPrice.Add("Old Tutu", 100);

        NametoDesc.Add("Cloudy Glasses", "Glasses marred with wear.\rIncreases INV by 9.");                    NametoShortName.Add("Cloudy Glasses", "ClodGlass");
        NametoType.Add("Cloudy Glasses", 2);                                                                   NametoPrice.Add("Cloudy Glasses", 35);

        NametoDesc.Add("Temmie Armor", "The things you can do with a\rcollege education! Raises ATTACK when\rworn. Recovers HP every other\rturn. INV up slightly.");
        NametoShortName.Add("Temmie Armor", "Temmie AR");        NametoType.Add("Temmie Armor", 2);            NametoPrice.Add("Temmie Armor", 9999);

        NametoDesc.Add("Stained Apron", "Heals 1 HP every other turn.");                                       NametoShortName.Add("Stained Apron", "StainApro");
        NametoType.Add("Stained Apron", 2);                                                                    NametoPrice.Add("Stained Apron", 200);

        NametoDesc.Add("Cowboy Hat", "This battle-worn hat makes you\rwant to grow a beard. It also\rraises ATTACK by 5.");
        NametoShortName.Add("Cowboy Hat", "CowboyHat");          NametoType.Add("Cowboy Hat", 2);              NametoPrice.Add("Cowboy Hat", 350);

        NametoDesc.Add("Heart Locket", "It says \"Best Friends Forever.\"");                                   NametoShortName.Add("Heart Locket", "<--Locket");
        NametoType.Add("Heart Locket", 2);                                                                     NametoPrice.Add("Heart Locket", 500);

        NametoDesc.Add("The Locket", "You can feel it beating.");                                              NametoShortName.Add("The Locket", "TheLocket");
        NametoType.Add("The Locket", 2);                                                                       NametoPrice.Add("The Locket", 99999);
    }

    public static void UpdateEquipBonuses() {
        TextMessage[] mess; float amount; string replacement;
        ItemLibrary(PlayerCharacter.instance.Weapon, 1, out mess, out amount, out replacement);
        PlayerCharacter.instance.WeaponATK = (int)amount;
        ItemLibrary(PlayerCharacter.instance.Armor, 2, out mess, out amount, out replacement);
        PlayerCharacter.instance.ArmorDEF = (int)amount;
    }

    private static string HPRecoverString(float HP) {
        if (HP < 0)                                                             return "[w:10]\nYou lost " + -HP + " HP.";
        if (PlayerCharacter.instance.HP + HP >= PlayerCharacter.instance.MaxHP) return "[w:10]\nYour HP was maxed out.";
        return "[w:10]\nYou recovered " + HP + " HP!";
    }

    public static void ItemLibrary(string name, int type, out TextMessage[] mess, out float amount, out string replacement) {
        mess = null; amount = 0; replacement = "";
        switch (type) {
            case 0:
                switch (name) {
                    case "Bandage":
                        amount = 10;
                        mess = new[] { new TextMessage("You re-applied the bandage.[w:10]\nStill kind of gooey." + HPRecoverString(amount), true, false) };
                        break;
                    case "Monster Candy":
                        amount = 10;
                        mess = new[] { new TextMessage("You ate the Monster Candy.[w:10]\nVery un-licorice-like." + HPRecoverString(amount), true, false) };
                        break;
                    case "Spider Donut":
                        amount = 12;
                        mess = new[] { new TextMessage("You ate the Spider Donut.[w:10]\nDon't worry, [w:5]spider didn't." + HPRecoverString(amount), true, false) };
                        break;
                    case "Spider Cider":
                        amount = 24;
                        mess = new[] { new TextMessage("You drank the Spider Cider." + HPRecoverString(amount), true, false) };
                        break;
                    case "Butterscotch Pie":
                        amount = 999;
                        mess = new[] { new TextMessage("You ate the Butterscotch Pie." + HPRecoverString(amount), true, false) };
                        break;
                    case "Snail Pie":
                        amount = PlayerCharacter.instance.MaxHP - (int)PlayerCharacter.instance.HP - 1;
                        mess = new[] { new TextMessage("You ate the Snail Pie.[w:10]\nYour HP was maxed out.", true, false) };
                        break;
                    case "Snowman Piece":
                        amount = 45;
                        mess = new[] { new TextMessage("You ate the Snowman Piece." + HPRecoverString(amount), true, false) };
                        break;
                    case "Nice Cream":
                        amount = 15;
                        int randomCream = Math.RandomRange(0, 8);
                        string sentenceCream = "You ate the Nice Cream.[w:10]\n";
                        switch (randomCream) {
                            case 0: sentenceCream += "You're super spiffy!"; break;
                            case 1: sentenceCream += "Are those claws natural?"; break;
                            case 2: sentenceCream += "Love yourself! I love you!"; break;
                            case 3: sentenceCream += "You look nice today!"; break;
                            case 4: sentenceCream += "(An illustration of a hug)"; break;
                            case 5: sentenceCream += "Have a wonderful day!"; break;
                            case 6: sentenceCream += "Is this as sweet as you?"; break;
                            case 7: sentenceCream += "You're just great!"; break;
                        }
                        mess = new[] { new TextMessage(sentenceCream + HPRecoverString(amount), true, false) };
                        break;
                    case "Bisicle":
                        amount = 11;
                        replacement = "Unisicle";
                        mess = new[] { new TextMessage("You ate one half of the\rBisicle." + HPRecoverString(amount), true, false) };
                        break;
                    case "Unisicle":
                        amount = 11;
                        mess = new[] { new TextMessage("You ate the Unisicle." + HPRecoverString(amount), true, false) };
                        break;
                    case "Cinnamon Bunny":
                        amount = 22;
                        mess = new[] { new TextMessage("You ate the Cinnabon Bun." + HPRecoverString(amount), true, false) };
                        break;
                    case "Astronaut Food":
                        amount = 21;
                        mess = new[] { new TextMessage("You ate the Astronaut Food." + HPRecoverString(amount), true, false) };
                        break;
                    case "Crab Apple":
                        amount = 18;
                        mess = new[] { new TextMessage("You ate the Crab Apple." + HPRecoverString(amount), true, false) };
                        break;
                    case "Sea Tea":
                        amount = 18;
                        mess = new[] { new TextMessage("[sound:SeaTea]You drank the Sea Tea." + HPRecoverString(amount), true, false) };
                        break;
                    case "Abandoned Quiche":
                        amount = 34;
                        mess = new[] { new TextMessage("You ate the quiche." + HPRecoverString(amount), true, false) };
                        break;
                    case "Temmie Flakes":
                        amount = 2;
                        mess = new[] { new TextMessage("You ate the Temmie Flakes." + HPRecoverString(amount), true, false) };
                        break;
                    case "Dog Salad":
                        int randomSalad = Math.RandomRange(0, 4);
                        string sentenceSalad;
                        switch (randomSalad) {
                            case 0:
                                amount = 2;
                                sentenceSalad = "Oh. These are bones...";
                                break;
                            case 1:
                                amount = 10;
                                sentenceSalad = "Oh. Fried tennis ball...";
                                break;
                            case 2:
                                amount = 30;
                                sentenceSalad = "Oh. Tastes yappy...";
                                break;
                            default:
                                amount = 999;
                                sentenceSalad = "It's literally garbage???";
                                break;
                        }
                        mess = new[] { new TextMessage(sentenceSalad + HPRecoverString(amount), true, false) };
                        break;
                    case "Instant Noodles":
                        amount = GlobalControls.isInFight ? 4 : 15;
                        mess = new[] { new TextMessage("You remove the Instant\rNoodles from their\rpackaging.", true, false),
                                       new TextMessage("You put some water in\rthe pot and place it\ron the heat.", true, false),
                                       new TextMessage("You wait for the water\rto boil...", true, false),
                                       new TextMessage("[noskip][music:pause]...[w:30]\n...[w:30]\n...", true, false),
                                       new TextMessage("[noskip]It's[w:30] boiling.", true, false),
                                       new TextMessage("[noskip]You place the noodles[w:30]\rinto the pot.", true, false),
                                       new TextMessage("[noskip]4[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                       new TextMessage("[noskip]3[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                       new TextMessage("[noskip]2[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                       new TextMessage("[noskip]1[w:30] minute left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                       new TextMessage("[noskip]The noodles[w:30] are finished.", true, false),
                                       new TextMessage("...they don't taste very\rgood.", true, false),
                                       new TextMessage("You add the flavor packet.", true, false),
                                       new TextMessage("That's better.", true, false),
                                       new TextMessage("Not great,[w:5] but better.", true, false),
                                       new TextMessage("[music:unpause][health:" + amount + "]You ate the Instant Noodles." + HPRecoverString(amount), true, false) };
                        amount = 0;
                        break;
                    case "Hot Dog...?":
                        amount = 20;
                        mess = new[] { new TextMessage("[sound:HotDog]You ate the Hot Dog." + HPRecoverString(amount), true, false) };
                        break;
                    case "Hot Cat":
                        amount = 21;
                        mess = new[] { new TextMessage("[sound:HotCat]You ate the Hot Cat." + HPRecoverString(amount), true, false) };
                        break;
                    case "Junk Food":
                        amount = 17;
                        mess = new[] { new TextMessage("You ate the Junk Food." + HPRecoverString(amount), true, false) };
                        break;
                    case "Hush Puppy":
                        amount = 65;
                        mess = new[] { new TextMessage("You ate the Hush Puppy.[w:10]\nDog-magic is neutralized." + HPRecoverString(amount), true, false) };
                        break;
                    case "Starfait":
                        amount = 14;
                        mess = new[] { new TextMessage("You ate the Starfait." + HPRecoverString(amount), true, false) };
                        break;
                    case "Glamburger":
                        amount = 27;
                        mess = new[] { new TextMessage("You ate the Glamburger." + HPRecoverString(amount), true, false) }; break;
                    case "Legendary Hero":
                        amount = 40;
                        mess = new[] { new TextMessage("[sound:LegHero]You ate the Legendary Hero." + HPRecoverString(amount), true, false) };
                        break;
                    case "Steak in the Shape of Mettaton's Face":
                        amount = 60;
                        mess = new[] { new TextMessage("You ate the Face Steak." + HPRecoverString(amount), true, false) };
                        break;
                    case "Popato Chisps":
                        amount = 13;
                        mess = new[] { new TextMessage("You ate the Popato Chisps." + HPRecoverString(amount), true, false) };
                        break;
                    case "Bad Memory":
                        amount = PlayerCharacter.instance.HP <= 3 ? 999 : -1;
                        mess = new[] { new TextMessage("You consume the Bad Memory." + HPRecoverString(amount), true, false) };
                        break;
                    case "Last Dream":
                        amount = 17;
                        mess = new[] { new TextMessage("Through DETERMINATION, the dream\rbecame true." + HPRecoverString(amount), true, false) };
                        break;
                    default:
                        UnitaleUtil.Warn("The item " + name + " doesn't exist in CYF's consumable item pool.");
                        break;
                }
                if (amount != 0)
                    if (UnitaleUtil.IsOverworld) EventManager.instance.luaPlayerOw.setHP(PlayerCharacter.instance.HP + amount);
                    else                         PlayerController.instance.Hurt(-amount, 0);
                break;
            case 1:
                switch (name) {
                    case "Toy Knife":     amount = 3;  break;
                    case "Tough Glove":   amount = 5;  break;
                    case "Ballet Shoes":  amount = 7;  break;
                    case "Torn Notebook": amount = 2;  break;
                    case "Burnt Pan":     amount = 10; break;
                    case "Empty Gun":     amount = 12; break;
                    case "Worn Dagger":   amount = 15; break;
                    case "Real Knife":    amount = 99; break;
                    default:              UnitaleUtil.Warn("The item " + name + " doesn't exist in CYF's weapon item pool."); break;
                }
                break;
            case 2:
                switch (name) {
                    case "Faded Ribbon":   amount = 3;  break;
                    case "Manly Bandanna": amount = 7;  break;
                    case "Old Tutu":       amount = 10; break;
                    case "Temmie Armor":   amount = 20; break;
                    case "Cloudy Glasses": amount = 6;  break;
                    case "Stained Apron":  amount = 11; break;
                    case "Cowboy Hat":     amount = 12; break;
                    case "Heart Locket":   amount = 15; break;
                    case "The Locket":     amount = 99; break;
                    default:               UnitaleUtil.Warn("The item " + name + " doesn't exist in CYF's armor item pool."); break;
                }
                break;
            default:
                switch (name) {
                    case "Testing Dog":
                        mess = new[] { new TextMessage("This dog is testing something.\nI must leave it alone.", true, false) };
                        break;
                    case "Stick":
                        mess = new[] { new TextMessage("You throw the stick.[w:10]\nNothing happens.", true, false) };
                        break;
                    default:
                        UnitaleUtil.Warn("The item " + name + " doesn't exist in CYF's miscellaneous item pool.");
                        break;
                }
                break;
        }
    }

    public static int InventoryNumber(string itemName) {
        for (int i = 0; i < inventory.Count; i++)
            if (inventory[i].Name == itemName)
                return i + 1;
        return -1;
    }

    public static bool ItemExists(string itemName) {
        return NametoDesc.ContainsKey(itemName);
    }

    public static void RemoveItem(int index) {
        try { inventory.RemoveAt(index); }
        catch { /* ignored */ }
    }

    private static void SetEquip(int ID) {
        string Name = inventory[ID].Name;
        int mode;

        if (NametoType.ContainsKey(Name))   mode = NametoType[Name];
        else if (addedItems.Contains(Name)) mode = addedItemsTypes[addedItems.IndexOf(Name)];
        else                                throw new CYFException("The item \"" + Name + "\" doesn't exist.");

        switch (mode) {
            case 1:
                PlayerCharacter.instance.WeaponATK = itemStatAmount;
                RemoveItem(ID);
                AddItem(PlayerCharacter.instance.Weapon);
                PlayerCharacter.instance.Weapon = Name;
                break;
            case 2:
                PlayerCharacter.instance.ArmorDEF = itemStatAmount;
                RemoveItem(ID);
                AddItem(PlayerCharacter.instance.Armor);
                PlayerCharacter.instance.Armor = Name;
                break;
            default:
                throw new CYFException("The item \"" + Name + "\" can't be equipped.");
        }
    }

    public static TextMessage[] ChangeEquipment(int ID, bool silent = false) {
        string name = inventory[ID].Name;
        SetEquip(ID);
        return silent ? null : new[] { new TextMessage("You equipped the " + name + ".", true, false) };
    }

    public static void RemoveAddedItems() {
        for (int i = 0; i < inventory.Count; i ++)
            if (addedItems.Any(str => inventory[i].Name == str)) {
                inventory.RemoveAt(i);
                i --;
            }

        foreach (string str in addedItems) {
            if (str == PlayerCharacter.instance.Weapon && PlayerCharacter.instance.Weapon != "Stick" && !NametoDesc.ContainsValue(str)) {
                for (int i = 0; i < inventory.Count; i++)
                    if (inventory[i].Name == "Stick") {
                        inventory.RemoveAt(i);
                        break;
                    }
                PlayerCharacter.instance.Weapon = "Stick";
                PlayerCharacter.instance.WeaponATK = 0;
            } else if (str == PlayerCharacter.instance.Weapon && PlayerCharacter.instance.Weapon != "Stick" && NametoDesc.ContainsValue(str)) {
                TextMessage[] mess; float amount; string replacement;
                ItemLibrary(str, 1, out mess, out amount, out replacement);
                PlayerCharacter.instance.WeaponATK = (int)amount;
            }

            if (str == PlayerCharacter.instance.Armor && PlayerCharacter.instance.Armor != "Bandage" &&!NametoDesc.ContainsValue(str)) {
                for (int i = 0; i < inventory.Count; i++)
                    if (inventory[i].Name == "Bandage") {
                        inventory.RemoveAt(i);
                        break;
                    }
                PlayerCharacter.instance.Armor = "Bandage";
            } else if (str == PlayerCharacter.instance.Armor && PlayerCharacter.instance.Armor != "Bandage" && NametoDesc.ContainsValue(str)) {
                TextMessage[] mess; float amount; string replacement;
                ItemLibrary(str, 2, out mess,  out amount, out replacement);
                PlayerCharacter.instance.ArmorDEF = (int)amount;
            }
        }
        addedItems = new List<string>();
        addedItemsTypes = new List<int>();
    }
}