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
        name = "Custom Temple Gate (Any)",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "Any",
            entityTypes = "TheoCrystal",
            playerMode = "Ignored",
            closeOnNone = false,
            killDream = true
        }
    },
    {
        name = "Custom Temple Gate (All)",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "All",
            entityTypes = "TheoCrystal",
            playerMode = "Ignored",
            closeOnNone = false,
            killDream = true
        }
    },
    {
        name = "Custom Temple Gate (Each)",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "Each",
            entityTypes = "TheoCrystal",
            playerMode = "Ignored",
            closeOnNone = false,
            killDream = true
        }
    },
    {
        name = "Custom Temple Gate (None)",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "None",
            entityTypes = "TheoCrystal",
            playerMode = "Ignored",
            closeOnNone = false,
            killDream = true
        }
    }
}

customTheoGate.fieldInformation = {
    direction = {
        options = { "Down", "Up", "Left", "Right" },
        editable = false
    },
    theoMode = {
        options = { "Any", "All", "Each", "None" },
        editable = false
    },
    playerMode = {
        options = { "Ignored", "Required", "Repels" },
        editable = false
    },
    height = {
        fieldType = "integer",
        minimumValue = 16
    },
    entityTypes = {
        fieldType = "string"
    },
    closeOnNone = {
        fieldType = "boolean"
    },
    killDream = {
        fieldType = "boolean"
    }
}

customTheoGate.fieldOrder = {
    "x",
    "y",
    "height",
    "direction",
    "theoMode",
    "entityTypes",
    "closeOnNone",
    "killDream",
    "playerMode"
}

-- Prevent standard width handles from cluttering the UI
customTheoGate.ignoredFields = {
    "width"
}

customTheoGate.minimumSize = { 16, 48 }
customTheoGate.resizable = { false, true }

function customTheoGate.sprite(room, entity)
    local sprites = {}

    -- Treat "height" as the total extension of the gate in any direction
    local ext = math.max(entity.height or 48, 16)
    local direction = entity.direction or "Down"

    local sprite = drawableSprite.fromTexture(texture, entity)

    if sprite then
        sprite:setJustification(0.25, 0.0)

        -- Since the texture is natively vertical, scaleY always controls its length
        sprite.scaleY = ext / sprite.meta.height

        if direction == "Up" then
            sprite.rotation = math.pi
            sprite.x = sprite.x + 16
            sprite.y = sprite.y + ext
        elseif direction == "Right" then
            sprite.rotation = -math.pi / 2
            sprite.x = sprite.x + 0
            sprite.y = sprite.y + 16
        elseif direction == "Left" then
            sprite.rotation = math.pi / 2
            sprite.x = sprite.x + ext
        end

        table.insert(sprites, sprite)
    end

    return sprites
end

function customTheoGate.selection(room, entity)
    local ext = math.max(entity.height or 48, 16)
    local direction = entity.direction or "Down"

    -- Swap the bounding box dimensions based on rotation
    if direction == "Left" or direction == "Right" then
        return utils.rectangle(entity.x, entity.y, ext, 16)
    end

    return utils.rectangle(entity.x, entity.y, 16, ext)
end

return customTheoGate
