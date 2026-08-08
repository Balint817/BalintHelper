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

trigger.triggerText = function(room, trigger)
	local finalTriggerText = "Try (" .. trigger.tryMethodName .. ")"
	if trigger.catchMethodName ~= "" then
		finalTriggerText = finalTriggerText .. " Catch (" .. trigger.catchMethodName .. ")"
	end
	if trigger.finallyMethodName ~= "" then
		finalTriggerText = finalTriggerText .. " Finally (" .. trigger.finallyMethodName .. ")"
	end
	return finalTriggerText
end

return trigger