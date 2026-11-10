local STREAM_KEY = "Blocks"
local OUTPUT_PATH = "F:/Mini World Items/Blocks.json"

local START_ID = 1
local END_ID = 600000

local SCAN_PER_FRAME = 5000    -- Number of empty IDs to scan per frame
local ITEMS_PER_FRAME = 20     -- Yield frame after printing 20 valid items to prevent I/O buffer overflow
local CLEAR_LOG_INTERVAL = 200 -- Truncate TriggerScript.log every 200 valid items
local REQUIRE_DESC = true      -- Set to false to include blocks/items with empty descriptions

-- 1. Items:
-- local getDefName = function(id) return Item:getItemName(id) end
-- local getDefDesc = function(id) return Item:getItemDesc(id) end

-- 2. Blocks:
local getDefName = function(id) return Block:GetBlockDefName(id) end
local getDefDesc = function(id) return Block:GetBlockDefDesc(id) end

-- 3. Creatures:
-- local getDefName = function(id) return Creature:GetMonsterDefName(id) end
-- local getDefDesc = function(id) return Creature:GetMonsterDefDesc(id) end

local gsub = string.gsub
local format = string.format
local tostring = tostring

local function escapeJSON(str)
    if not str then return "" end
    str = tostring(str)
    str = gsub(str, "\\", "\\\\")
    str = gsub(str, "\"", "\\\"")
    str = gsub(str, "\n", "\\n")
    str = gsub(str, "\r", "")
    return str
end

local function RunLosslessScraper()
    print(format("[Script] Starting data extraction for [%s]...", STREAM_KEY))
    print(format("@@OPEN_JSON|%s|%s", STREAM_KEY, OUTPUT_PATH))

    local scanCounter = 0
    local framePrintCounter = 0
    local validCount = 0

    for id = START_ID, END_ID do
        local resName, name = getDefName(id)

        if resName == 0 and name and name ~= "" and name ~= "nil" and name ~= "Phiên bản game quá cũ" then
            local resDesc, desc = getDefDesc(id)
            local safeDesc = (resDesc == 0 and desc) and desc or ""

            if (not REQUIRE_DESC) or (safeDesc ~= "") then
                validCount = validCount + 1
                framePrintCounter = framePrintCounter + 1

                local jsonPayload = format('@@JSON_BATCH|%s|{"id": %d, "name": "%s", "desc": "%s"}', 
                    STREAM_KEY, id, escapeJSON(name), escapeJSON(safeDesc))
                print(jsonPayload)

                if framePrintCounter >= ITEMS_PER_FRAME then
                    threadpool:wait(0)
                    framePrintCounter = 0
                    scanCounter = 0
                end

                if validCount % CLEAR_LOG_INTERVAL == 0 then
                    print(format("@@PROG|%d|%d|%d", id, END_ID, validCount))
                    threadpool:wait(0.1)
                    print("@@CLEAR_LOG")
                    threadpool:wait(0.2)
                    scanCounter = 0
                    framePrintCounter = 0
                end
            end
        end

        scanCounter = scanCounter + 1

        if scanCounter >= SCAN_PER_FRAME then
            print(format("@@PROG|%d|%d|%d", id, END_ID, validCount))
            threadpool:wait(0)
            scanCounter = 0
            framePrintCounter = 0
        end
    end

    threadpool:wait(0.2)
    print(format("@@PROG|%d|%d|%d|FORCE", END_ID, END_ID, validCount))
    print(format("@@CLOSE_JSON|%s", STREAM_KEY))
    threadpool:wait(0.2)
    print("@@CLEAR_LOG")
    print(format("[Script] Extraction completed! Total valid records: %d.", validCount))
end

threadpool:work(RunLosslessScraper)