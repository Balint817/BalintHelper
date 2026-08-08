local trigger = {}

trigger.name = "BalintHelper/ArithmeticInstructionTrigger"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			type = "Add",
        }
    }
}

local enumValues = {

            "Add",
            "Subtract",
            "Multiply",
            "Divide",
            "Modulo",
            "Increment",
            "Decrement",
            "Negate",
            "BitwiseAnd",
            "BitwiseOr",
            "BitwiseXor",
            "LeftShift",
            "RightShift",
            "Complement",
            "Plus",
            "Not",
            "IndexFromEnd",
            "IndexRange"
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

trigger.triggerText = function(room, trigger)
    return "Arithmetic (" .. trigger.type .. ")"
end

return trigger