local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/StructCopyInstruction"

trigger.placements = {
    {
        name = "Instruction (Copy Struct)",
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