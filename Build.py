import sys, os, subprocess, shutil, math

'''
Welcome to the CYF build script!

This is a specialized script to build CYF releases completely automatically.
The only requirements are Unity 2018.2.13f1, and 7-zip if you wish to auto-package the builds too.
Just set up the options below in "Script Vars" to your liking and run the script!
'''

### Script Vars ###

# This is the version of CYF to name the executables and the Documentation folder
CYFversion = "0.6.4"

# This is the path we will build to
buildPath = os.getcwd() + "\\bin"
if not "bin" in os.listdir():
    print("Creating \"bin\" folder...", end="")
    sys.stdout.flush()
    os.mkdir("bin")
    print("Done.")
else:
    print("\"bin\" folder already exists.")

# This is the path of Unity on your machine
unityPath = "C:\\Program Files\\Unity\\Hub\\Editor\\2018.2.13f1\\Editor\\Unity.exe"
if not os.path.exists(unityPath):
    unityPath = "C:\\Program Files\\Unity\\Editor\\Unity.exe"

# This determines if this script will attempt to use 7-zip to package the builds after they have been created
doPackage = True

# This is the path of 7-zip on your machine
sevenZPath = "C:\\Program Files (x86)\\7-Zip\\7z.exe"
if not os.path.exists(sevenZPath):
    sevenZPath = "C:\\Program Files\\7-zip\\7z.exe"

### Actual code or whatever ###

# Define a list of pairs of folder names, command line arguments for Unity, and target names respectively
# * Mac is not in this list, see below
buildTargets = [
    ("CYF v" + CYFversion + " - Windows (32-bit)", "-buildWindowsPlayer",        "Create Your Frisk " + CYFversion + ".exe"),
    ("CYF v" + CYFversion + " - Windows (64-bit)", "-buildWindows64Player",      "Create Your Frisk " + CYFversion + ".exe"),
    ("CYF v" + CYFversion + " - Linux (32-bit)",   "-buildLinux32Player",        "Create Your Frisk " + CYFversion + ".x86"),
    ("CYF v" + CYFversion + " - Linux (64-bit)",   "-buildLinux64Player",        "Create Your Frisk " + CYFversion + ".x86_64")
]
macTarget = ("CYF v" + CYFversion + " - Mac",      "-buildOSXUniversalPlayer",   "Create Your Frisk " + CYFversion + ".app")

def buildWithUnity(folder, argument, target):
    print("")
    if len(folder) < 76:
        print("╒" + ("═" * math.ceil((74 - len(folder)) / 2)) + folder + ("═" * math.floor((74 - len(folder)) / 2)) + "╕")
    else:
        print(folder)
    # Make sure destination folder doesn't exist
    if folder in os.listdir("bin"):
        print("Folder " + folder + " already exists, deleting...", end="")
        sys.stdout.flush()
        try:
            shutil.rmtree("bin\\" + folder)
            print("Done.")
        except:
            print("\n\nFatal error when attempting to delete \"bin\\" + folder + "\" folder. Exiting.\nYou should probably delete it manually.")
            sys.exit()
    
    # Build the Unity executable
    print("Begin Unity build for " + folder + "...", end="")
    sys.stdout.flush()
    subprocess.call([unityPath, "-batchmode", "-logFile " + buildPath + "\\output_" + folder + ".txt", argument, target, "-quit"])
    print("Done.")
    
    # Copy over the Documentation
    print("Copying Documentation...", end="")
    sys.stdout.flush()
    shutil.copytree("Documentation CYF 1.0", buildPath + "\\" + folder + "\\Documentation CYF " + CYFversion)
    print("Done.")
    
    # Copy over the Default and Mods folders
    print("Copying Default folder...", end="")
    sys.stdout.flush()
    shutil.copytree("Assets\\Default", buildPath + "\\" + folder + "\\Default")
    print("Removing .meta files...", end="")
    sys.stdout.flush()
    for path,dirs,files in os.walk(buildPath + "\\" + folder + "\\Default"):
        for file in files:
            if file.endswith(".meta"):
                os.remove(path + "\\" + file)
    print("Done.")
    
    print("Copying Mods folder...", end="")
    sys.stdout.flush()
    shutil.copytree("Assets\\Mods", buildPath + "\\" + folder + "\\Mods")
    print("Removing .meta files...", end="")
    sys.stdout.flush()
    for path,dirs,files in os.walk(buildPath + "\\" + folder + "\\Mods"):
        for file in files:
            if file.endswith(".meta"):
                os.remove(path + "\\" + file)
    print("Hiding @0.5.0_SEE_CRATE...", end="")
    sys.stdout.flush()
    os.system("attrib +h +s \"bin\\" + folder + "\\Mods\\@0.5.0_SEE_CRATE\"")
    print("Done.")
    
    print("Adding \"Mods starting with @ won't appear in CYF\"...", end="")
    sys.stdout.flush()
    open(buildPath + "\\" + folder + "\\Mods\\Mods starting with @ won't appear in CYF", "w").close()
    print("Done.")
    
    if len(folder) < 76:
        print("╘" + ("═" * 74) + "╛")
    print("")

# Let's do it!...Except for Mac
print("Disabling allowFullscreenSwitch...", end="")
sys.stdout.flush()
psCurrent = open("ProjectSettings\\ProjectSettings.asset", "r")
settingsCurrent = psCurrent.read()
psCurrent.close()
ps = open("ProjectSettings\\ProjectSettings.asset", "w")
settings = settingsCurrent.replace("allowFullscreenSwitch: 1", "allowFullscreenSwitch: 0")
ps.write(settings)
ps.close()
print("Done.\n")

