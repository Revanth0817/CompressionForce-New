# 📘 PLC Signal Configuration – README

This README focuses on **`signals.json`**.

## 🧠 Key Concepts (Very Important)

Before editing `signals.json`, understand these concepts:
### 1️ Physical Signal (PLC Address)
A **PLC address** is the physical truth.
Example:
`InputRegister 0, length 2`
This represents **one real value in the PLC**.

### 2️ Primary Signal (Canonical Owner)
A **primary signal** is the **only signal that owns a PLC address**.
- Only primary signals are polled
- Only primary signals write to the cache
- Only one primary is allowed per PLC address
> 🔒 **Rule: One PLC address → exactly one primary signal**

### 🔹 About isPrimary (Important)
`isPrimary` is optional in `signals.json`

The system applies the following default rule:
- If a signal defines an address, it is treated as primary
- If a signal does NOT define an address, it is treated as an alias
- You may still explicitly set:
`"isPrimary": true`
or
`"isPrimary": false`
but this is not required as long as the address rules are followed.

⚠️ Startup validation will still enforce:
- Only one primary per PLC address
- Aliases must NOT define addresses

### 3️ Aliases (Alternate Names)
Aliases are **different names for the same physical signal**.
They:
- Do NOT define addresses
- Do NOT participate in polling
- Do NOT store separate values
They simply resolve to the primary signal.

Example use cases:
- Different screen names
- Different functional meanings
- Legacy naming support

Aliases always inherit the groups of their primary signal and cannot belong to different groups.

### 4️ Groups (Context / UI / Pages)
Groups define **where or why a signal is used**, NOT how it is read.
Examples:
- Pages (`MainPage`, `SettingsPage`)
- Categories (`Analog_Inputs`, `Digital_Inputs`)
- Features (`AutoTare`, `Safety`)

Groups:
- Can be multiple per signal
- Do NOT affect polling
- Are used by UI and SignalR later

### 4️A Roles (Different Meaning, Same Physical Value)
Sometimes the **same PLC value is used for different purposes in different contexts**.
Examples:
- Display value on MainPage
- Control reference in AutoMode
- Diagnostic value in Maintenance
⚠️ In this case, **aliases MUST NOT be used**.
#### Why aliases are NOT enough
Aliases only provide **different names for the same meaning**.  
They cannot represent **different semantic roles**.
### ✅ Correct approach for different meanings
Use **roles** on top of a primary signal.
* The **primary signal** owns the PLC address and polling.
* **Roles** represent different logical meanings of the same value.
* Each role can belong to different groups (pages/features).

⚠️ Important
* Roles do NOT define PLC addresses.
* Roles do NOT participate in polling.
* Roles always share the same underlying value as their primary signal.
* Roles are logical views, not physical signals.

#### Example
```json
{
  "signalId": "S1_Main_Loadcell",
  "isPrimary": true,
  "description": "Main compression loadcell",
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0",
    "length": 2
  },
  "groups": ["MainPage"],

  "aliases": ["Compression_Force"],

  "roles": {
    "AutoMode_Force": {
      "description": "Force reference used in Auto mode",
      "groups": ["AutoMode"]
    }
  }
}
```

### 5️ Description (Human-readable)
`description` is for:
- Documentation
- Tooltips
- UI labels
- Maintenance clarity
It has **no technical effect**.

        ----------------------------------------------------------------------------------------------------

## 📄 `signals.json` – Structure

### Basic example (Primary Signal)
```json
{
  "signalId": "S1_Main_Loadcell",
  "description": "Main compression loadcell",
  "dataType": "Double",
  "updateClass": "Critical",
  "isPrimary": true,
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0",
    "length": 2
  },
  "groups": ["Analog_Inputs", "MainPage"],
  "aliases": ["Compression_Force", "AutoTare_Reference"]
}
```
        ----------------------------------------------------------------------------------------------------

## ✅ Rules You MUST Follow

