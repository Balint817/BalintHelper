local trigger = {}

trigger.name = "BalintHelper/GetEntityInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			types = "",
		}
	}
}

trigger.fieldInformation = {
	types = {
		fieldType = "string",
		description = "A semicolon separated list of entity IDs and/or type names to filter for. Leave empty to match all entities."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "types"
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