for target in buildTargets:
    buildWithUnity(target[0], target[1], buildPath + "\\" + target[0] + "\\" + target[2])

# Now, the special behavior for Mac
print("")
if len(macTarget[0]) < 76:
    print("╒" + ("═" * math.floor((74 - len(macTarget[0])) / 2)) + macTarget[0] + ("═" * math.ceil((74 - len(macTarget[0])) / 2)) + "╕")
else:
    print(macTarget[0])

print("Enabling allowFullscreenSwitch...", end="")
sys.stdout.flush()
psCurrent = open("ProjectSettings\\ProjectSettings.asset", "r")
settingsCurrent = psCurrent.read()
psCurrent.close()
ps = open("ProjectSettings\\ProjectSettings.asset", "w")
settings = settingsCurrent.replace("allowFullscreenSwitch: 0", "allowFullscreenSwitch: 1")
ps.write(settings)
ps.close()
print("Done.")

# Make sure destination folder doesn't exist
if macTarget[0] in os.listdir("bin"):
    print("Folder " + macTarget[0] + " already exists, deleting...", end="")
    sys.stdout.flush()
    try:
        shutil.rmtree("bin\\" + macTarget[0])
        print("Done.")
    except:
        print("\n\nFatal error when attempting to delete \"bin\\" + macTarget[0] + "\" folder. Exiting.\nYou should probably delete it manually.")
        sys.exit()

# Build the Unity executable
print("", end="")
print("Begin Unity build for " + macTarget[0] + "...", end="")
sys.stdout.flush()
subprocess.call([unityPath, "-batchmode", "-logFile " + buildPath + "\\output.txt", macTarget[1], buildPath + "\\" + macTarget[0] + "\\" + macTarget[2], "-quit"])
print("Done.")

# Copy over the Documentation
print("Copying Documentation...", end="")
sys.stdout.flush()
shutil.copytree("Documentation CYF 1.0", buildPath + "\\" + macTarget[0] + "\\Documentation CYF " + CYFversion)
print("Done.")

# Copy over the Default and Mods folders
print("Copying Default folder...", end="")
sys.stdout.flush()
shutil.copytree("Assets\\Default", buildPath + "\\" + macTarget[0] + "\\" + macTarget[2] + "\\Default")
print("Removing .meta files...", end="")
sys.stdout.flush()
for path,dirs,files in os.walk(buildPath + "\\" + macTarget[0] + "\\" + macTarget[2] + "\\Default"):
    for file in files:
        if file.endswith(".meta"):
            os.remove(path + "\\" + file)
print("Done.")

print("Copying Mods folder...", end="")
sys.stdout.flush()
shutil.copytree("Assets\\Mods", buildPath + "\\" + macTarget[0] + "\\" + macTarget[2] + "\\Mods")
print("Removing .meta files...", end="")
sys.stdout.flush()
for path,dirs,files in os.walk(buildPath + "\\" + macTarget[0] + "\\" + macTarget[2] + "\\Mods"):
    for file in files:
        if file.endswith(".meta"):
            os.remove(path + "\\" + file)
print("Hiding @0.5.0_SEE_CRATE...", end="")
sys.stdout.flush()
os.system("attrib +h +s \"bin\\" + macTarget[0] + "\\" + macTarget[2] + "\\Mods\\@0.5.0_SEE_CRATE\"")
print("Done.")

print("Adding \"Mods starting with @ won't appear in CYF\"...", end="")
sys.stdout.flush()
open(buildPath + "\\" + macTarget[0] + "\\" + macTarget[2] + "\\Mods\\Mods starting with @ won't appear in CYF", "w").close()
print("Done.")

# Copy over the warning .txt file
print("Copying \"How to add mods to CYF (Mac).txt\"...", end="")
sys.stdout.flush()
shutil.copyfile("How to add mods to CYF (Mac).txt", buildPath + "\\" + macTarget[0] + "\\How to add mods to CYF (Mac).txt")
print("Done.")

print("Disabling allowFullscreenSwitch...", end="")
sys.stdout.flush()
psCurrent = open("ProjectSettings\\ProjectSettings.asset", "r")
settingsCurrent = psCurrent.read()
psCurrent.close()
ps = open("ProjectSettings\\ProjectSettings.asset", "w")
settings = settingsCurrent.replace("allowFullscreenSwitch: 1", "allowFullscreenSwitch: 0")
ps.write(settings)
ps.close()
print("Done.")

if len(macTarget[0]) < 76:
    print("╘" + ("═" * 74) + "╛")

if doPackage:
    print("\nBegin packaging all builds into archives through 7-zip.")
    binContents = os.listdir("bin")
    for build in [x for x in binContents if os.path.isdir(os.path.join(os.path.abspath("bin"), x))]:
        print("Creating \"" + build + ".zip\"...")
        sys.stdout.flush()
        subprocess.call([sevenZPath, "a", "-mx9", buildPath + "\\" + build + ".zip", buildPath + "\\" + build + "\\*", "-bso0", "-bsp0"])
        print("Done.")

print("\n\n\nAll done!")
print("Now, you must test all Create Your Frisk builds before release and clean up their Mods folders if applicable!")
print("Have fun mooving boolet.\n")
os.system("pause")
