const greenIcon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-green.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});

const greyIcon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-grey.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});

const redIcon = new L.Icon({
    iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
    shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/0.7.7/images/marker-shadow.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41]
});

var markerGroup = null;
var map = null;

window.onload = (event) => {
    map = L.map('map').setView([38.226837, -85.731025], 13);
    markerGroup = L.layerGroup().addTo(map);

    let Position = L.Control.extend({
        _container: null,
        options: {
            position: 'bottomleft'
        },

        onAdd: function (map) {
            var latlng = L.DomUtil.create('div', 'mouseposition');
            this._latlng = latlng;
            return latlng;
        },

        updateHTML: function (lat, lng) {
            var latlng = lat + " " + lng;
            //this._latlng.innerHTML = "Latitude: " + lat + "   Longitiude: " + lng;
            this._latlng.innerHTML = "LatLng: " + latlng;
        }
    });

    map.addEventListener('mousemove', (event) => {
        let lat = Math.round(event.latlng.lat * 100000) / 100000;
        let lng = Math.round(event.latlng.lng * 100000) / 100000;
        this.position.updateHTML(lat, lng);
    });

    map.addEventListener('click', (event) => {
        let lat = Math.round(event.latlng.lat * 100000) / 100000;
        let lng = Math.round(event.latlng.lng * 100000) / 100000;
        navigator.clipboard.writeText(lat + "," +lng);
    });


    this.position = new Position();
    map.addControl(this.position);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    //get assistants
    loadAssistants();
    getReservations();
}

function loadAssistants() {
    markerGroup.remove();
    markerGroup = L.layerGroup().addTo(map);

    var assistants = getAssistants().then(assistants => {
        assistants.forEach(assistant => {
            const loc = assistant.location;
            const reserved = assistant.isReserved;
            L.marker([loc.y, loc.x], { icon: reserved ? greyIcon : greenIcon, title: loc.y + ", " + loc.x }).bindTooltip(assistant.id.toString(), {permanent: true, direction:'bottom'}).addTo(markerGroup);
        });
    })


}

const getAssistants = async () => {
    const response = await fetch('http://localhost:8080/Assistant/GetAll');
    const assistantsJson = await response.json();
    return assistantsJson;
}


const getNearest = async (location) => {
    const latitude = parseFloat(location.split(",")[0]);
    const longitude = parseFloat(location.split(",")[1]);

    const response = await fetch('http://localhost:8080/Assistant/GetNearestAssistant?Latitude=' + latitude + '&Longitude=' + longitude);
    markerGroup.remove();
    markerGroup = L.layerGroup().addTo(map);
    L.marker([latitude, longitude], { icon: redIcon }).addTo(markerGroup);

    const assistants = await response.json();
    assistants.forEach(assistant => {
        const loc = assistant.location;
        const reserved = assistant.isReserved;
        L.marker([loc.y, loc.x], { icon: reserved ? greyIcon : greenIcon, title: assistant.id + ": " + loc.y + ", " + loc.x }).bindTooltip(assistant.id.toString(), { permanent: true, direction: 'bottom' }).addTo(markerGroup);
        L.polyline([[loc.y, loc.x], [latitude, longitude]], {color: 'black', weight: 1}).bindTooltip(assistant.distance.toFixed(3) + "mi", { permanent: true, direction: 'right' }).addTo(markerGroup);
    });
}

const updateAssistant = async (id, location) => {
    const [latitude, longitude] = parseLatLong(location);
    const data = {
        "assistantId": id,
        "location": {
            "latitude": latitude,
            "longitude": longitude
        }
    };
    const response = await fetch('http://localhost:8080/Assistant/UpdateAssistant', {
        method: "PUT", body: JSON.stringify(data), headers: {
            "Content-Type": "application/json"
        }
    });

    loadAssistants()
}

const reserve = async (id, location, reservationDisplay) => {    
    if (!reservationDisplay) {
        const customerHasReservationResponse = await fetch('http://localhost:8080/Reservation/GetReservationForCustomer?CustomerId=' + id);
        const customerHasReservation = await customerHasReservationResponse.json();
        if (customerHasReservation) {
            return;
        }
    }

    const latitude = parseFloat(location.split(",")[0]);
    const longitude = parseFloat(location.split(",")[1]);

    const data = {
        "customerId": id,
        "location": {
            "latitude": latitude,
            "longitude": longitude
        }
    };

    const response = await fetch('http://localhost:8080/Reservation/Reserve', {
        method: "POST", body: JSON.stringify(data), headers: {
            "Content-Type": "application/json"
        }
    });

    markerGroup.remove();
    markerGroup = L.layerGroup().addTo(map);

    L.marker([latitude, longitude], { icon: redIcon, title: "c" + id + ": " + latitude + ", " + longitude }).bindTooltip(id.toString(), { permanent: true, direction: 'bottom' }).addTo(markerGroup);


    const assistant = await response.json();
    const loc = assistant.location;
    const reserved = assistant.isReserved;
    L.marker([loc.y, loc.x], { icon: greyIcon, title: "a" + assistant.id + ": " + loc.y + ", " + loc.x }).bindTooltip(assistant.id.toString(), { permanent: true, direction: 'bottom' }).addTo(markerGroup);

    getReservations();
}

const release = async (customerId, assistantId) => {
    const data = {
        "customerId": customerId,
        "assistantId": assistantId
    };

    const response = await fetch('http://localhost:8080/Reservation/Release', {
        method: "PUT", body: JSON.stringify(data), headers: {
            "Content-Type": "application/json"
        }
    });

    loadAssistants();
    getReservations();

}

const parseLatLong = function (locString) {
    if (locString == null || locString.indexOf(",") == -1 || locString.length < 3) {
        console.error("locString is incorrect format. Expected a latitude and longitude seperated by comma.");
        return [0, 0];
    }
    const location = locString.replaceAll(" ", "").split(",")
    const latitude = parseFloat(location[0]);
    const longitude = parseFloat(location[1]);
    return [latitude, longitude];
}

const getReservations = async function() {
    var response = await fetch('http://localhost:8080/Reservation/GetActive');
    var reservations = await response.json();
    var reservationTable = "<table><thead><tr><th>reservation</th><th>customer</th><th>assistant</th><th>distance</th></tr></thead><tbody>";
    reservations.forEach(reservation => {
        reservationTable += "<tr onclick=\"reserve(" + reservation.customerId + ",'" + reservation.customerLocation.y + "," + reservation.customerLocation.x + "', true)\"><td>" + reservation.id + "</td><td>" + reservation.customerId + "</td><td>" + reservation.assistantId + "</td><td>" + reservation.distance.toFixed(3) + "mi" + "</td></tr>";
    });
    reservationTable += "</tbody></table>";
    document.getElementById('reservations').innerHTML = reservationTable;
}