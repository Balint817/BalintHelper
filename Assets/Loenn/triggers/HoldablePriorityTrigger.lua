local trigger = {}

trigger.name = "BalintHelper/HoldablePriorityTrigger"

trigger.placements = {
    {
        name = "Holdable Priority Trigger",
        data = {
            width = 16,
            height = 16,

            mode = "LowestId",
            disableTheoFreeze = false,
        }
    }
}

local modes = {
    "LowestId",
    "HighestId",
    "Newest",
    "Oldest",
    "Closest",
    "Furthest",
    "ClosestFacing",
    "FurthestFacing",
}

trigger.fieldInformation = {
    mode = {
        options = modes,
        editable = false,
        description = table.concat({
            "How to choose between multiple valid holdables. Accepted values:",
            "LowestId = vanilla behavior.",
            "HighestId = reverse vanilla priority.",
            "Newest/Oldest = based on the controller's remembered grab order.",
            "Closest/Furthest = center of player hitbox to center of holdable hitbox.",
            "ClosestFacing/FurthestFacing = from the player's facing-side hitbox edge to the holdable center."
        }, "\n")
    },
    disableTheoFreeze = {
        fieldType = "boolean",
        description = "If enabled, always prefer the holdable the player is already supposed to be holding.\nThis prevents 'Theo Freeze'."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "mode",
    "disableTheoFreeze"
}

return trigger