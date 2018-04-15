///* When the user clicks on the button, 

var map;

function display_gpx(url) {
    if (!url) return;

    //var url = elt.getAttribute('data-gpx-source');
    var mapid = "demo-map";
    var elevid = "demo-elev";
    var demo = document.getElementById('demo');


    if (!url || !mapid) return;
    _t('h3').textContent = "please wait...";

    function _t(t) { return demo.getElementsByTagName(t)[0]; }
    function _c(c) { return demo.getElementsByClassName(c)[0]; }

    if (map != undefined) { map.remove(); }
    map = L.map(mapid);
    L.tileLayer('http://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: 'Map data &copy; <a href="http://www.osm.org">OpenStreetMap</a>'
    }).addTo(map);

  //  var control = L.control.layers(null, null).addTo(map);

    new L.GPX(url, {
        async: true,
        marker_options: {
            startIconUrl: 'http://github.com/mpetazzoni/leaflet-gpx/raw/master/pin-icon-start.png',
            endIconUrl: 'http://github.com/mpetazzoni/leaflet-gpx/raw/master/pin-icon-end.png',
            shadowUrl: 'http://github.com/mpetazzoni/leaflet-gpx/raw/master/pin-shadow.png',
        },
    }).on('loaded', function (e) {
        var gpx = e.target;
        var elev_data = gpx.get_elevation_data();
        map.fitBounds(gpx.getBounds());
   //     control.addOverlay(gpx, gpx.get_name());

        /*
         * Note: the code below relies on the fact that the demo GPX file is
         * an actual GPS track with timing and heartrate information.
         */
        _t('h3').textContent = gpx.get_name();
        //_c('start').textContent = gpx.get_start_time().toDateString() + ', '
        //  + gpx.get_start_time().toLocaleTimeString();
        _c('distance').textContent = (gpx.get_distance()/1000).toFixed(1);
        //_c('duration').textContent = gpx.get_duration_string(gpx.get_moving_time());
        //_c('pace').textContent = gpx.get_duration_string(gpx.get_moving_pace_imp(), true);
        //_c('avghr').textContent = gpx.get_average_hr();
        _c('elevation-gain').textContent =gpx.get_elevation_gain().toFixed(0);
        _c('elevation-loss').textContent = gpx.get_elevation_loss().toFixed(0);
        _c('elevation-net').textContent = (gpx.get_elevation_gain() - gpx.get_elevation_loss()).toFixed(0);

        //drawProfile1("demo-elev", elev_data);

        // convert array to json for profile graph
        var i, n = elev_data.length;
        var json_elev = new Array();
        for (i = 0; i < n; i++) {
            json_elev.push(new height(elev_data[i][0].toFixed(1), elev_data[i][1].toFixed(0)));

        }
        drawProfile(elevid, json_elev);

    }).addTo(map);


}
function height(dist_t, height_t) {
    this.Distance = dist_t;
    this.Height = height_t;
}


var routeTable, r, route, row,  destination, distance, type, url;

var routes = [
    ['Stithians Lake', 'Intermediate', '50km', "https://quilkin.co.uk/gpxmaps/Stithians_Int.gpx"],
    ['BMMR short ', 'Leisure', '40km', "https://quilkin.co.uk/gpxmaps/bmmr40.gpx"],
    ['BMMR medium', 'Intermediate', '55km'," https://quilkin.co.uk/gpxmaps/bmmr55.gpx"],
    ["Nearly Land's End", 'Long', '130km', "https://quilkin.co.uk/gpxmaps/NearlyLandsEnd.gpx"],
    
]
document.getElementById("newRoute").style.display = "none";
routeTable = document.getElementById('routes');
createRouteTable();

function createRouteTable() {

    routeTable.innerHTML = "";
    for (r = 0; r < routes.length; r++) {
        route = routes[r];
        row = routeTable.insertRow(r);
        destination = row.insertCell(0);
        type = row.insertCell(1);
        distance = row.insertCell(2);
        destination.innerHTML = route[0];
        type.innerHTML = route[1];
        distance.innerHTML = route[2];
        (function (url) {
            row.addEventListener("click", function () { display_gpx(url); });
        })(route[3]);

    }
    row = routeTable.insertRow(r);
    destination = row.insertCell(0);
    destination.colSpan = 3;
    destination.innerHTML = "Add new route";
    row.addEventListener("click", function () {
       // location.href = "addRoute.html";
        document.getElementById("newRoute").style.display = "block";

    });

}
var newName, newGPX, newDist;
function validateForm() {
    newName = document.getElementById('name').value;
    if (newName == "") {
        alert("Name must be filled out");
        return false;
    }
    newDist = document.getElementById('dist').value;
    if (newDist <= 0) {
        alert("Distance must be greater than zero");
        return false;
    }
    newGPX = document.getElementById("gpx").value;
    if (newGPX == "") {
        alert("GPX data must be provided");
        return false;
    }
    document.getElementById("newRoute").style.display = "none";
    routes.push([newName, ' ', newDist+'km', newGPX]);
    createRouteTable();
    return true;
}




