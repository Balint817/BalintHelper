local utils = require("utils")

local entity = {}

entity.name = "BalintHelper/TemplateEntitySelector"
entity.depth = -13000

entity.texture = "loenn/BalintHelper/template/tinsert"

entity.placements = {
    name = "main",
    data = {
        target = "Celeste.Snowball",
        targetMode = "Type",
        runMode = "First",
        activeChannel = "",
        outputChannel = "",
        outputIncrement = 1.0,
    }
}

local targetModeValues = {
    "Type",
    "TypeVariable",
    "EntityVariable"
}

local runModeValues = {
    "First",
    "All"
}

entity.fieldInformation = {
    target = {
        fieldType = "string",
        description = "The target string to match against. Interpreted according to targetMode."
    },
    targetMode = {
        options = targetModeValues,
        editable = false,
        description = "How to interpret the target string.\nType = a hardcoded entity type\nTypeVariable = a variable that contains an entity type (or list of types)\nEntityVariable = a variable that contains an entity instance (or list of entities)"
    },
    runMode = {
        options = runModeValues,
        editable = false,
        description = "How many entities can be added to the template.\nFirst = only the first entity\nAll = all entities every frame"
    },
    activeChannel = {
        fieldType = "string",
        description = "If provided, new entities will only be added to the template when this channel is active."
    },
    outputChannel = {
        fieldType = "string",
        description = "If provided, this channel will be incremented whenever a new entity is added to the template."
    },
    outputIncrement = {
        fieldType = "number",
        description = "The amount to increment the output channel by whenever a new entity is added to the template."
    }
}

function entity.selection(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 6, 12, 12)
end

return entity