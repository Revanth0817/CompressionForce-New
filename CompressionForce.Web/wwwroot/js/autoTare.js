const connection = new signalR.HubConnectionBuilder()
    .withUrl("/plcHub")
    .build();

connection.on("plcUpdate", (key, value) => {
    console.log(key, value);
    // update DOM / React state / etc
});

connection.start();
