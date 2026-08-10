local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local entity = {}

entity.name = "BalintHelper/TemplateEntitySelector"
entity.depth = -13000

entity.nodeLimits = {1, 1}
entity.nodeLineRenderType = "line"

entity.nodeVisibility = "always"

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
    local nodeRects = {}
    for _, node in ipairs(entity.nodes or {}) do
        table.insert(nodeRects, utils.rectangle(node.x - 4, node.y - 16, 8, 8))
    end
    return utils.rectangle(entity.x - 6, entity.y - 6, 12, 12), nodeRects
end

function entity.nodeSprite(room, entity, node, nodeIndex, viewport)
    if viewport == nil then return {} end --bad. bad bad bad

    local somethingAtNode = false
    for _, e in ipairs(room.entities) do
        if e ~= entity and e.x == node.x and e.y == node.y and string.find(e._name, "auspicioushelper", 1, true) then
            somethingAtNode = true
            break
        end
    end

    if somethingAtNode then
        return drawableSprite.fromTexture("loenn/BalintHelper/template/tinsertnode", {
            x = node.x, y = node.y - 3,
            depth = -13001,
        })
    end

    return {
        drawableText.fromText("No matches", node.x - 20, node.y - 33, 40, 18, nil, nil, "ff4444"),
        drawableSprite.fromTexture("loenn/BalintHelper/template/tinsertnode", {
            x = node.x, y = node.y - 3,
            color = "ff4444",
            depth = -13001,
        }),
    }
end

return entity