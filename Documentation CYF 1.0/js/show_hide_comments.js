var codeblocks = document.getElementsByClassName('code-container');
var supportsTransitions = supportsTransitions();

for (var i = 0; i < codeblocks.length; i++) {
    var item = codeblocks[i];  
    item.innerHTML = '<div class="showhidebtn"><span class="linklike" onclick="showComments(this, true)">Show</span> / <span class="linklike" onclick="showComments(this, false)">Hide</span> comments</div>' + item.innerHTML;
}

function showComments(target, disp){
    var comments = target.parentNode.parentNode.getElementsByClassName('comments');
    for (var i = 0; i < comments.length; i++) {
        var item = comments[i];  
        if(supportsTransitions){
            if(disp)
                item.style.opacity = "1.0";
            else
                item.style.opacity = "0.0";
        } else {
            item.style.visibility = disp ? "visible" : "hidden";
        }
    }
}

/* Code by vcsjones from StackOverflow at http://stackoverflow.com/questions/7264899/detect-css-transitions-using-javascript-and-without-modernizr
which in turn was adapted from this gist by jackfuchs https://gist.github.com/jackfuchs/556448 */

function supportsTransitions() {
    var b = document.body || document.documentElement,
        s = b.style,
        p = 'transition';

    if (typeof s[p] == 'string') { return true; }

    // Tests for vendor specific prop
    var v = ['Moz', 'webkit', 'Webkit', 'Khtml', 'O', 'ms'];
    p = p.charAt(0).toUpperCase() + p.substr(1);

    for (var i=0; i<v.length; i++) {
        if (typeof s[v[i] + p] == 'string') { return true; }
    }

    return false;
}