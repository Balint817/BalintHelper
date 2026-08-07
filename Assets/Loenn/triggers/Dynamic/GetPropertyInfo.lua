local trigger = {}

trigger.name = "BalintHelper/GetPropertyInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			propertyName = "",
			returnType = "",
			indexerTypes = "",
			action = "Raw"
		}
	}
}


local actionValues = {
            "Raw",
            "Read",
            "ReadIndexer",
            "Write",
            "WriteIndexer",
            "Invoke"
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the property."
	},
	propertyName = {
		fieldType = "string",
		description = "The name of the property to fetch."
	},
	returnType = {
		fieldType = "string",
		description = "Optional. The property's type, used to disambiguate between properties with the same name."
	},
	indexerTypes = {
		fieldType = "string",
		description = "Optional. A semicolon separated list of the property's indexer parameter types, if it is an indexer."
	},
	action = {
		options = actionValues,
		editable = false,
		description = "The action to perform on the constructor."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "propertyName", "returnType", "indexerTypes", "action"
}

local languageRegistry = require("language_registry")

trigger.triggerText = function(room, trigger)
    local language = languageRegistry.getLanguage()
    local result = language.triggers[trigger._name].placements.name.main

    if result._exists then
        return tostring(result)
    else
        return trigger._name
    end
end

-- TODO

return trigger
