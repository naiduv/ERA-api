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
        navigator.clipboard.writeText(lat + " " +lng);
    });


    this.position = new Position();
    map.addControl(this.position);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    //get assistants
    loadAssistants()
}

function loadAssistants() {
    markerGroup.remove();
    markerGroup = L.layerGroup().addTo(map);

    var assistants = getAssistants().then(assistants => {
        assistants.forEach(assistant => {
            const loc = assistant.location;
            const reserved = assistant.isReserved;
            L.marker([loc.latitude, loc.longitude], { icon: reserved ? greyIcon : greenIcon, title: loc.latitude + ", " + loc.longitude }).bindTooltip(assistant.id.toString(), {permanent: true, direction:'bottom'}).addTo(markerGroup);
        });
    })


}

const getAssistants = async () => {
    const response = await fetch('http://localhost:8080/Assistant/GetAll');
    const assistantsJson = await response.json();
    return assistantsJson;
}


const getNearest = async (latitude, longitude) => {
    const response = await fetch('http://localhost:8080/Assistant/GetNearestAssistant?Latitude=' + latitude + '&Longitude=' + longitude);
    markerGroup.remove();
    markerGroup = L.layerGroup().addTo(map);
    L.marker([latitude, longitude], { icon: redIcon }).addTo(markerGroup);

    const assistants = await response.json();
    assistants.forEach(assistant => {
        const loc = assistant.location;
        const reserved = assistant.isReserved;
        L.marker([loc.latitude, loc.longitude], { icon: reserved ? greyIcon : greenIcon, title: assistant.id + ": " + loc.latitude + ", " + loc.longitude }).bindTooltip(assistant.id.toString(), { permanent: true, direction: 'bottom' }).addTo(markerGroup);
    });
}

const updateAssistant = async (id, latitude, longitude) => {
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

const reserve = async (id, latitude, longitude) => {
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

    L.marker([latitude, longitude], { icon: redIcon, title: "c"+ id + ": " + latitude + ", " + longitude }).addTo(markerGroup);


    const assistant = await response.json();
    const loc = assistant.location;
    const reserved = assistant.isReserved;
    L.marker([loc.latitude, loc.longitude], { icon: greyIcon, title: "a" + assistant.id + ": " + loc.latitude + ", " + loc.longitude }).bindTooltip(assistant.id.toString(), { permanent: true, direction: 'bottom' }).addTo(markerGroup);

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

    loadAssistants()
}
