local trigger = {}

trigger.name = "BalintHelper/DeleteEntityTrigger"

trigger.placements = {
    {
        name = "Delete Entity Trigger",
        data = {
            width = 16,
            height = 16,

            entityTypes = "",
            targetingMode = "Inside",
            flag = ""
        }
    }
}

local targetingModes = { "Inside", "Outside", "Everywhere" }

trigger.fieldInformation = {
    entityTypes = {
        fieldType = "string",
        description = "Comma-separated type names or entity IDs. Leave empty to disable."
    },
    targetingMode = {
        options = targetingModes,
        editable = false,
        description = "Where to look for matching entities."
    },
    flag = {
        fieldType = "string",
        description = "Optional session flag check. Use flagName or !flagName, or leave empty to disable."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "entityTypes",
    "targetingMode",
    "flag"
}

return trigger