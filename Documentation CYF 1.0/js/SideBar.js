// Names of the various tabs of the side bar
// Empty strings are separators
const tabs = [
    "Welcome",
    "How to read this documentation",
    "",
    "Controls",
    "Basic Setup",
    "Unity Setup (Optional)",
    "Special Variables",
    "Terminology",

    "Text Commands",
    "Game Events",
    "",
    "Misc. Functions",
    "General Objects",
    "The Player Object",
    "The Script Object",
    "The Audio Object",
    "The NewAudio Object",
    "The Input Object",
    "The Text Object",
    "The Time Object",
    "The Inventory Object",
    "The Misc Object",
    "The Discord Object",
    "The Arena Object",
    "The UI Object",

    "",
    "Projectile Management",
    "The Pixel-Perfect Collision System",
    "Sprites &amp; Animation",
    "",

    "Shader Introduction",
    "The Shader Object",
    "Coding a Shader",
    "",

    "Overworld Basics",
    "How to create a map",
    "How to create an event",
    "How to animate an event",
    "How to create a shop",
    "",

    "The General Overworld Object",
    "The Event Overworld Object",
    "The Player Overworld Object",
    "The Screen Overworld Object",
    "The Inventory Overworld Object",
    "The Map Overworld Object",

    "",

    "Dialog Bubble Names",
    "Key List",
    "Item List"
];

// Links for tabs which have one
const links = new Map([
    ["Welcome", "index.html"],
    ["How to read this documentation", "howtoread.html"],
    ["Controls", "controls.html"],
    ["Basic Setup", "basic.html"],
    ["Unity Setup (Optional)", "unity.html"],
    ["Special Variables", "variables.html"],
    ["Terminology", "terms.html"],

    ["Text Commands", "api-text.html"],
    ["Game Events", "api-events.html"],

    ["Misc. Functions", "api-functions-main.html"],
    ["General Objects", "api-functions-object.html"],
    ["The Player Object", "api-functions-player.html"],
    ["The Script Object", "api-functions-script.html"],
    ["The Audio Object", "api-functions-audio.html"],
    ["The NewAudio Object", "api-functions-newaudio.html"],
    ["The Input Object", "api-functions-input.html"],
    ["The Text Object", "cyf-text.html"],
    ["The Time Object", "api-functions-time.html"],
    ["The Inventory Object", "cyf-inventory.html"],
    ["The Misc Object", "api-functions-misc.html"],
    ["The Discord Object", "api-functions-discord.html"],
    ["The Arena Object", "api-functions-waves.html"],
    ["The UI Object", "api-functions-ui.html"],

    ["Projectile Management", "api-projectile.html"],
    ["The Pixel-Perfect Collision System", "cyf-ppcollision.html"],
    ["Sprites &amp; Animation", "api-animation.html"],

    ["Shader Introduction", "shaders.html"],
    ["The Shader Object", "shaders-object.html"],
    ["Coding a Shader", "shaders-coding.html"],

    ["Overworld Basics", "overworld.html"],
    ["How to create a map", "overworld-howto-map.html"],
    ["How to create an event", "overworld-howto-event.html"],
    ["How to animate an event", "overworld-howto-animevent.html"],
    ["How to create a shop", "overworld-howto-shop.html"],

    ["The General Overworld Object", "overworld-object-general.html"],
    ["The Event Overworld Object", "overworld-object-event.html"],
    ["The Player Overworld Object", "overworld-object-player.html"],
    ["The Screen Overworld Object", "overworld-object-screen.html"],
    ["The Inventory Overworld Object", "overworld-object-inventory.html"],
    ["The Map Overworld Object", "overworld-object-map.html"],

    ["Dialog Bubble Names", "media/dialogoptions.png"],
    ["Key List", "api-keys.html"],
    ["Item List", "item-list.html"]
]);

// These pages are not in the pages folder of the docs
const pageIsInRoot = [ "Welcome", "Dialog Bubble Names" ];

// Tabs at which these categories start
const categoriesStart = new Map([
    ["Basics", "Welcome"],
    ["API", "Text Commands"],
    ["Functions &amp; Objects:", "Misc. Functions"],
    ["Shaders", "Shader Introduction"],
    ["Overworld", "Overworld Basics"],
    ["Overworld Objects:", "The General Overworld Object"],
    ["Resources", "Dialog Bubble Names"]
]);

