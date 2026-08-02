local trigger = {}

trigger.name = "BalintHelper/TypeOfInstructionTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "Instruction (Typeof)",
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
        description = "The target runtime type instance to get."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

return trigger