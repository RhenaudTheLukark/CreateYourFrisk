-- A library to add random voices to every letter in a dialogue.
-- First, we make a new table for our random voices and our module.

local voices = {} -- This will contain the voices we're going to use. Only accessible from within this library.
local randomvoicer = {} -- The actual module.

-- You can change your voices from your own scripts with this. See the actual encounter for usage.
function randomvoicer.setvoices(table)
    voices = table 
end

-- This randomizes all lines in a table.
function randomvoicer.randomizetable(table)
    for i=1,#table do
        table[i] = randomvoicer.randomizeline(table[i])
    end
    return table
end

-- This function will take care of inserting a random voice in front of every letter.
function randomvoicer.randomizeline(text)
    local skipping = false -- We will use this variable to stop inserting voices when we find [ and continue when we find ], otherwise we'll screw up commands.
    -- First, we can just skip the whole thing if there aren't any voices.
    if #voices == 0 then
        return text
    end

    -- Now we can go over every letter in the text, and add a voice to it.
    local temptext = ""
    for i=1,#text do
        local nextletter = text:sub(i,i) -- Get the next letter.
        if nextletter == "[" then -- Start skipping text when we encounter a command.
            skipping = true
        elseif nextletter == "]" then -- We can stop skipping again when the command is over.
            skipping = false
        elseif skipping == false then -- If we aren't skipping, we can insert random voices.
            temptext = temptext .. randomvoicer.voice() -- Adds a random voice to the temporary string.
        end
        temptext = temptext .. nextletter -- In all cases, we should include the next letter of the string.
    end

    return temptext -- Don't forget to return the modified text.
end

-- This returns a random voice command depending on what voices you have set here.
function randomvoicer.voice()
    if #voices == 0 then
        return ""
    end

    return "[voice:" .. voices[math.random(#voices)] .. "]"
end

return randomvoicer