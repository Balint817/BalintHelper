local trigger = {}

trigger.name = "BalintHelper/TypedInstructionTrigger/IsTypeInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "",
        }
    }
}

trigger.fieldInformation = {
    type = {
        fieldType = "string",
        description = "The target type to compare to."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

return trigger