### ✅ Rule 1 – Exactly one primary per PLC address
* **✔ Correct:** `"S1_Main_Loadcell" → isPrimary: true`
* **❌ Wrong (will fail startup):** Two signals with same address both marked `isPrimary`


### ✅ Rule 2 – Aliases must NOT define addresses
* **✔ Correct:**
```json
    { 
      "signalId": "Compression_Force", 
      "isPrimary": false 
    }
```
* **❌ Wrong:** Alias with address defined

* 
### ✅ Rule 3 – A primary signalId must never appear as an alias
* **❌ Wrong:** `"aliases": ["S1_Main_Loadcell"]`
    * *This causes ambiguity and is blocked at startup.*

    * 
### ✅ Rule 4 – No alias chains
* **❌ Wrong:** `A → alias B` and `B → alias C`
* **✔ Correct:** `A → aliases ["B", "C"]`
    * *All aliases must point **directly** to the primary signal.*


### ✅ Rule 5 – Groups are NOT data types
Groups like `Analog_Inputs` or `Digital_Inputs` are **semantic categories**, not PLC memory areas.

**Example:**
* `Analog_Inputs` ≠ `InputRegister`
* `Digital_Inputs` ≠ `DiscreteInput`

        ----------------------------------------------------------------------------------------------------
# 🧭 Signal Configuration Decision Guide

### _(Alias vs Group vs Role)_
- Use this table whenever you are unsure how to model a signal in `signals.json`.

## 🔑 Quick Rule of Thumb
- **Name change → Alias**  
- **Page/context change → Group**  
- **Meaning/behavior change → Role**

## 📊 Decision Table

| Situation                                                   | Use               | Why                                            | Example                                  |
|-------------------------------------------------------------|-------------------|------------------------------------------------|------------------------------------------|
| Same PLC address, same meaning, different name              | **Alias**         | Just a different label for the same value      | `S1_Main_Loadcell` → `Compression_Force` |
| Same PLC address, same meaning, different page              | **Group**         | Same value shown in multiple UI pages          | `MainPage`, `SettingsPage`               |
| Same PLC address, same meaning, different name **and** page | **Alias + Group** | Name is different, context is different        | Alias inherits groups from primary       |
| Same PLC address, different semantic meaning                | **Role**          | Value is used differently (display vs control) | `AutoMode_Force`                         |
| Same PLC address, different semantic meaning **and** page   | **Role + Group**  | Contextual meaning tied to page                | `AutoMode` role                          |
| Same PLC address, different polling behavior                | ❌ **Not allowed**| Polling is tied to the primary signal only     | —                                       |
| Same PLC address, multiple primaries                        | ❌ **Not allowed**| Causes duplicated polling                      | Blocked at startup                      |
| Alias with its own groups                                   | ❌ **Not allowed**| Aliases inherit groups                         | Use Role instead                        |
| Role defining PLC address                                   | ❌ **Not allowed**| Roles are logical only                         | Primary owns address                    |

## 🧠 What Each Concept Represents

| Concept            | Represents        | Owns PLC Address | Polled   | Can Have Groups      |
|--------------------|-------------------|------------------|----------|----------------------|
| **Primary Signal** | Physical value    | ✅ Yes           | ✅ Yes  | ✅ Yes               |
| **Alias**          | Alternate name    | ❌ No            | ❌ No   | ❌ No (inherits)     |
| **Group**          | UI / context      | —                | —        | Attached to Primary  |
| **Role**           | Alternate meaning | ❌ No            | ❌ No   | ✅ Yes               |


        ----------------------------------------------------------------------------------------------------
## ❌ Common Mistakes (Avoid These)

### ❌ Using groups instead of aliases
* `"groups": ["S1MainLoadcell"]`
* **➡ Wrong** — this is a name, not a context.

### ❌ Using aliases instead of groups
* `"aliases": ["MainPage"]`
* **➡ Wrong** — pages are not names.

### ❌ Defining multiple PRIMARY signals for the same PLC address
* Two signals with the same PLC address both marked as `isPrimary`.
* **➡ Wrong** — this causes duplicated polling and is blocked at startup.

