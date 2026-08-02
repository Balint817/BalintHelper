local trigger = {}

trigger.name = "BalintHelper/ConditionalInstructionTrigger/ConditionalInstruction"

trigger.nodeLimits = {2, -1}

trigger.placements = {
	{
		name = "Instruction (Conditional)",
		data = {
			width = 16,
			height = 16,
		}
	}
}


trigger.fieldOrder = {
	"x", "y", "width", "height"
}

return trigger
