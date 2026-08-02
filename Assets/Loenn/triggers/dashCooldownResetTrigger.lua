local trigger = {}

trigger.name = "BalintHelper/DashCooldownSetTrigger"

trigger.placements = {
    {
        name = "main",
        data = {
            width  = 16,
            height = 16,

            value = 0.0,

            maxUses = 0,

            resetOnEnter = true,
            resetOnStay  = false,
            resetOnLeave = false,
        }
    }
}

trigger.fieldInformation = {
    value        = { fieldType = "number", minimumValue = 0.0, description = "Value to set the dash cooldown timer to" },
    maxUses      = { fieldType = "integer", minimumValue = 0, description = "Number of times the trigger can fire (0 = unlimited)" },
    resetOnEnter = { fieldType = "boolean", description = "Whether it should fire when entering the trigger" },
    resetOnStay  = { fieldType = "boolean", description = "Whether it should fire when staying inside the trigger" },
    resetOnLeave = { fieldType = "boolean", description = "Whether it should fire when leaving the trigger" },
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "value", 
    "maxUses",
    "resetOnEnter", "resetOnStay", "resetOnLeave",
}

return trigger