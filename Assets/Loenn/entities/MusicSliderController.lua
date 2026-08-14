local entity = {}

entity.name = "BalintHelper/MusicSliderController"

entity.placements = {
    {
        name = "main",
        data = {
            sliderName = "music_progress",
            musicName = "",
            mode = "Time",
        }
    }
}

local sliderModes = { "Percentage", "Time" }

entity.fieldInformation = {
    sliderName = {
        fieldType = "string",
        description = "Name of the session slider to write the music position into"
    },
    musicName = {
        fieldType = "string",
        description = "FMOD event path to match against (e.g. event:/music/lvl1/main). Leave blank to track whatever music is currently playing"
    },
    mode = {
        options = sliderModes,
        editable = false,
        description = "Percentage: 0.0 to 1.0 progress through the track.\nTime: elapsed seconds"
    }
}

entity.fieldOrder = {
    "x", "y",
    "sliderName",
    "musicName",
    "mode",
}

entity.texture = "loenn/BalintHelper/musicslider"

entity.justification = { 0.5, 0.5 }

return entity