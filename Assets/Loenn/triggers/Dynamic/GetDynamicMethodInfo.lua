local trigger = {}

trigger.name = "BalintHelper/GetDynamicMethodInfoTrigger/LoadConstantInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
	{
		name = "main",
		data = {
			width = 16,
			height = 16,
			name = "",
		}
	}
}

trigger.fieldInformation = {
	name = {
		fieldType = "string",
		description = "The name of the dynamic method to fetch."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height", "name"
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
