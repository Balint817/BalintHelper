local trigger = {}

trigger.name = "BalintHelper/CompareInstructionTrigger"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "Equals",
        }
    }
}

local enumValues = {

            "Equals",
            "NotEquals",
            "GreaterThan",
            "GreaterThanOrEquals",
            "LessThan",
            "LessThanOrEquals"
}

trigger.fieldInformation = {
    type = {
        options = enumValues,
        editable = false,
        description = "The operation to execute."
    }
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type"
}

-- TODO

return trigger