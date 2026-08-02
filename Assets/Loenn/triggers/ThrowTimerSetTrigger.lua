local trigger = {}

trigger.name = "BalintHelper/ThrowTimerSetTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,

            value = 0.0,

            entityTypes = "TheoCrystal,ExtendedVariantMode/TheoCrystal",

            playerTriggerMode = "Stay",
            onlyOnce = true,
            waitForSuccess = true,
        }
    }
}

local triggerModes = { "OnEntry", "OnLeave", "EntryOrLeave", "Stay" }

trigger.fieldInformation = {
    value = {
        fieldType = "number",
        description = "Value to set the player's minHoldTimer to"
    },
    entityTypes = {
        fieldType = "string",
        description = "Comma-separated type names or entity IDs to match against the held entity"
    },
    playerTriggerMode = {
        options = triggerModes,
        editable = false,
        description = "When the trigger fires"
    },
    onlyOnce = {
        fieldType = "boolean",
        description = "If true, the trigger removes itself after firing"
    },
    waitForSuccess = {
        fieldType = "boolean",
        description = "If true, onlyOnce is only counted if at least one target was affected"
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "value",
    "entityTypes",
    "playerTriggerMode",
    "onlyOnce",
    "waitForSuccess"
}

return trigger