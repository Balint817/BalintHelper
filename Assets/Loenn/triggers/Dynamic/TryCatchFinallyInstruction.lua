local trigger = {}

trigger.name = "BalintHelper/TryCatchFinallyInstructionTrigger/TryCatchFinallyInstruction"

trigger.nodeLimits = {1, -1}

trigger.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16,
			tryMethodName = "",
			catchMethodName = "",
			finallyMethodName = "",
        }
    }
}

trigger.fieldInformation = {
	tryMethodName = {
		fieldType = "string",
		description = "The dynamic method's name to execute in the try block"
	},
	catchMethodName = {
		fieldType = "string",
		description = "The dynamic method's name to execute in the catch block, optional."
	},
	finallyMethodName = {
		fieldType = "string",
		description = "The dynamic method's name to execute in the finally block, optional."
	}
}

trigger.fieldOrder = {
	"x", "y", "width", "height",
	"tryMethodName", "catchMethodName", "finallyMethodName"
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