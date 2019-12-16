#dirty python script to read out unity multi-sprite meta files and make XML maps out of them

#files = ["uidialog", "monster", "papyrus", "sans", "uibattlesmall", "uidamagetext"]
files = ["wingdings"]

nametag = "- name: "
bordertag = "border: "
xtag = "x: "
ytag = "y: "
wtag = "width: "
htag = "height: "
rx, ry, rw, rh, bx, by, bz, bw = 0, 0, 0, 0, 0, 0, 0, 0
charname = ""
xmldict = {}
xmlhead = """<?xml version="1.0" encoding="iso-8859-1"?>
<font>
<spritesheet>
"""

xmlnode = """<sprite name="%s">
    <rect x="%i" y="%i" w="%i" h="%i"/>
    <border x="%i" y="%i" z="%i" w="%i"/>
</sprite>
"""

xmltail = "</spritesheet>\n</font>"
xmlstr = ""
def parseborder(bstr):
    global bx, by, bz, bw
    bstr = bstr[1:-1]
    vars = bstr.split(',')
    for var in vars:
        var = var.strip()
        intv = int(var[2:])
        if var[0] == 'x':
            bx = intv
        if var[0] == 'y':
            by = intv
        if var[0] == 'z':
            bz = intv
        if var[0] == 'w':
            bw = intv
    
    
def striptag(line, tag):
    return line[len(tag):]

    
for fn in files:
    file = open(fn + ".png.meta")
    xmlfile = open(fn + ".xml", "w")
    lines = file.readlines()
    xmlstr = xmlhead
    for line in lines:
        line = line.strip()
        if line.startswith(nametag):
            print line
            charname = striptag(line, nametag)
            if charname =="''''":
                charname = "'"
            elif charname[0] == "'":
                charname = charname[1]
                
            if charname == "&":
                charname = "ampersand"
        if line.startswith(xtag):
            rx = int(striptag(line, xtag))
        if line.startswith(ytag):
            ry = int(striptag(line, ytag))
        if line.startswith(wtag):
            rw = int(striptag(line, wtag))
        if line.startswith(htag):
            rh = int(striptag(line, htag))
        if line.startswith(bordertag):
            print rx,ry,rw,rh
            borderline = striptag(line, bordertag)
            parseborder(borderline)
            if bx+by+bz+bw > 0:
                print 'borders:', bx,by,bz,bw
            xmldict[charname] = xmlnode%(charname, rx,ry,rw,rh,bx,by,bz,bw)
            print "--- CHARACTER COMPLETE ---"

    for k,v in sorted(xmldict.items()):
        xmlstr += v
    xmldict = {}        
    xmlstr += xmltail
    xmlfile.write(xmlstr)
    xmlfile.close()
    xmlstr = ""

