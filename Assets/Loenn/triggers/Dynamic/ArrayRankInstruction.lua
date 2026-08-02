local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/ArrayRankInstruction"

trigger.placements = {
    {
        name = "Instruction (Get Array Rank)",
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