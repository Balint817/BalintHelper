local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local silentDebris = {}

silentDebris.name = "BalintHelper/SilentFloatingDebris"
silentDebris.depth = -5

local texture = "scenery/debris"

silentDebris.placements = {
    {
        name = "Silent Floating Debris",
        data = {
            width = 16,
            height = 16
        }
    }
}

silentDebris.minimumSize = { 8, 8 }
silentDebris.resizable = { true, true }

silentDebris.fieldOrder = {
    "x",
    "y",
    "width",
    "height"
}

silentDebris.fieldInformation = {
    x = {
        fieldType = "integer"
    },
    y = {
        fieldType = "integer"
    }
}

function silentDebris.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 16
    local height = entity.height or 16

    for x = 0, width - 1, 8 do
        for y = 0, height - 1, 8 do
            local sprite = drawableSprite.fromTexture(texture, entity)
            if sprite then
                sprite:useNewQuad(0, 0, 8, 8)
                sprite:setPosition(entity.x + x, entity.y + y)
                sprite:setJustification(0.0, 0.0)
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

function silentDebris.selection(room, entity)
    return utils.rectangle(
        entity.x,
        entity.y,
        entity.width or 16,
        entity.height or 16
    )
end

return silentDebris