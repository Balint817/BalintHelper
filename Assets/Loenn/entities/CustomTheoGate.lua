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
        name = "any",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "Any",
            entityTypes = "TheoCrystal;ExtendedVariantMode/TheoCrystal",
            closeOnNone = false,
            killDream = true,
            playerMode = "Ignored",
            spriteId = "templegate_theo",
            outputFlag = ""
        }
    },
    {
        name = "all",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "All",
            entityTypes = "TheoCrystal;ExtendedVariantMode/TheoCrystal",
            closeOnNone = false,
            killDream = true,
            playerMode = "Ignored",
            spriteId = "templegate_theo",
            outputFlag = ""
        }
    },
    {
        name = "each",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "Each",
            entityTypes = "TheoCrystal;ExtendedVariantMode/TheoCrystal",
            closeOnNone = false,
            killDream = true,
            playerMode = "Ignored",
            spriteId = "templegate_theo",
            outputFlag = ""
        }
    },
    {
        name = "none",
        data = {
            height = 48,
            direction = "Down",
            theoMode = "None",
            entityTypes = "TheoCrystal;ExtendedVariantMode/TheoCrystal",
            closeOnNone = false,
            killDream = true,
            playerMode = "Ignored",
            spriteId = "templegate_theo",
            outputFlag = ""
        }
    }
}

customTheoGate.fieldInformation = {
    height = {
        fieldType = "integer",
        minimumValue = 16
    },
    direction = {
        options = { "Down", "Up", "Left", "Right" },
        editable = false
    },
    theoMode = {
        description = "How many nearby matches should be required to open the gate.\nAny: At least one match\nAll: All valid entities\nEach: One of each entity\nNone: Closes when any is near the gate.",
        options = { "Any", "All", "Each", "None" },
        editable = false
    },
    entityTypes = {
        description = "Comma-separated type names or entity IDs to match against nearby entities.",
        fieldType = "string"
    },
    closeOnNone = {
        description = "If true, the gate will close when no matching entities are present in the current room (for example, by being thrown out of the room)",
        fieldType = "boolean"
    },
    killDream = {
        description = "If true, the gate will kill the player if they collide with it in a dream block.",
        fieldType = "boolean"
    },
    playerMode = {
        description = "How the player should be treated when they approach the gate.\nIgnored: The player is not required to open the gate.\nRequired: Besides the entities, the player is also required to be nearby to open the gate.\nRepels: The gate closes if the player is nearby.",
        options = { "Ignored", "Required", "Repels" },
        editable = false
    },
    spriteId = {
        description = "The sprite ID to use for the gate.",
        fieldType = "string"
    },
    outputFlag = {
        description = "The session flag to set when the gate opens. If empty, no flag is set.",
        fieldType = "string"
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
    "playerMode",
    "spriteId",
    "outputFlag"
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
