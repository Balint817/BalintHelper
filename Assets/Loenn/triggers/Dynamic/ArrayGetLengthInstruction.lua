local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/ArrayGetLengthInstruction"

trigger.placements = {
    {
        name = "Instruction (Get Array Rank Length)",
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