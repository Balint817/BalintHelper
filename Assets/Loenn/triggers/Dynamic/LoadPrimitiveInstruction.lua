local trigger = {}

trigger.name = "BalintHelper/LoadPrimitiveInstructionTrigger/LoadConstantInstruction"

trigger.placements = {
    {
        name = "Instruction (Load Primitive)",
        data = {
            width = 16,
            height = 16,
			type = "Bool",
			value = "true"
        }
    }
}

local enumValues = {
            "Bool",
            "Byte",
            "SByte",
            "Int16",
            "Int32",
            "Int64",
            "UInt16",
            "UInt32",
            "UInt64",
            "Decimal",
            "Double",
            "Float",
            "NativeInt",
            "NativeUInt",
            "Char",
            "String",
            "Null"
}

trigger.fieldInformation = {
    type = {
        options = enumValues,
        editable = false,
        description = "The type of the constant."
    },
	value = {
	    fieldType = "string",
		description = "The actual value to load."
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type", "value"
}

return trigger