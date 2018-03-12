/// <reference path="~\openlayers\OpenLayers.js" />




var MapData = (function ($) {
    "use strict";

    var MapData = {};
    

    function urlBase() {
     return "https://localhost/WebMap/WebMap.svc/";
      //  return "https://www.quilkin.co.uk/WebMap.svc/";
     

    }
    function webRequestFailed(xhr) {
            alert('Web Error: ' + xhr.status + ' Status Text: ' + xhr.statusText + ' ' + xhr.responseText);
        }

        //alert("Web Error: " + error + " " + status);


    MapData.json = function (url, type, data, successfunc) {
        var thisurl = urlBase() + url;
        if (data === null) {
            $.ajax({
                type: type,
                url: thisurl,
                contentType: 'application/x-www-form-urlencoded',
                success: successfunc,
                error: webRequestFailed
            });
        }
        else {
            var dataJson = JSON.stringify(data);

            $.ajax({
                type: type,
                data: dataJson,
                url: thisurl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successfunc,
                error: webRequestFailed
            });
        }
    };

    MapData.jsonMapzen = function (data, successfunc) {
        var dataJson = JSON.stringify(data);
        var url = 'https://valhalla.mapzen.com/route?json=' +  dataJson + '&api_key=valhalla-3F5smze' ;
        
        //var dataJson = "{\"locations\":[{\"lat\":42.358528,\"lon\":-83.271400},{\"lat\":42.996613,\"lon\":-78.749855}]}"
            $.ajax({
                url: url,
                type: "GET",
                contentType: 'application/x-www-form-urlencoded',
                dataType: "json",
                success: successfunc,
                error: webRequestFailed
            });

    };


    return MapData;


}(jQuery));

