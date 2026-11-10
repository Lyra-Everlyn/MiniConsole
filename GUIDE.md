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
  print("@@OPEN_JSON|ITEMS|F:\\Mini World Items\\Items.json")