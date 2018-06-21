/*var ToggleButton = document.createElement("button");

ToggleButton.style.fontFamily = "8bitoperator";
ToggleButton.style.color = "white";
ToggleButton.style.textShadow = "1px 1px rgba(0, 0, 0, 0.5)";
ToggleButton.style.backgroundColor = "#aaaaaa";
ToggleButton.style.marginLeft = "-10px";
ToggleButton.style.marginBottom = "5px";

ToggleButton.innerHTML = "Toggle Font";
ToggleButton.onclick = SwapFonts;

var node = document.getElementsByClassName("tabs")[0];

node.insertBefore(ToggleButton, node.firstChild);*/

/*var link = document.createElement("a");
link.href = "javascript:SwapFonts();";

var node = document.getElementsByClassName("col-xs-3")[0];
var img = node.getElementsByClassName("centerbt")[0];

node.appendChild(link);
link.appendChild(img);*/

var node = document.getElementsByClassName("tabs")[0];

var img = document.createElement("img");
var link = document.createElement("a");

var src;

var locationSplit = window.location.pathname.split("/");
if (locationSplit[locationSplit.length - 2] !== "pages") {
    src = "img/fontbt_0.png";
} else {
    src = "../img/fontbt_0.png";
}

img.src = src;
link.href = "javascript:SwapFonts();";

img.style = "margin-bottom: 10px; margin-top: 5px;";
link.style = "left: 12.5%; position: relative;";

link.onmouseover = function() {img.src = img.src.replace("_0", "_1");}
link.onmouseout = function() {img.src = img.src.replace("_1", "_0");}

node.insertBefore(link, node.firstChild);
link.appendChild(img);

var fontsSwapped = false;
        
function getCSS(element, property) {
    return window.getComputedStyle(element, null).getPropertyValue(property);
}

function SwapFonts() {
    var i;
    var table = document.body.getElementsByTagName("*");
    for (i = 0; i < table.length; i++) {
        if (getCSS(table[i], "font-family").indexOf("monospace") == -1 && getCSS(table[i], "font-family").indexOf("courier new") == -1) {
            //alert(getCSS(table[i], "font-family"));
            
            if (!fontsSwapped) {
                table[i].style.fontFamily = "sans-serif";
                // table[i].style.fontWeight = "bold";
            } else {
                table[i].style.fontFamily = "'8bitoperator', sans-serif";
                // table[i].style.fontWeight = "normal";
            }
        }
    }
    fontsSwapped = !fontsSwapped;
}
