local trigger = {}

trigger.name = "BalintHelper/HoldableTimerSetTrigger"

trigger.placements = {
    {
        name = "Holdable Grab Timer Set Trigger",
        data = {
            width = 16,
            height = 16,

            value = 0.0,

            entityTypes = "TheoCrystal,ExtendedVariantMode/TheoCrystal",
            targetingMode = "Inside",

            playerTriggerMode = "Never",
            entityTriggerMode = "Never",
            global = false,
            onlyOnce = true,
            waitForSuccess = true,
        }
    }
}

local triggerModes = { "Never", "OnEntry", "OnLeave", "EntryOrLeave", "Stay" }
local targetingModes = { "Inside", "Outside", "Everywhere" }

trigger.fieldInformation = {
    value = {
        fieldType = "number",
        minimumValue = 0.0,
        description = "Value to set the cannotHoldTimer to"
    },
    entityTypes = {
        fieldType = "string",
        description = "Comma-separated type names or entity IDs"
    },
    targetingMode = {
        options = targetingModes,
        editable = false,
        description = "Which entities are affected when the trigger fires"
    },
    playerTriggerMode = {
        options = triggerModes,
        editable = false,
        description = "When the player interacting with the trigger fires it"
    },
    entityTriggerMode = {
        options = triggerModes,
        editable = false,
        description = "When a matching entity interacting with the trigger fires it"
    },
    global = {
        fieldType = "boolean",
        description = "If true, ignores interaction triggers and applies to targets every frame"
    },
    onlyOnce = {
        fieldType = "boolean",
        description = "If true, the trigger removes itself after activating"
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
    "targetingMode",
    "playerTriggerMode",
    "entityTriggerMode",
    "global",
    "onlyOnce",
    "waitForSuccess"
}

return trigger
