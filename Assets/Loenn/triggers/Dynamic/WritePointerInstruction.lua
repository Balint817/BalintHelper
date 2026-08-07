local trigger = {}

trigger.name = "BalintHelper/TypedInstructionTrigger/WritePointerInstruction"

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
        description = "The type to write the pointer as."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

local languageRegistry = require("language_registry")

trigger.triggerText = function(room, trigger)
    return "Write Pointer (" .. trigger.type .. ")"
end

return trigger