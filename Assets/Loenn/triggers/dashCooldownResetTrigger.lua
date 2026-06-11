-- Lönn plugin for BalintHelper/DashCooldownSetTrigger
-- Place this file at:
--   <your mod zip>/Loenn/triggers/dashCooldownSetTrigger.lua

local trigger = {}

trigger.name = "BalintHelper/DashCooldownSetTrigger"

trigger.placements = {
    {
        name = "default",
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
    value        = { fieldType = "number", minimumValue = 0.0 },
    resetOnEnter = { fieldType = "boolean" },
    resetOnStay  = { fieldType = "boolean" },
    resetOnLeave = { fieldType = "boolean" },
    maxUses      = { fieldType = "integer", minimumValue = 0 },
}

-- Field order in the inspector panel
trigger.fieldOrder = {
    "x", "y", "width", "height",
    "value", 
    "maxUses",
    "resetOnEnter", "resetOnStay", "resetOnLeave",
}

return trigger