local trigger = {}

trigger.name = "BalintHelper/TypedInstructionTrigger/IsTypeInstruction"

trigger.placements = {
    {
        name = "Instruction (Is Object Of Type)",
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