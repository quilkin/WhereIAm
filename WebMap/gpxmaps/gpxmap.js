/* When the user clicks on the button, 
toggle between hiding and showing the dropdown content */
function myFunction() {
    document.getElementById("myDropdown").classList.toggle("show");
}

// Close the dropdown if the user clicks outside of it
window.onclick = function (event) {
    if (!event.target.matches('.dropbtn')) {

        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}

var map;

function display_gpx(elt) {
    if (!elt) return;

    var url = elt.getAttribute('data-gpx-source');
    var mapid = "demo-map";
    var elevid = "demo-elev";
    var demo = document.getElementById('demo');


    if (!url || !mapid) return;

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

document.getElementById('stithians').onclick = function () {
    display_gpx(document.getElementById('stithians'));
};
document.getElementById('bmmr40').onclick = function () {
    display_gpx(document.getElementById('bmmr40'));
};
document.getElementById('bmmr55').onclick = function () {
    display_gpx(document.getElementById('bmmr55'));
};
document.getElementById('LandsEnd').onclick = function () {
    display_gpx(document.getElementById('LandsEnd'));
};



