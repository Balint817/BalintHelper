local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/WriteInstruction"

trigger.placements = {
    {
        name = "Instruction (Write)",
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