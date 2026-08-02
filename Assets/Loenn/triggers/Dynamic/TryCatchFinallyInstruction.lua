local trigger = {}

trigger.name = "BalintHelper/TryCatchFinallyInstructionTrigger/TryCatchFinallyInstruction"

trigger.placements = {
    {
        name = "Instruction (Try/Catch/Finally)",
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

return trigger