✔ **Correct approach:**  
Use **one primary signal** and define additional names as **aliases**,  
and use **groups** to control page placement.


### ❌ Adding comments like _comment
* JSON does not support comments reliably.
* **✔ Use:** `"description": "Explanation here"`

        ----------------------------------------------------------------------------------------------------
### 🧩 Minimal Valid Signal Examples
-This section shows the smallest valid configurations for common scenarios.
-Use these as templates when adding or modifying signals.

### ✅ 1. Minimal Primary Signal (Most Common)
When to use:
-You have a real PLC address and just want to read the value.
```json
{
  "signalId": "S1_Main_Loadcell",
  "dataType": "Double",
  "updateClass": "Critical",
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0",
    "length": 2
  }
}
```
✔ isPrimary is optional
✔ Presence of address makes it primary
✔ Will be polled and cached

### ✅ 2. Primary Signal With Groups (UI Pages / Categories)
When to use:
-Same value appears on multiple pages or categories.
```json
{
  "signalId": "Door_Interlock",
  "dataType": "Bool",
  "updateClass": "Fast",
  "address": {
    "kind": "Numeric",
    "area": "DiscreteInput",
    "value": "2"
  },
  "groups": ["Digital_Inputs", "Safety", "MainPage"]
}
```
✔ Groups do not affect polling
✔ Used by UI / SignalR

### ✅ 3. Primary Signal With Aliases (Different Names, Same Meaning)
When to use:
-Same physical value, same meaning, different naming contexts.
```json
{
  "signalId": "S1_Main_Loadcell",
  "dataType": "Double",
  "updateClass": "Critical",
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0",
    "length": 2
  },
  "aliases": [
    "Compression_Force",
    "AutoTare_Reference"
  ],
  "groups": ["Analog_Inputs", "MainPage"]
}
```
✔ Aliases do NOT define addresses
✔ Aliases inherit groups from primary
✔ Aliases are NOT polled

### ✅ 4. Alias-Only Entry (Explicit Alias Declaration)
When to use:
-You want to explicitly declare an alias without redefining the primary.
```json
{
  "signalId": "Compression_Force",
  "isPrimary": false
}
```
✔ No address
✔ Resolves to its primary
✔ Optional — aliases can also be declared inline in primary

### ✅ 5. Primary Signal With Roles (Different Meaning, Same Value)
When to use:
-Same PLC value is used with different semantic meaning.
```json
{
  "signalId": "S1_Main_Loadcell",
  "dataType": "Double",
  "updateClass": "Critical",
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0",
    "length": 2
  },
  "groups": ["MainPage"],
  "roles": {
    "AutoMode_Force": {
      "description": "Force reference used during automatic cycle",
      "groups": ["AutoMode"]
    },
    "Diagnostics_Force": {
      "description": "Force shown on diagnostics screen",
      "groups": ["Diagnostics"]
    }
  }
}
```
✔ Roles do NOT define addresses
✔ Roles do NOT affect polling
✔ Roles may have their own groups
✔ Roles share the same cached value

### ❌ 6. Invalid Examples (Will Fail Startup)
❌ 6A. Alias defining an address
```json
{
  "signalId": "Compression_Force",
  "address": {
    "kind": "Numeric",
    "area": "InputRegister",
    "value": "0"
  }
}
```
🚫 Aliases must NOT define addresses.

❌ 6B. Two primaries for the same address
```json
{
  "signalId": "Loadcell_A",
  "address": { "kind": "Numeric", "area": "InputRegister", "value": "0" }
},
{
  "signalId": "Loadcell_B",
  "address": { "kind": "Numeric", "area": "InputRegister", "value": "0" }
}
```
🚫 Causes duplicated polling → blocked at startup.

❌ 6C. Role defining an address
```json
"roles": {
  "AutoMode_Force": {
    "address": {
      "kind": "Numeric",
      "area": "InputRegister",
      "value": "0"
    }
  }
}
```
🚫 Roles are logical only — primary owns the address.