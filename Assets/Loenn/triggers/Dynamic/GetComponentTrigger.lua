local trigger = {}

trigger.name = "BalintHelper/GetComponentTrigger/NopInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			componentType = "",
		}
	}
}

trigger.fieldInformation = {
	componentType = {
		fieldType = "string",
		description = "The fully qualified type name of the component to fetch from the entity currently on top of the stack."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "componentType"
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
