local trigger = {}

trigger.name = "BalintHelper/AllowCurveOnCollideTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,

            flag = "",
            global = false
        }
    }
}

trigger.fieldInformation = {
    flag = {
        fieldType = "string",
        description = "Optional session flag check. Use flagName or !flagName. Empty means always run."
    },
    global = {
        fieldType = "boolean",
        description = "If true, the trigger will run globally instead of while the player is inside."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "flag",
    "global"
}

return trigger