// Tabs before which these categories ends
// If there's no value, then the category ends at the end of the side bar
const categoriesEnd = new Map([
    ["Basics", "Text Commands"],
    ["API", "Shader Introduction"],
    ["Functions &amp; Objects:", "Projectile Management"],
    ["Shaders", "Overworld Basics"],
    ["Overworld", "Dialog Bubble Names"],
    ["Overworld Objects:", "Dialog Bubble Names"]
]);

// Categories with an exclamation mark next to them
// Used to set apart categories with new elements in them
const isNew = [
    "Special Variables", "Text Commands", "Game Events",
    "The Text Object", "The Arena Object", "The UI Object",
    "Projectile Management", "Sprites &amp; Animation",
    "General Objects", "Item List", "Key List",
    "The Input Object" ];

// Categories with a <CYF> prefix
// Used for categories only added during CYF's development
const isCYF = [
    "Unity Setup (Optional)", "The NewAudio Object",
    "The Text Object", "The Inventory Object",
    "The Misc Object", "The Discord Object",
    "The UI Object", "The Pixel-Perfect Collision System",
    "Shaders", "Overworld", "Key List", "Item List" ];

// Returns the various classes an element should have depending on its category depth,
// whether it's active and whether it's a header or not
function GetClasses(title, depth, isActiveTab, isHeader = false) {
    var hasIndent = depth > 1;
    var hasNew = isNew.includes(title);
    var headerType = isHeader ? (depth == 1 ? "li-header " : "li-subheader ") : "";
    return headerType + (hasIndent ? "li-indent " : "") + (hasNew ? "new " : "") + (isActiveTab ? "active" : "");
}

// Fetches the link of an element. Returns an empty string if it has none
// Also takes in account whether the current page is in the pages folder or not
function GetLink(title, isInRoot) {
    if (!links.has(title))
        return "";
    var link = links.get(title);
    var needsRoot = pageIsInRoot.includes(title);
    if (isInRoot && !needsRoot)
        link = "pages/" + link;
    else if (!isInRoot && needsRoot)
        link = "../" + link;
    return link;
}

// Main function creating the entire side bar
function CreateSideBar(activeTab) {
    var contents = '<div class="col-md-2"><nav class="nav-sidebar"><ul class="nav tabs">';
    var categories = [];
    var isInRoot = pageIsInRoot.includes(activeTab);

    for (var i = 0; i < tabs.length; i++) {
        var title = tabs[i];

        // Enter category
        for (var [key, value] of categoriesStart.entries())
            if (value == title) {
                categories.push(key);
                categoriesStart.delete(key);

                // Add category to the HTML content to add
                var classes = GetClasses(key, categories.length, false, true);
                var line = '<li ' + (classes == "" ? '' : (' class="' + classes + '"')) + '>'

                if (isCYF.includes(key))
                    line += '<span class="CYF"></span> ';
                line += key + '</li>\n';
                contents += line;
            }

        if (title == "") {
            // Separators
            contents += '<hr style="margin-top:0px; margin-bottom:5px;">\n';
        } else {
            var depth = categories.length;
            var isActiveTab = activeTab == title;

            // Add line
            var classes = GetClasses(title, depth, isActiveTab);
            var line = '<li' + (classes == "" ? '' : (' class="' + classes + '"'));
            if (isActiveTab)
                line += ' style="margin-left:5px;"'
            line += '>';

            // Add link or '< >' characters around the title
            var link = GetLink(title, isInRoot);
            if (isActiveTab)
                line += '&gt; ';
            else if (link != "")
                line += '<a href="' + link + '">';

            if (isCYF.includes(title))
                line += '<span class="CYF"></span> ';

            // Add the title
            line += title;

            // Close the open elements
            if (isActiveTab)
                line += ' &lt;';
            else if (link != "")
                line += '</a>';
            line += '</li>\n';

            // Add line to the contents to display
            contents += line;
        }

        // Exit category
        if (i < tabs.length - 1) {
            var nextTitle = tabs[i + 1];
            for (var j = categories.length; j >= 0; j--)
                if (categoriesEnd.get(categories[j]) == nextTitle)
                    categories.splice(j);
        }
    }

    contents += '</ul></nav></div>';

    // Write it all in the HTML document
    document.write(contents);
}