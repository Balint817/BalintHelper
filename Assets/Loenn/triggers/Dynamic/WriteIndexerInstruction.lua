local trigger = {}

trigger.name = "BalintHelper/BaseInstructionTrigger/WriteIndexerInstruction"

trigger.placements = {
    {
        name = "Instruction (Write Indexer)",
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