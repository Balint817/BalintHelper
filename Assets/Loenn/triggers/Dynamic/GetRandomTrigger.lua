local trigger = {}

trigger.name = "BalintHelper/GetRandomTrigger/NopInstruction"

trigger.placements = {
	{
		name = "Instruction (Get Random)",
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
