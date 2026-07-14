local trigger = {}

trigger.name = "BalintHelper/CameraViewTrigger"

trigger.placements = {
    {
        name = "Camera View Trigger",
        data = {
            width = 16,
            height = 16,

            flag = "",
            triggerOnPlayer = false,
            onlyOnce = true,
            resetFlag = false
        }
    }
}

trigger.fieldInformation = {
    flag = {
        fieldType = "string",
        description = "Session flag to set when triggered. Required."
    },
    triggerOnPlayer = {
        fieldType = "boolean",
        description = "If true, entering the trigger with the player also activates it."
    },
    onlyOnce = {
        fieldType = "boolean",
        description = "If true, the trigger removes itself after firing once."
    },
    resetFlag = {
        fieldType = "boolean",
        description = "If true, the flag is cleared when the trigger is no longer active. If onlyOnce is true, this also delays the removal of the trigger until the flag is cleared."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "flag",
    "triggerOnPlayer",
    "onlyOnce",
    "resetFlag"
}

return trigger