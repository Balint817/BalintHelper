local trigger = {}

trigger.name = "BalintHelper/GetConstructorInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			className = "",
			argumentTypes = "",
		}
	}
}

trigger.fieldInformation = {
	className = {
		fieldType = "string",
		description = "The name of the class that declares the constructor."
	},
	argumentTypes = {
		fieldType = "string",
		description = "Optional. A comma separated list of the constructor's parameter types."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "className", "argumentTypes"
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

return trigger
