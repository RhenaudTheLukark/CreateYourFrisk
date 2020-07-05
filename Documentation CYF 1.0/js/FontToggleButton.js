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
