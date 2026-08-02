local trigger = {}

trigger.name = "BalintHelper/SpawnEntityTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,

            entityName = "",
            triggerMode = "OnPlayerEntry",
            flag = ""
        }
    }
}

local triggerModes = { "OnPlayerEntry", "Automatically" }

trigger.fieldInformation = {
    entityName = {
        fieldType = "string",
        description = "Entity SID, type name, or full type name to spawn."
    },
    triggerMode = {
        options = triggerModes,
        editable = false,
        description = "When the trigger should spawn the entity."
    },
    flag = {
        fieldType = "string",
        description = "Optional session flag check. Use flagName or !flagName. Empty means always run."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "entityName",
    "triggerMode",
    "flag"
}

return trigger