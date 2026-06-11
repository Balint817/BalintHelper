local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local customTheoGate = {}

customTheoGate.name = "BalintHelper/CustomTheoGate"
customTheoGate.depth = -9000

local texture = "objects/door/TempleDoorC00"

customTheoGate.texture = texture
customTheoGate.justification = { 0.0, 0.0 }

customTheoGate.placements = {
    {
        name = "Custom Temple Gate (Any Crystal)",
        data = {
            height = 48,
            theoMode = "Any",
            entityTypes = "TheoCrystal"
        }
    },
    {
        name = "Custom Temple Gate (All Crystals)",
        data = {
            height = 48,
            theoMode = "All",
            entityTypes = "TheoCrystal"
        }
    }
}

customTheoGate.fieldInformation = {
    theoMode = {
        options = { "Any", "All" },
        editable = false
    },
    height = {
        fieldType = "integer",
        minimumValue = 16
    },
    entityTypes = {
        fieldType = "string"
    }
}

customTheoGate.fieldOrder = {
    "x",
    "y",
    "height",
    "theoMode",
    "entityTypes"
}

customTheoGate.ignoredFields = {
    "width"
}

customTheoGate.minimumSize = { 8, 16 }
customTheoGate.resizable = { false, true }

function customTheoGate.sprite(room, entity)
    local sprites = {}

    local height = math.max(entity.height or 48, 16)

    -- Base gate sprite
    local sprite = drawableSprite.fromTexture(texture, entity)

    if sprite then
        sprite:setJustification(0.25, 0.0)

        -- Stretch vertically to match the entity height.
        -- The Theo gate texture is 48 px tall.
        sprite.scaleY = height / sprite.meta.height

        table.insert(sprites, sprite)
    end

    return sprites
end

function customTheoGate.selection(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    if sprite then
        sprite:setJustification(0.0, 0.0)

        local width = sprite.meta.width
        local height = math.max(entity.height or sprite.meta.height, 16)

        return utils.rectangle(
            entity.x,
            entity.y,
            width,
            height
        )
    end

    return utils.rectangle(
        entity.x,
        entity.y,
        15,
        math.max(entity.height or 48, 16)
    )
end

return customTheoGate