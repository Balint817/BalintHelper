local waveform = {}


waveform.name = "BalintHelper/MusicWaveform"


waveform.defaultData = {
    placement = "Bottom",
    color = "ffffff",
    height = 32.0,
    barCount = 64,
    barSpacing = 1.0,
    smoothing = 0.35,
    gain = 1.0,
    edgeOffset = 0.0,
    idleBehavior = "Ripple",
}


local placementValues = {
    "Top",
    "Bottom",
    "Both"
}


local idleBehaviorValues = {
    "Ripple",
    "Flat"
}


waveform.fieldInformation = {
    placement = {
        options = placementValues,
        editable = false,
        description = "Where the waveform is drawn.\nTop = mirrored horizontally and vertically off the top edge\nBottom = drawn growing up from the bottom edge\nBoth = draws both the top and bottom variants at once"
    },
    color = {
        fieldType = "color",
        description = "The color of the waveform bars."
    },
    height = {
        fieldType = "number",
        minimumValue = 1.0,
        description = "The maximum height in pixels a bar can reach at full amplitude."
    },
    barCount = {
        fieldType = "integer",
        minimumValue = 4,
        description = "The number of bars spanning the width of the camera."
    },
    barSpacing = {
        fieldType = "number",
        minimumValue = 0.0,
        description = "The gap in pixels between adjacent bars."
    },
    smoothing = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0,
        description = "How much bars ease toward their new amplitude each frame, from 0 (instant, jittery) to 1 (very smooth, laggy)."
    },
    gain = {
        fieldType = "number",
        minimumValue = 0.0,
        description = "Multiplier applied to the raw waveform amplitude before it's clamped and drawn."
    },
    edgeOffset = {
        fieldType = "number",
        description = "Pushes the waveform's baseline inward from the camera edge it's attached to, in pixels."
    },
    idleBehavior = {
        options = idleBehaviorValues,
        editable = false,
        description = "What the waveform does when no music is currently playing.\nRipple = plays a gentle idle wave animation\nFlat = bars stay flat/still"
    }
}


waveform.fieldOrder = {
    "placement",
    "color",
    "height",
    "barCount",
    "barSpacing",
    "smoothing",
    "gain",
    "edgeOffset",
    "idleBehavior",
}


return waveform