var myMap = (function ($) {
    "use strict";


    aggressiveEnabled: false;
    var userID = 0;
    // distance to check if on track or not (approx 20 m)
    var near = 0.0002;
    // distance to check if too far from track (appox 500m)
    var far = 0.05;
    // message when off track
    var offTrack1 = "Attention! You are "
    var offTrack2 = " metres off course. Correct course is to the "

    var myMap = {},
        map = null,
        location,
        path,
        aggressiveEnabled,
        locations = [],
        iconCentre1,
        messageBox,
        routeLine = null,
        routes = [],
        markers=[],
        legs = [],
        wayPoints = [],
        routePoints = [],
        followedPoints = [],
        nearestPoint = null,
        lastNearestPoint = null,
        onTrack = false,
        lastLine1, lastLine2, lastDem,
        bikeType,
        useRoads = 2,
        useHills = 2,
        nearest,nextNearest,
        dialog,dialogContents;


    var redIcon = new L.Icon({
        iconUrl: 'scripts/images/marker-icon-red.png',
        shadowUrl: 'scripts/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    var greenIcon = new L.Icon({
        iconUrl: 'scripts/images/marker-icon-green.png',
        shadowUrl: 'scripts/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    var orangeIcon = new L.Icon({
        iconUrl: 'scripts/images/marker-icon-orange.png',
        shadowUrl: 'scripts/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    var yellowIcon = new L.Icon({
        iconUrl: 'scripts/images/marker-icon-yellow.png',
        shadowUrl: 'scripts/images/marker-shadow.png',
        iconSize: [25, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41]
    });

    var user = window.localStorage.username;
    var pw = window.localStorage.password;
    if (user != undefined && user.length > 0) {
        $("#username").val(user);
    }
    if (pw != undefined && pw.length > 0) {
        $("#password").val(pw) ;
    }
    // Create additional Control placeholders
    function addControlPlaceholders(map) {
        var corners = map._controlCorners,
            l = 'leaflet-',
            container = map._controlContainer;

        function createCorner(vSide, hSide) {
            var className = l + vSide + ' ' + l + hSide;

            corners[vSide + hSide] = L.DomUtil.create('div', className, container);
        }

        createCorner('verticalcenter', 'left');
        createCorner('verticalcenter', 'right');
        createCorner('center', 'left');
        createCorner('center', 'right');

    }
    $("#newpw").click(function() {
        //alert("Please ensure your phone has web access, then login with password 'NewPassword'");
        alert("Please ensure your phone has web access");
        $("#password").val('NewPassword');
        $("#login").click();
    })

    $("#login").click(function() {
        var u, p, creds, form = $("#login");
        //disable the button so we can't resubmit while we wait
        $("#login").attr("disabled", "disabled");
        u = $("#username").val();
        p = $("#password").val();
        //userRole = 0;


        if (u !== '' && p !== '') {
            creds = { name: u, pw: p, email: "", code: 0 };
            MapData.json('Login', "POST", creds, function (res) {
                if (res.pw == "NewPassword") {
                    alert("See your phone for more instructions, which should arrive within 10 minutes");

                }
                else if (res.pw == "" && res.name=="") {
                    alert("Invalid username");
                }
                else if (res.pw == "") {
                        alert("Invalid password");
                    }

                else {
                    window.localStorage.username = u;
                    window.localStorage.password = p;
                    userID = res.id;

                    createMap(userID,true);
                    // refresh map every 5 minutes
                    setInterval(function () {
                        updateMap(userID,false);
                    }, 300000);
 
                }
                $("#login").removeAttr("disabled");
            });

        } else {
            alert("You must enter a username and password");
            $("#login").removeAttr("disabled");
        }

        //userID =1;

        //createMap(userID,true);
        //// refresh map every 5 minutes
        //setTimeout(function () {
        //    updateMap(userID,false);
        //}, 300000);

        return false;
    })

    function updateMap(userid, firstUpdate) {
        // get the list of points to map
        var tempID = userID;
        if (!firstUpdate) {
            tempID = -tempID;
        }
        MapData.json('GetLocations', "POST", tempID, function (locs) {
        //MapData.json('GetLocationsGet', "GET", null, function (locs) {

            // first point will be the latest one recorded, use this to centre the map
            location = locs[0];
            map.setView([location.latitude, location.longitude],14);

            var index, count = locs.length;
            var now = new Date();
            var reggie = /(\d{2})\/(\d{2})\/(\d{4}) (\d{2}):(\d{2}):(\d{2})/;
            var dateArray, dateObj;
            for (index = count - 1; index >= 0; index--) {

                var loc = locs[index];
                if (loc.latitude != 0) {
                    var dt = now;
                    // convert SQL date string to EU format
                    dateArray = reggie.exec(loc.recorded_at);
                    dt = new Date(
                    (+dateArray[3]),
                    (+dateArray[2]) - 1, // Careful, month starts at 0!
                    (+dateArray[1]),
                    (+dateArray[4]),
                    (+dateArray[5]),
                    (+dateArray[6])
                );
                    var timediff = now.valueOf() - dt.valueOf();
                    var colour = 'blue';
                    if (index === 0) {
                        colour = 'purple';
                        if (timediff < 60 * 60000) {
                            // newer than one hour
                            colour = 'red';
                        }
                    }
                    else {
                        if (timediff > 24 * 60 * 60000)
                            colour = 'gray';
                    }

                    var circle = L.circle([loc.latitude, loc.longitude], (index === 0) ? 60 : 15, {
                        color: colour,
                        fillColor: colour,
                        fillOpacity: 0.5
                    }).addTo(map);
                    circle.bindPopup(loc.recorded_at);
                }
            }



        }, true, null);
    }

    function createMap(userid) {

        map = L.map('map', { messagebox: true });
        L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 18

        }).addTo(map);
        updateMap(userID, 300);
        //AddControls();
    }
      

  

    function AddControls() {

        addControlPlaceholders(map);

        L.control.mousePosition().addTo(map);

        // a cross-hair for choosing points
        iconCentre1 = L.control({ position: 'centerleft' });
        iconCentre1.onAdd = function (map) {
            this._div = L.DomUtil.create('div', 'myControl');
            var img_log = "<div><img src=\"images/crosshair.png\"></img></div>";
            this._div.innerHTML = img_log;
            return this._div;

        }
        iconCentre1.addTo(map);

        dialogContents = [
         "<p>Quilkin cycle router: Options</p>",
         "<button class='btn btn-primary' onclick='myMap.changeBike()'>Bike Type: Hybrid</button><br/><br/>",
          "<button class='btn btn-primary' onclick='myMap.changeHills()'>Use of hills (0-9): 2</button><br/><br/>",
          "<button class='btn btn-primary' onclick='myMap.changeMainRoads()'>Use of main roads (0-9): 2</button><br/><br/>",
        ].join('');

        dialog = L.control.dialog()
                  .setContent(dialogContents)
                  .addTo(map);

        //L.easyButton('<span class="bigfont">&rarr;</span>', createRoute).addTo(map);
        L.easyButton('<span class="bigfont">&check;</span>', addPoint).addTo(map);
        L.easyButton('<span class="bigfont">&cross;</span>', deletePoint).addTo(map);
        L.easyButton('<span class="bigfont">&odot;</span>', openDialog).addTo(map);
        bikeType = "Hybrid";
        map.messagebox.options.timeout = 5000;
        map.messagebox.setPosition('bottomleft');
        map.messagebox.show('');


    }

    function addPoint() {

        var centre = map.getCenter();
        wayPoints.push(L.latLng(centre.lat, centre.lng));
        //if (route == undefined) {
        //if (routes.length == 0)
        var marker = L.marker([centre.lat, centre.lng]).addTo(map);
       // responsiveVoice.speak("Added a point");
        if (wayPoints.length === 1) {
            marker = L.marker([centre.lat, centre.lng], { icon: greenIcon }).addTo(map);
            markers.push(marker);
            // this is first (starting) point. Need more points!
            return;
        }
        
        markers.push(marker);
        createRoute();
    }
    function deletePoint()
    {
        if (wayPoints.length < 2) {
            alert("No waypoints to delete!")
            return;
        }
        var marker = markers.pop();
        map.removeLayer(marker);
        wayPoints.pop();
        createRoute();
    }
    function openDialog() {
        dialog.open();
    }
    myMap.changeBike = function()
    {
        switch (bikeType) {
            case 'Hybrid': bikeType = 'Cross'; dialogContents = dialogContents.replace("Hybrid", "Cross"); break;
            case 'Cross': bikeType = 'Mountain'; dialogContents = dialogContents.replace("Cross", "Mountain"); break;
            case 'Mountain': bikeType = 'Road'; dialogContents = dialogContents.replace("Mountain", "Road"); break;
            case "Road": bikeType = 'Hybrid'; dialogContents = dialogContents.replace("Road", "Hybrid"); break;
        }
        dialog.setContent(dialogContents);
        dialog.update();
        if (wayPoints.length >= 2)
            createRoute();
    }
    myMap.changeMainRoads = function () {
        useRoads = (useRoads + 1) % 10;
        dialogContents = dialogContents.replace(/roads \(0-9\): [0-9]/, "roads (0-9): " + useRoads);
        dialog.setContent(dialogContents);
        dialog.update();
        if (wayPoints.length >= 2)
            createRoute();
    }
    myMap.changeHills = function () {
        useHills = (useHills + 1) % 10;
        dialogContents = dialogContents.replace(/hills \(0-9\): [0-9]/, "hills (0-9): " + useHills);
        dialog.setContent(dialogContents);
        dialog.update();
        if (wayPoints.length >= 2)
            createRoute();
    }
    // Code from Mapzen site
    function polyLineDecode(str, precision) {
        var index = 0,
            lat = 0,
            lng = 0,
            coordinates = [],
            shift = 0,
            result = 0,
            byte = null,
            latitude_change,
            longitude_change,
            factor = Math.pow(10, precision || 6);

        // Coordinates have variable length when encoded, so just keep
        // track of whether we've hit the end of the string. In each
        // loop iteration, a single coordinate is decoded.
        while (index < str.length) {

            // Reset shift, result, and byte
            byte = null;
            shift = 0;
            result = 0;

            do {
                byte = str.charCodeAt(index++) - 63;
                result |= (byte & 0x1f) << shift;
                shift += 5;
            } while (byte >= 0x20);

            latitude_change = ((result & 1) ? ~(result >> 1) : (result >> 1));

            shift = result = 0;

            do {
                byte = str.charCodeAt(index++) - 63;
                result |= (byte & 0x1f) << shift;
                shift += 5;
            } while (byte >= 0x20);

            longitude_change = ((result & 1) ? ~(result >> 1) : (result >> 1));

            lat += latitude_change;
            lng += longitude_change;

            coordinates.push([lat / factor, lng / factor]);
        }

        return coordinates;
    };

    function createRoute()
    {
        if (wayPoints.length < 2)
        {
            alert("No waypoints added!")
            return;
        }
        //if (route != undefined)
        //    map.removeLayer(route);
        while (routes.length > 0)
        {
            var route = routes.pop();
            map.removeLayer(route);
        }
        //var polyline = L.polyline(wayPoints, { color: 'red' }).addTo(map);
        var p, points=[];
        for (p = 0; p < wayPoints.length; p++) {
            points.push({ lat: wayPoints[p].lat, lon: wayPoints[p].lng })
        }
        var data = {
            //locations: [{ lat: wayPoints[0].lat, lon: wayPoints[0].lng }, { lat: wayPoints[1].lat, lon: wayPoints[1].lng }],
            locations: points,
            costing: "bicycle",
            costing_options: {
                bicycle: {
                    bicycle_type: bikeType,
                    use_roads: useRoads / 10,
                    use_hills: useHills / 10
                }
            }
        }
        MapData.jsonMapzen(data,getRoute);
       
    }

    function getRoute(response) {

        
        routePoints = [];
        followedPoints = [];
        nearestPoint = null;
        onTrack = false;

        for (var i = 0; i < response.trip.legs.length; i++) {
            var leg = response.trip.legs[i];
            var legPoints= [];
            var index = 0;
            for (var j = 0; j < leg.maneuvers.length; j++) {
                var maneuver = leg.maneuvers[j];
                var instruction = maneuver.verbal_pre_transition_instruction;
                var shapeIndex = maneuver.begin_shape_index;
                while (index++ < shapeIndex)
                {
                    // no instructions at these points
                    legPoints.push('');
                }
                legPoints.push(instruction);
            }
            // now get coordinates of all points
            var pline = leg.shape;
            var locations = polyLineDecode(pline, 6);

            var colour = 'red';
            if (wayPoints.length > 2) colour = 'blue';
            if (wayPoints.length > 3) colour = 'green';
            var route = new L.Polyline(locations, {
                color: colour,
                opacity: 1,
                weight: 2,
                clickable: false
            }).addTo(map);
            //var id = L.stamp(route);
            routes.push(route);
            for (var loc = 0; loc < locations.length; loc++) {
                // save points passed through, and store any instruction for each point 
                var p1 = locations[loc][0], p2 = locations[loc][1];
                var instr = legPoints[loc];
                routePoints.push([p1, p2, instr]);

            }
        }
        // add dummy final points to help with array indexing later
        var lastPoint = routePoints[routePoints.length - 1];
        routePoints.push(lastPoint);
        routePoints.push(lastPoint);

    }
    function pointToLine(point0,line1,line2) {
        // find min distance from point0 to line defined by points line1 and line2
        // equation from Wikipedia
        var numer,dem;
        var x1 = line1[0], x2 = line2[0], x0 = point0[0];
        var y1 = line1[1], y2 = line2[1], y0 = point0[1];
        
        if (line1===lastLine1 && line2 === lastLine2) {
            // same line as we checked before, can save time by not recalculating sqaure root on demoninator
            dem = lastDem;
        }
        else {
            dem = Math.sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
            lastLine1 = line1;
            lastLine2 = line2;
            lastDem = dem;
        }
        numer =  Math.abs((y2-y1)*x0 - (x2-x1)*y0 + x2*y1 - y2*x1);
        return numer / dem;

    }
    function distanceBetweenCoordinates(point0, point1)
    {
        var x1 = point0[0], x2 = point1[0];
        var y1 = point0[1], y2 = point1[1];
        var R = 6371e3; // metres
        var φ1 = x1 / 57.2958;
        var φ2 = x2 / 57.2958;
        var Δφ = (x2 - x1) / 57.2958;
        var Δλ = (y2 - y1) / 57.2958;

        var a = Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
                Math.cos(φ1) * Math.cos(φ2) *
                Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
        var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

        var d = R * c;
        return d;

    }
    function bearingFromCoordinate(point0, point1) {

        var x1 = point0[0], x2 = point1[0];
        var y1 = point0[1], y2 = point1[1];
        var dLon = (y2 - y1);
        var y = Math.sin(dLon) * Math.cos(x2);
        var x = Math.cos(x1) * Math.sin(x2) - Math.sin(x1)
                * Math.cos(x2) * Math.cos(dLon);
        var brng = Math.atan2(y, x);
        brng = brng * 57.2958;
        brng = (brng + 360) % 360;

        if (brng < 22.5)
            return 'North';
        if (brng < 67.5)
            return 'North East';
        if (brng < 112.5)
            return 'East';
        if (brng < 157.5)
            return 'South East';
        if (brng < 202.5)
            return 'South';
        if (brng < 247.5)
            return 'South West';
        if (brng < 292.5)
            return 'West';
        if (brng < 337.5)
            return 'North West';
        return 'North';
    }

    myMap.planRouteLine = function (lat, lon) {
        //var thisPoint = [lat, lon];
        //var centre = map.getCenter();
        //if (routeLine != null)
        //{
        //    map.removeLayer(routeLine);
        //}
        //routeLine = L.polyline([thisPoint,centre], { color: 'red' }).addTo(map);
    }

    myMap.checkInstructions = function (lat, lon) {
        var thisPoint = [lat, lon];

        followedPoints.push(thisPoint);
        if (nearestPoint === null && onTrack === false) {
            // Nowhere near?. Check point against all points in the route
            for (var loc = 0; loc < routePoints.length; loc++) {
                var point = routePoints[loc];
                // within 20 metres (approx)?
                if (Math.abs(point[0] - lat) < near) {
                    if (Math.abs(point[1] - lon) < near) {
                        nearestPoint = loc;
                        onTrack = true;
                        break;
                    }
                }
            }
        }
        else {
            // we are (or were) on track, see how far we are from the nearest route segment (line)
            lastNearestPoint = nearestPoint;
            nearest = pointToLine(thisPoint, routePoints[nearestPoint], routePoints[nearestPoint + 1]);
            nextNearest = pointToLine(thisPoint, routePoints[nearestPoint + 1], routePoints[nearestPoint + 2]);
            onTrack = (nearest < near);
            if (nextNearest < near) {
                // we have moved on nearer to the next point
                ++nearestPoint;
                onTrack = (nextNearest < near);
            }
            if (!onTrack) {
                // need to start looking from scratch again
                
                nearestPoint = null;
            }
        }
        
        if (onTrack) {
            map.messagebox.show(nearestPoint);
            // find the appropriate instruction to provide
            var instruction = routePoints[nearestPoint][2];
            //if (instruction.length > 2)
            //    responsiveVoice.speak(instruction);
        }
        else if (lastNearestPoint) {
            if (lastNearestPoint >= routePoints.length) {
                lastNearestPoint = routePoints.length - 1;
            }
             // convert offset to multiples of ten metres
            var dist = Math.floor(distanceBetweenCoordinates(thisPoint, routePoints[lastNearestPoint])/10)*10;
            var bearing = bearingFromCoordinate(thisPoint, routePoints[lastNearestPoint]);
            //responsiveVoice.speak(offTrack1 + dist + offTrack2 + bearing);
        }
        return (onTrack);
    }
    return myMap
})(jQuery)
