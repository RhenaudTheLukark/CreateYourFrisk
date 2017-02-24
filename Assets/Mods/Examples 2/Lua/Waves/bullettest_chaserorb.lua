-- The chasing attack from the documentation example.
chasingbullet = CreateProjectile('bullet', Arena.width/2, Arena.height/2)
chasingbullet.SetVar('xspeed', 0)
chasingbullet.SetVar('yspeed', 0)

function Update()
    local xdifference = Player.x - chasingbullet.x
    local ydifference = Player.y - chasingbullet.y
    local xspeed = chasingbullet.GetVar('xspeed') / 2 + xdifference / 100
    local yspeed = chasingbullet.GetVar('yspeed') / 2 + ydifference / 100
    chasingbullet.Move(xspeed, yspeed)
    chasingbullet.SetVar('xspeed', xspeed)
    chasingbullet.SetVar('yspeed', yspeed)
end