-- The chasing attack from the documentation example.
testedge = {false,false,false,false}
xtemp = -Arena.width/2
ytemp = -Arena.width/2
temp = math.floor(math.random(100) / 25)
while testedge[temp] == true do
    temp = math.floor(math.random(100) / 25)
end
if temp == 0 or temp == 2 then
    xtemp = Arena.width/2
end
if temp == 0 or temp == 1 then
    ytemp = Arena.width/2
end
testedge[temp] = true
chasingbullet1 = CreateProjectile('largeball', xtemp, ytemp)
chasingbullet1.SetVar('xspeed', 0)
chasingbullet1.SetVar('yspeed', 0)
xtemp = -Arena.width/2
ytemp = -Arena.width/2
temp = math.floor(math.random(100) / 25)
while testedge[temp] == true do
    temp = math.floor(math.random(100) / 25)
end
if temp == 0 or temp == 2 then
    xtemp = Arena.width/2
end
if temp == 0 or temp == 1 then
    ytemp = Arena.width/2
end
testedge[temp] = true
chasingbullet2 = CreateProjectile('largeball', xtemp, ytemp)
chasingbullet2.SetVar('xspeed', 0)
chasingbullet2.SetVar('yspeed', 0)
xtemp = -Arena.width/2
ytemp = -Arena.width/2
temp = math.floor(math.random(100) / 25)
while testedge[temp] == true do
    temp = math.floor(math.random(100) / 25)
end
if temp == 0 or temp == 2 then
    xtemp = Arena.width/2
end
if temp == 0 or temp == 1 then
    ytemp = Arena.width/2
end
testedge[temp] = true
chasingbullet3 = CreateProjectile('largeball', xtemp, ytemp)
chasingbullet3.SetVar('xspeed', 0)
chasingbullet3.SetVar('yspeed', 0)
rotation = 0

function Update()
    local xdifference1 = Player.x - chasingbullet1.x
    local ydifference1 = Player.y - chasingbullet1.y
    local xspeed1 = chasingbullet1.GetVar('xspeed') / 2 + xdifference1 / 100
    local yspeed1 = chasingbullet1.GetVar('yspeed') / 2 + ydifference1 / 100
    chasingbullet1.Move(xspeed1, yspeed1)
    chasingbullet1.SetVar('xspeed', xspeed1)
    chasingbullet1.SetVar('yspeed', yspeed1)
    chasingbullet1.sprite.rotation = rotation
    local xdifference2 = Player.x - chasingbullet2.x
    local ydifference2 = Player.y - chasingbullet2.y
    local xspeed2 = chasingbullet2.GetVar('xspeed') / 2 + xdifference2 / 100
    local yspeed2 = chasingbullet2.GetVar('yspeed') / 2 + ydifference2 / 100
    chasingbullet2.Move(xspeed2, yspeed2)
    chasingbullet2.SetVar('xspeed', xspeed2)
    chasingbullet2.SetVar('yspeed', yspeed2)
    chasingbullet2.sprite.rotation = rotation
    local xdifference3 = Player.x - chasingbullet3.x
    local ydifference3 = Player.y - chasingbullet3.y
    local xspeed3 = chasingbullet3.GetVar('xspeed') / 2 + xdifference3 / 100
    local yspeed3 = chasingbullet3.GetVar('yspeed') / 2 + ydifference3 / 100
    chasingbullet3.Move(xspeed3, yspeed3)
    chasingbullet3.SetVar('xspeed', xspeed3)
    chasingbullet3.SetVar('yspeed', yspeed3)
    chasingbullet3.sprite.rotation = rotation
    rotation = rotation + 10
end