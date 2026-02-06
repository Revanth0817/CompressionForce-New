# 📘 PLC Configuration – README

## Purpose of this document
This document explains **how to correctly configure PLC using JSON files** in this project.
It is written for:
- New team members
- Commissioning engineers
- Future maintainers

⚠️ **Incorrect configuration can cause duplicated polling, wrong values, or unsafe behavior.** Please read this fully before editing any JSON file.

        ----------------------------------------------------------------------------------------------------

## 📁 Configuration Files Overview
All PLC-related configuration lives under:
`CompressionForce.Web/Configurations`

### Files and responsibilities
| File | Purpose |
| --- | --- |
| `signals.json` | Defines PLC signals, addresses, aliases, and UI grouping |
| `polling.json` | Defines polling frequencies |
| `plc-connection.json` | Defines protocol and connection details |

        ----------------------------------------------------------------------------------------------------

## 🔄 How Polling Works (So You Don’t Break It)
* Polling is based on **primary signals only**.
* Polling frequency comes from `updateClass`.
* Groups and aliases do NOT affect polling.
* Changing groups will NEVER change PLC traffic.
* *This is intentional.*

## 🧪 What Happens If You Make a Mistake?
The system performs **startup validation**. If configuration is invalid:
* Application **will not start**.
* A clear error message will be shown.
* No unsafe or partial behavior occurs.
* *This is by design.*

## 🧭 Remember This
PLC Address    → Primary Signal
Primary Signal → Aliases (names)
Primary Signal → Groups (context)

* **Address** = hardware truth
* **Alias** = vocabulary
* **Group** = usage / UI

## ✅ When in Doubt
If you are unsure:
1.  Do **NOT** duplicate a signal.
2.  Prefer adding a **group**.
3.  Prefer adding an **alias**.
4.  Ask before marking `isPrimary: true`.
5.  If the same value is used with a different meaning, ask whether a **role** is needed instead of an alias.

## 📌 Final Notes
This configuration system is designed to:
* Scale across protocols (Modbus, OPC UA, ADS).
* Support multiple access strategies.
* Avoid redundant PLC reads.
* Be safe during commissioning.

Following these rules ensures:
* Deterministic behavior.
* Clean UI integration.
* Eficient and Safe
* Future extensibility without refactor.


