# Lua IPC Protocol & Usage Guide

This guide explains how to communicate with the **MiniConsole** directly from your in-game Mini World Lua scripts. Because the Mini World Lua environment runs inside a restricted sandbox without direct `io.open` access, this engine uses `TriggerScript.log` as an Inter-Process Communication (IPC) channel via the `@@` command prefix.

---

## 1. Quick Start Workflow

1. **Launch the Engine**: Start `MiniConsole.exe` first. Verify that it successfully detects your `TriggerScript.log` file.
2. **Load the Lua Script**: Copy your scraping or logging script into a Mini World map (Developer Mode -> Script Editing).
3. **Run inside Threadpool**: Always execute heavy loops inside `threadpool:work()` so `threadpool:wait()` can yield execution back to the game engine.
4. **Enter Play Mode**: Start the map. The C# console will automatically intercept `@@` commands, write data to disk, and render the live progress bar.

---

## 2. IPC Command Reference

All engine commands must start with the `@@` prefix and use the pipe character (`|`) as a parameter delimiter.

| Command | Format | Description |
| :--- | :--- | :--- |
| **Open JSON** | `@@OPEN_JSON\|<Key>\|<FullPath>` | Creates a new JSON file and writes the opening `[` bracket. |
| **Write JSON** | `@@JSON_BATCH\|<Key>\|{...}~~{...}` | Appends one or more JSON objects separated by `~~`. |
| **Progress** | `@@PROG\|<Current>\|<Total>\|<Found>` | Updates the console progress bar and window title. |
| **Clear Log** | `@@CLEAR_LOG` | Truncates `TriggerScript.log` to 0 bytes to free game memory. |
| **Close JSON** | `@@CLOSE_JSON\|<Key>` | Writes the closing `]` bracket and flushes the file to disk. |

---

## 3. Detailed Command Breakdown

### 3.1. `@@OPEN_JSON` — Initialize a JSON Stream
Opens or overwrites a target `.json` file on your hard drive and associates it with a unique stream identifier (`<Key>`).

* **Syntax**: `@@OPEN_JSON|<Key>|<FullPath>`
* **Parameters**:
  * `<Key>`: A case-insensitive identifier for the stream (e.g., `ITEMS`, `BLOCKS`, `MOBS`).
  * `<FullPath>`: The absolute Windows file path. Remember to escape backslashes (`\\`) inside Lua strings.
* **Example**:
  ```lua
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
```
