local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/DupInstruction"

trigger.placements = {
    {
        name = "Instruction (Duplicate)",
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