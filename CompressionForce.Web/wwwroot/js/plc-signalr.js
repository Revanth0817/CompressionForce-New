const connection = new signalR.HubConnectionBuilder()
    .withUrl("/plcHub")
    .withAutomaticReconnect()
    .build();

connection.on("plcSignalUpdated", (key, value) => {
    switch (key) {
        // Motor related
        case "MOTOR_STATUS":
            updateMotorInput("motorStatus", value);
            break;
        case "MOTOR_TRIP":
            updateMotorInput("motorTrip", value);
            break;
        case "REVOLUTIONS":
            updateInput("revolutions", value);
            break;

        // Loadcells
        case "LC_S1_MAIN":
            updateInput("S1Main", value);
            break;
        case "LC_S2_MAIN":
            updateInput("S2Main", value);
            break;
        case "LC_S1_PRE":
            updateInput("S1Pre", value);
            break;
        case "LC_S2_PRE":
            updateInput("S2Pre", value);
            break;
        case "LC_S1_EJECT":
            updateInput("S1Eject", value);
            break;
        case "LC_S2_EJECT":
            updateInput("S2Eject", value);
            break;

        // Buttons (true = ON, false = OFF)
        case "DOOR_INTERLOCK":
            setButtonStateById("doorInterlockBtn", value);
            break;
        case "TURRET_START":
            setButtonStateById("turretStartBtn", value);
            break;
    }
});

function updateInput(id, value) {
    const el = document.getElementById(id);
    if (!el) return;

    if (!isNaN(value)) {
        el.value = Number(value).toFixed(2) ;
    } else {
        el.value = value;
    }
}

function updateMotorInput(id, value) {
    const el = document.getElementById(id);
    if (!el) return;

    el.value = value ? "On" : "Off";

    // Toggle colors
    if (value) {
        el.classList.add("bg-success", "text-white");
        el.classList.remove("bg-danger");
    } else {
        el.classList.add("bg-danger", "text-white");
        el.classList.remove("bg-success");
    }
}

function setButtonStateById(id, value) {
    const btn = document.getElementById(id);
    if (!btn) return;

    const isOn = (value === true || value === "1" || value === 1);

    btn.disabled = !isOn;

    // Visual feedback: override base background class
    if (isOn) {
        btn.classList.add("btn-success");
        btn.classList.remove("btn-secondary");
    } else {
        btn.classList.add("btn-secondary");
        btn.classList.remove("btn-success");
    }
}

// Turret Start click handler
document.addEventListener("DOMContentLoaded", () => {
    const turretBtn = document.getElementById("turretStartBtn");
    if (turretBtn) {
        turretBtn.addEventListener("click", () => {
            fetch("/autotare/offset", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ value: "Ok" })
            })
                .then(resp => {
                    if (!resp.ok) throw new Error("Request failed");
                    return resp.text();
                })
                .then(data => console.log("Server response:", data))
                .catch(err => console.error("Error sending offset:", err));
        });
    }
});

connection.start()
    .then(() => console.log("AutoTare SignalR connected"))
    .catch(err => console.error(err));
