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
            "Vector2",
            "Color",
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
		description = "The actual value to load. For Vector2, use format 'x;y'. For Color, use a hex color string."
	}
}

trigger.fieldOrder = {
    "x", "y", "width", "height", "type", "value"
}

return trigger