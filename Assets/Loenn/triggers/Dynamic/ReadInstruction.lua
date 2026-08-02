local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/ReadInstruction"

trigger.placements = {
    {
        name = "Instruction (Read)",
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