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

-- Enable resizing on both the X and Y axes
silentDebris.minimumSize = { 8, 8 }
silentDebris.resizable = { true, true }

-- Explicitly define the order of settings in the right-click properties menu
silentDebris.fieldOrder = {
    "x",
    "y",
    "width",
    "height"
}

-- Force Lönn to treat X and Y as numbers in the UI configuration
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

    -- Fill the resized zone with tiled 8x8 debris textures for visual clarity in the editor
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