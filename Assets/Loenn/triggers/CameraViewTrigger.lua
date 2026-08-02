local trigger = {}

trigger.name = "BalintHelper/CameraViewTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,

            flag = "",
            triggerOnPlayer = false,
            onlyOnce = true,
            resetFlag = false,
            needsBino = true
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
    },
    needsBino = {
        fieldType = "boolean",
        description = "If true, the trigger requires binoculars to be active. Does not affect 'triggerOnPlayer' setting."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "flag",
    "triggerOnPlayer",
    "onlyOnce",
    "resetFlag",
    "needsBino"
}

return trigger