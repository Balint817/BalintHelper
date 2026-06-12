local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local customPedestal = {}

customPedestal.name = "BalintHelper/CustomPedestal"
customPedestal.depth = 8998

customPedestal.texture = "characters/theoCrystal/pedestal"
customPedestal.justification = {0.5, 1.0}

customPedestal.placements = {
    {
        name = "Custom Theo Crystal Pedestal",
        data = {
            spriteNormal           = "characters/theoCrystal/pedestal",
            spriteBroken           = "objects/pedestal/damaged",
            startBroken            = false,
            returnDelay            = 2.0,
            instantReturnInBounds  = true,
            maxDistance            = 0.0,
            entityTypes            = "TheoCrystal",
            breakable              = false,
            brokenDisableDuration  = 5.0,
            showReturnLine         = true,
            canDash                = false,
            canExplode             = false,
            canGrab                = true,
            returnParticleColorA   = "7fffff",
            returnParticleColorB   = "ffffff",
            explodeParticleColorA  = "7fffff",
            explodeParticleColorB  = "ffffff",
            breakParticleColorA    = "ffffff",
            breakParticleColorB    = "aaaaaa",
            repairParticleColorA   = "7fffff",
            repairParticleColorB   = "ffffff",
            soundTeleport          = "event:/game/01_forsaken_city/birdbros_thrust",
            soundBreak             = "event:/game/05_mirror_temple/crystaltheo_break_free",
            soundRepair            = "event:/game/09_core/iceblock_reappear",
        }
    }
}

customPedestal.fieldInformation = {
    returnDelay           = { fieldType = "number", minimumValue = 0.0, description = "Time in seconds before teleporting" },
    maxDistance           = { fieldType = "number", minimumValue = 0.0, description = "Maximum distance at which a teleport can occur, or 0 for no limit" },
    brokenDisableDuration = { fieldType = "number", minimumValue = 0.0, description = "Time in seconds the pedestal stays inactive after being broken, or 0 to stay broken" },
    entityTypes           = { fieldType = "string", description = "Comma-separated entity type names and/or numeric Lönn entity IDs to track. Entities must be holdable." },
    spriteNormal          = { fieldType = "string", description = "Atlas path for intact state sprite" },
    spriteBroken          = { fieldType = "string", description = "Atlas path for broken state sprite" },
    returnParticleColorA  = { fieldType = "color", description = "Primary color for the return line's particles" },
    returnParticleColorB  = { fieldType = "color", description = "Secondary color for the return line's particles" },
    explodeParticleColorA = { fieldType = "color", description = "Primary color for the teleport particles" },
    explodeParticleColorB = { fieldType = "color", description = "Secondary color for the teleport particles" },
    breakParticleColorA   = { fieldType = "color", description = "Primary color for the break particles" },
    breakParticleColorB   = { fieldType = "color", description = "Secondary color for the break particles" },
    repairParticleColorA  = { fieldType = "color", description = "Primary color for the repair particles" },
    repairParticleColorB  = { fieldType = "color", description = "Secondary color for the repair particles" },
    soundTeleport         = { fieldType = "string", description = "Sound event for teleporting" },
    soundBreak            = { fieldType = "string", description = "Sound event for breaking" },
    soundRepair           = { fieldType = "string", description = "Sound event for repairing" },
    breakable			  = { fieldType = "boolean", description = "Whether the pedestal can be 'broken' (disabled) by dashing into it." },
    startBroken			  = { fieldType = "boolean", description = "Whether the pedestal starts in a broken (disabled) state." },
    instantReturnInBounds = { fieldType = "boolean", description = "Whether delay should be skipped within the pedestal's hitbox." },
    showReturnLine		  = { fieldType = "boolean", description = "Whether to show a line of particles returning to the pedestal to show where an entity is being teleported." },
    canDash		          = { fieldType = "boolean", description = "If breakable=true, determines whether it can be triggered by a dash." },
    canExplode 	          = { fieldType = "boolean", description = "If breakable=true, determines whether it can be triggered by an explosion." },
    canGrab 	          = { fieldType = "boolean", description = "Whether an item can be retrieved without breaking the pedestal." },
}

customPedestal.fieldOrder = {
    "x", "y",
    "returnDelay", "brokenDisableDuration",
    "maxDistance", "entityTypes",
    "breakable", "startBroken", "instantReturnInBounds", "showReturnLine",
    "canDash", "canExplode", "canGrab",
    "spriteNormal", "spriteBroken",
    "soundTeleport", "soundBreak", "soundRepair",
    "returnParticleColorA", "returnParticleColorB",
    "explodeParticleColorA", "explodeParticleColorB",
    "breakParticleColorA", "breakParticleColorB",
    "repairParticleColorA", "repairParticleColorB",
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