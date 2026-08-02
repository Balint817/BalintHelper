local trigger = {}

trigger.name = "BalintHelper/TypedInstructionTrigger/ReadPointerInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "Instruction (Read Pointer)",
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
        description = "The type to read the pointer as."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

return trigger