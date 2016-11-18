var map;
var markers;
var popup;
var epsg4326 = new OpenLayers.Projection("EPSG:4326");

function createMap(divName, centre, zoom) { 
    OpenLayers.Util.onImageLoadError = function() {
        this.src = "cycle_map_more_soon2.png";
    } 
    map = new OpenLayers.Map(divName, { 
        maxExtent: new OpenLayers.Bounds(-20037508,-20037508,20037508,20037508),
        numZoomLevels: 19,
        maxResolution: 156543,
        units: 'm',
        projection: "EPSG:900913",
        controls: [
            //new OpenLayers.Control.LayerSwitcher({roundedCornerColor: "#575757"}),
            new OpenLayers.Control.Permalink('permalink'),
            new OpenLayers.Control.Permalink('editlink', 'http://www.openstreetmap.org/edit.html'),
            new OpenLayers.Control.Attribution(),
            new OpenLayers.Control.PanZoomBar(),
            new OpenLayers.Control.Navigation() 
        ],
        displayProjection: new OpenLayers.Projection("EPSG:4326")
    });

    var cycleattrib = '<b>OpenCycleMap.org - the <a href="http://www.openstreetmap.org">OpenStreetMap</a> Cycle Map</b><br />'
                    //+ '<a href="/docs/">Key and More Info</a>'
                    //+ ' | <a href="/donate/">Donate</a>'
                    //+ ' | <a href="http://shop.opencyclemap.org">Shop</a>'
                    //+ ' | <a href="/gps/">GPS</a><br /><br />'
                    //+ '<a href="http://www.thunderforest.com">Developer Information</a>'
    ;


    var cycle = new OpenLayers.Layer.OSM("OpenCycleMap",
        [
            "https://a.tile.thunderforest.com/cycle/${z}/${x}/${y}.png",
            "https://b.tile.thunderforest.com/cycle/${z}/${x}/${y}.png",
            "https://c.tile.thunderforest.com/cycle/${z}/${x}/${y}.png"
        ],
        {
            displayOutsideMaxExtent: true,
            attribution: cycleattrib,
            transitionEffect: 'resize'
        });
    map.addLayer(cycle);

    if (!map.getCenter())
        map.setCenter(centre, zoom);
    map.events.register("moveend", map, updateLocation);
    map.events.register("changelayer", map, updateLocation);
    return map;
}

function mercatorToLonLat(merc) {
    var lon = (merc.lon / 20037508.34) * 180;
    var lat = (merc.lat / 20037508.34) * 180;
    lat = 180 / Math.PI * (2 * Math.atan(Math.exp(lat * Math.PI / 180)) - Math.PI / 2);
    return new OpenLayers.LonLat(lon, lat);
}

function lonLatToMercator(ll) {
    var lon = ll.lon * 20037508.34 / 180;
    var lat = Math.log(Math.tan((90 + ll.lat) * Math.PI / 360)) / (Math.PI / 180);
    lat = lat * 20037508.34 / 180;
    return new OpenLayers.LonLat(lon, lat);
}

function scaleToZoom(scale) {
    return Math.log(360.0 / (scale * 512.0)) / Math.log(2.0);
}

function updateLocation() {
    var lonlat = map.getCenter().clone().transform(map.getProjectionObject(), epsg4326);
    var zoom = map.getZoom();
    var layers = getMapLayers();
    var extents = map.getExtent().clone().transform(map.getProjectionObject(), epsg4326);
    var expiry = new Date();
    expiry.setYear(expiry.getFullYear() + 10);
    document.cookie = "_osm_location=" + lonlat.lon + "|" + lonlat.lat + "|" + zoom + "|" + layers + "; expires=" + expiry.toGMTString();
}

function getMapLayers() {
    var layerConfig = "";
    for (var layers = map.getLayersBy("isBaseLayer", true), i = 0; i < layers.length; i++) {
        layerConfig += layers[i] == map.baseLayer ? "B" : "0";
    }
    for (var layers = map.getLayersBy("isBaseLayer", false), i = 0; i < layers.length; i++) {
        layerConfig += layers[i].getVisibility() ? "T" : "F";
    }
    return layerConfig;
}

function setMapExtent(extent) {
    map.zoomToExtent(extent.clone().transform(epsg4326, map.getProjectionObject()));
}
