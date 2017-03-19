-- The bouncing bullets attack from the documentation example.
spawntimer = 0
bullet = CreateProjectile("BoneCenter", Arena.width, 0)
bullet.ppcollision = true
bullet1 = CreateProjectile("BoneEdge", Arena.width, 0)
bullet1.sprite.SetParent(bullet.sprite)
bullet1.sprite.SetAnchor(0.5, 1)
bullet1.sprite.SetPivot(0.5, 0)
bullet1.sprite.x = 0
bullet1.sprite.y = 0
bullet1.ppcollision = true
bullet2 = CreateProjectile("BoneEdge", Arena.width, 0)
bullet2.ppcollision = true
bullet2.sprite.rotation = 180
bullet2.sprite.SetParent(bullet.sprite)
bullet2.sprite.SetAnchor(0.5, 0)
bullet2.sprite.SetPivot(0.5, 0)
bullet2.sprite.x = 0
bullet2.sprite.y = 0
bullet.Remove()
bullets = {}
--[[bullet = CreateProjectile("fball+", -Arena.width, Arena.height, "Top")
bullet.sprite.Scale(2, 0.5)
bullet1 = CreateProjectile("fball+", 0, Arena.height, "Top")
bullet1.sprite.Scale(-1, -1)
bullet2 = CreateProjectile("fball+", Arena.width, Arena.height, "Top")
bullet2.sprite.Scale(-2, -3)
bullet3 = CreateProjectile("BoneCenter", Arena.width, 0)
bullet4 = CreateProjectile("BoneEdge", Arena.width, 0)
bullet4.sprite.SetParent(bullet3.sprite)
bullet4.sprite.SetAnchor(0.5, 1)
bullet4.sprite.SetPivot(0.5, 0)
bullet5 = CreateProjectile("BoneEdge", Arena.width, 0)
bullet5.sprite.SetParent(bullet3.sprite)
bullet5.sprite.SetAnchor(0.5, 0)
bullet5.sprite.SetPivot(0.5, 0)
bullet5.sprite.rotation = 180]]

function Update()
    --bullet3.sprite.rotation = bullet3.sprite.rotation + 1
	if Input.GetKey("X") == -1 then EndWave() end
    --[[if Input.GetKey("E") == 1 then  Encounter["enemies"][1].Call("BindToArena", true)           end  --In EncounterGO, normal
        if Input.GetKey("D") == 1 then  Encounter["enemies"][1].Call("BindToArena", false)          end  --In arena_container, above the Arena
        if Input.GetKey("C") == 1 then  Encounter["enemies"][1].Call("BindToArena", {false, true})  end  --In arena_container, below the Arena
        if Input.GetKey("R") == 1 then  Encounter["enemies"][2].Call("BindToArena", true)           end
        if Input.GetKey("F") == 1 then  Encounter["enemies"][2].Call("BindToArena", false)          end
        if Input.GetKey("V") == 1 then  Encounter["enemies"][2].Call("BindToArena", {false, true})  end]]
	
    --[[if Input.GetKey("R") == -1 and not bullet.isactive then
        bullet = CreateProjectile("fball+", -Arena.width, Arena.height, "Top") 
            bullet.sprite.Scale(2, 0.5)
        end
        if Input.GetKey("F") == -1 and not bullet1.isactive then
            bullet1 = CreateProjectile("fball+", 0, Arena.height, "Top")
            bullet.sprite.Scale(-1, -1)
        end
        if Input.GetKey("V") == -1 and not bullet2.isactive then
            bullet2 = CreateProjectile("fball+", Arena.width, Arena.height, "Top")
            bullet2.sprite.Scale(-2, -3)
        end
        if Input.GetKey("T") == -1 then bullet.sprite.Dust(true, true)  end
        if Input.GetKey("G") == -1 then bullet1.sprite.Dust(true, true) end
        if Input.GetKey("B") == -1 then bullet2.sprite.Dust(true, true) end	]]
    if Input.GetKey("O") == -1 then DEBUG(Encounter["enemies"][1]["monstersprite"].spritename) end	
    --if Input.GetKey("L") == -1 then DEBUG(bullet2.sprite.spritename) end	
    if Input.GetKey("Y") == -1 then DEBUG(NewAudio.GetAudioName("testmusic")) end	
    if Input.GetKey("H") == -1 then DEBUG(NewAudio.GetAudioName("testvoice")) end	
    if Input.GetKey("N") == -1 then DEBUG(NewAudio.GetAudioName("testsound")) end	
    if Input.GetKey("W") == -1 then 
        DEBUG(Audio.filename)
        DEBUG("Before")
        NewAudio.PlayMusic("src", "mus_zz_megalovania")
        DEBUG("After")
        DEBUG(Audio.filename)
    end	
	
    spawntimer = spawntimer + 1
    if spawntimer%30 == 0 then
        local posx = 30 - math.random(60)
        local posy = Arena.height/2
        local bullet = CreateProjectile('bullet', posx, posy)
        bullet.SetVar('velx', 1 - 2*math.random())
        bullet.SetVar('vely', 0)
        table.insert(bullets, bullet)
    end
    
    for i=1,#bullets do
        local bullet = bullets[i]
        local velx = bullet.GetVar('velx')
        local vely = bullet.GetVar('vely')
        local newposx = bullet.x + velx
        local newposy = bullet.y + vely
        if(bullet.x > -Arena.width/2 and bullet.x < Arena.width/2) then
            if(bullet.y < -Arena.height/2 + 8) then 
                newposy = -Arena.height/2 + 8
                vely = 4
            end
        end
        vely = vely - 0.04
        bullet.MoveTo(newposx, newposy)
        bullet.SetVar('vely', vely)
    end
end

function EndingWave() 
end

function OnHit(bullet)
    Player.Hurt(300)
end