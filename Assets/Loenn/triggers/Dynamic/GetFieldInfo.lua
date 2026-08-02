local trigger = {}

trigger.name = "BalintHelper/GetFieldInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			fieldName = "",
			fieldType = "",
		}
	}
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the field."
	},
	fieldName = {
		fieldType = "string",
		description = "The name of the field to fetch."
	},
	fieldType = {
		fieldType = "string",
		description = "Optional. The field's type, used to disambiguate between fields with the same name."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "fieldName", "fieldType"
}

return trigger
