-- Set of animations
-- {fps,sprite_set,repeat}
Anim_To_Fight = {10,
{"Stances/transit_to_fight1",
"Stances/transit_to_fight2",
"Stances/transit_to_fight3",
"Stances/transit_to_fight4"},
false}
Anim_To_Side = {10,
{"Stances/transit_to_side1",
"Stances/transit_to_side2",
"Stances/transit_to_side3"},
false}
Anim_To_Side_Inv = {10,
{"Stances/transit_to_side3",
"Stances/transit_to_side2",
"Stances/transit_to_side1"},
false}

SetGlobal("anim_global",Anim_to_Fight)