local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local customPedestal = {}

customPedestal.name = "BalintHelper/CustomPedestal"
customPedestal.depth = 8998

customPedestal.texture = "characters/theoCrystal/pedestal"
customPedestal.justification = {0.5, 1.0}

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

-- Explicit selection() to bypass the entity.width/height branch in getSelectionUnsafe.
-- The Solid constructor always stores width=32, height=32 in EntityData, which Lönn
-- picks up and uses as the selection box before it ever considers the texture bounds.
-- We build the drawable ourselves and return its actual rectangle instead.
function customPedestal.selection(room, entity)
    local texture = entity.spriteNormal or "characters/theoCrystal/pedestal"
    local sprite = drawableSprite.fromTexture(texture, entity)

    if sprite then
        sprite:setJustification(0.5, 1.0)
        return sprite:getRectangle()
    end

    -- Fallback: derive from texture dimensions manually
    -- (shouldn't happen if the atlas path is valid)
    return utils.rectangle(entity.x - 16, entity.y - 64, 32, 32)
end

return customPedestal