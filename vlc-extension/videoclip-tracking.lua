function descriptor()
    return {
        title = "Video Clip Tracking",
        version = "1.0",
        author = "Rúni Vørmadal",
        url = 'http://www.videolan.org',
        shortdesc = "Track video clips",
        description = "Create and name scenes within a video for easy navigation and enable user to tag clips, and search clips across files using the web interface.",
        capabilities = {}
    }
end

function meta_changed()
    -- Handle metadata changes if needed
    dbg("Metadata changed")
end

local current_clip = nil
local dialog, list, status_label, message_label
local clips = {}

local function dbg(msg)
    vlc.msg.dbg("[Video Clip Tracking] " .. msg)
end

local function err(msg)
    vlc.msg.err("[Video Clip Tracking] " .. msg)
end

local function fmt_time(msec)
    local sec = msec / 1000
    return string.format("%02d:%02d:%02d", sec / 3600, (sec / 60) % 60, sec % 60)
end

-- inspired by https://github.com/rameessahlu/vlc-scene-navigator/blob/main/scene_navigator.lua
local function get_output_filename()
    local item = vlc.input.item()
    if item then
        local uri = item:uri() or ""
        local path = vlc.strings.decode_uri(uri):gsub("^file://", "")

        -- Normalize for Windows drive letters (e.g., /C:/...) only if needed
        if path:match("^/[A-Z]:/") then
            path = path:sub(2)
        end

        if path ~= "" then
            return path:gsub("%.[^%.\\/]+$", ".csv")
        end
    end
    return nil
end

-- update the clips list and and list
local function add_clip(from, to, name)
    -- insert and reorder list by start time
    table.insert(clips, {
        start = from,
        to = to,
        name = name
    })
    table.sort(clips, function(a, b)
        return a.start < b.start
    end)
end

-- load clips from a CSV file
local function load_clips_from_file(filename)
    if not filename then
        dbg("No output filename available to load clips from.")
        return
    end
    -- check if file exists
    local file = io.open(filename, "r")
    if not file then
        dbg("No existing clip file found: " .. filename)
        return
    end
    local iterator = io.lines(filename)
    if not iterator then
        err("Failed to open file: " .. filename)
        return
    end
    -- read each line as a row: start_time_ms,to_time_ms,clip_name
    for line in iterator do
        local start_str, to_str, name = line:match("^(%d+);(%d+);(.+)$")
        if start_str and to_str and name then
            local start_time = tonumber(start_str)
            local to_time = tonumber(to_str)
            add_clip(start_time, to_time, name)
        else
            err("Invalid line format: " .. line)
        end
    end
end

local function write_clips_to_file(filename)
    local file = io.open(filename, "w")
    if not file then
        err("Failed to open file for writing: " .. filename)
        return
    end
    for _, c in ipairs(clips) do
        file:write(string.format("%d;%d;%s\n", c.start, c.to, c.name))
    end
    file:close()
end

local function update_ui()
    if not dialog or not list then
        return
    end

    write_clips_to_file(get_output_filename() or "clips.csv")
    list:clear()
    for _, c in ipairs(clips) do
        list:add_value(string.format("%s - %s %s", fmt_time(c.start), fmt_time(c.to), c.name), _)
    end

end

local function clip()
    if vlc.playlist.status() == "playing" then
        vlc.playlist.pause()
        if current_clip then
            message_label:set_text("Click again to end clip at current time.")
        end
        return
    end

    -- check if any existing clip in clips starts or ends within 1 second of current time
    local input = vlc.object.input()
    if not input then
        return
    end
    local current_time = vlc.var.get(input, "time") / 1000 -- convert to milliseconds

    if not current_clip then
        current_clip = {
            from = current_time,
            to = nil
        }
        status_label:set_text("Clip started at " .. fmt_time(current_time))
        vlc.playlist.play()

        return
    end

    for _, c in ipairs(clips) do
        if (c.start and math.abs(c.start - current_time) < 1000) or (c.to and math.abs(c.to - current_time) < 1000) then
            vlc.playlist.play()
            return
        end
    end

    add_clip(current_clip.from, current_time, "Clip " .. (#clips + 1))
    status_label:set_text("")
    message_label:set_text("")
    current_clip = nil
    update_ui()
end

-- function to rewind or fast forward by a given number of seconds
local function seek(seconds)
    return function()
        local input = vlc.object.input()
        if not input then
            return
        end

        local current_time = vlc.var.get(input, "time")
        dbg("Seeking from " .. current_time .. " by " .. seconds .. " seconds")
        vlc.var.set(input, "time", current_time + seconds * 1000 * 1000)
    end
end

function activate()
    if dialog then
        return
    end

    dbg("Video Clip Tracking extension activated")
    dialog = vlc.dialog("Video Clip Tracking")
    -- default show one clip that starts at 00.00 and ends at the end of the video
    local input = vlc.object.input()
    local start_time = 0
    local end_time = 0
    if input then
        end_time = vlc.var.get(input, "length") / 1000 -- in milliseconds
    end
    local clip_name = "Clip 1"

    -- rewind 1 second button
    dialog:add_button("⏪ 1s", seek(-1), 1, 1, 1, 1)
    -- rewind 0.5 second button
    dialog:add_button("⏪ 0.5s", seek(-0.5), 2, 1, 1, 1)
    -- utf-8 character for play and pause button and cut/clip
    dialog:add_button("▶/⏸️/✂️", clip, 3, 1, 1, 1)
    dialog:add_button("▶", function()
        vlc.playlist.play()
        message_label:set_text("")
    end, 4, 1, 1, 1)
    -- fast forward 0.5 second button
    dialog:add_button("0.5s ⏩", seek(0.5), 5, 1, 1, 1)
    -- fast forward 1 second button
    dialog:add_button("1s ⏩", seek(1), 6, 1, 1, 1)

    status_label = dialog:add_label("", 1, 2, 2, 1)
    message_label = dialog:add_label("", 3, 2, 3, 1)
    list = dialog:add_list(1, 3, 6, 12)
    if list.set_callback then
        list:set_callback(nil)
    end

    dialog:add_label("Set clip name for selected clip:", 7, 3)
    local nameInput = dialog:add_text_input("", 7, 4)
    dialog:add_button("Set Clip Name", function()
        local selection = list:get_selection()

        if selection and next(selection) then
            clips[next(selection)].name = nameInput:get_text()
            update_ui()
            nameInput:set_text("")
        end
    end, 7, 5, 1, 1)

    dialog:add_button("Delete selected clip", function()
        local selection = list:get_selection()

        if selection and next(selection) then
            table.remove(clips, next(selection))
            update_ui()
        end
    end, 7, 6, 1, 1)

    load_clips_from_file(get_output_filename())
    update_ui()
end

function deactivate()
    dbg("Video Clip Tracking extension deactivated")
    if dialog then
        dialog:delete()
        dialog = nil
    end
end

function close()
    dbg("Video Clip Tracking extension closed")
    vlc.deactivate()
end
