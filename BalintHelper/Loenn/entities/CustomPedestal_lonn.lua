-- Lönn plugin for BalintHelper/CustomPedestal
-- Place at: Mods/BalintHelper/Loenn/entities/CustomPedestal.lua

local customPedestal = {}

customPedestal.name = "BalintHelper/CustomPedestal"
customPedestal.depth = 8998

customPedestal.placements = {
    {
        name = "normal",
        data = {
            spriteNormal           = "characters/theoCrystal/pedestal",
            spriteBroken           = "characters/theoCrystal/pedestal",
            returnDelay            = 0.0,
            instantReturnInBounds  = false,
            maxDistance            = 0.0,
            entityTypes            = "TheoCrystal",
            breakable              = false,
            brokenDisableDuration  = 0.0,
            showReturnLine         = true,
            particleReturn         = "",
            particleExplode        = "",
            particleBreak          = "",
            soundTeleport          = "event:/game/05_mirror_temple/crystaltheo_appear",
            soundBreak             = "event:/game/05_mirror_temple/crystaltheo_break_free",
            soundRepair            = "event:/game/general/strawberry_get",
            width                  = 32,
            height                 = 32,
        }
    }
}

customPedestal.fieldInformation = {
    returnDelay           = { fieldType = "number", minimumValue = 0.0 },
    maxDistance           = { fieldType = "number", minimumValue = 0.0 },
    brokenDisableDuration = { fieldType = "number", minimumValue = 0.0 },
    entityTypes           = { fieldType = "string" },
    spriteNormal          = { fieldType = "string" },
    spriteBroken          = { fieldType = "string" },
    particleReturn        = { fieldType = "string" },
    particleExplode       = { fieldType = "string" },
    particleBreak         = { fieldType = "string" },
    soundTeleport         = { fieldType = "string" },
    soundBreak            = { fieldType = "string" },
    soundRepair           = { fieldType = "string" },
}

function customPedestal.sprite(room, entity)
    local spritePath = entity.spriteNormal or "characters/theoCrystal/pedestal"
    -- Lönn renders this from GFX atlas; return a simple drawable
    return {
        {
            meta = {
                texture = spritePath,
                justificationX = 0.5,
                justificationY = 1.0,
                x = 0,
                y = 0,
            }
        }
    }
end

return customPedestal
