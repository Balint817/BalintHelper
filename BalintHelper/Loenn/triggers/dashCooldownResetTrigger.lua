-- Lönn plugin for BalintHelper/DashCooldownSetTrigger
-- Place this file at:
--   <your mod zip>/Loenn/triggers/dashCooldownSetTrigger.lua

local trigger = {}

trigger.name = "BalintHelper/DashCooldownSetTrigger"

trigger.placements = {
    {
        name = "Dash Cooldown Set Trigger",
        data = {
            -- Trigger dimensions (Lönn standard)
            width  = 16,
            height = 16,

            -- Value to set the dash cooldown timer to
            value = 0.0,

            -- Trigger behavior flags
            resetOnEnter = true,
            resetOnStay  = false,
            resetOnLeave = false,

            -- If true, the trigger can only fire a limited number of times
            -- (0 = unlimited, resets on room revisit)
            maxUses = 0,
        }
    }
}

-- Human-readable field types shown in Lönn
trigger.fieldInformation = {
    value        = { fieldType = "number", minimumValue = 0.0, description = "Value to set the dash cooldown timer to" },
    resetOnEnter = { fieldType = "boolean", description = "Whether it should fire when entering the trigger" },
    resetOnStay  = { fieldType = "boolean", description = "Whether it should fire when staying inside the trigger" },
    resetOnLeave = { fieldType = "boolean", description = "Whether it should fire when leaving the trigger" },
    maxUses      = { fieldType = "integer", minimumValue = 0, description = "Number of times the trigger can fire (0 = unlimited)" },
}

-- Field order in the inspector panel
trigger.fieldOrder = {
    "x", "y", "width", "height",
    "value", 
    "maxUses",
    "resetOnEnter", "resetOnStay", "resetOnLeave",
}

return trigger