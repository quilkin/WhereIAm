// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkID=397704
// To debug code on page load in Ripple or on Android devices/emulators: launch your app, set breakpoints, 
// and then run "window.location.reload()" in the JavaScript Console.
(function () {
    "use strict";

    document.addEventListener( 'deviceready', onDeviceReady.bind( this ), false );

    function onDeviceReady() {
        // Handle the Cordova pause and resume events
        //document.addEventListener( 'pause', onPause.bind( this ), false );
        //document.addEventListener( 'resume', onResume.bind( this ), false );
        
        //// TODO: Cordova has been loaded. Perform any initialization that requires Cordova here.
        //configureBackgroundGeoLocation();
        //myMap.watchPosition();

    };

    $(document).ready(function () {

        // just testing server connections

        //var loc = { latitude: 47.640068, longitude: -122.129858 };
        //MapData.json('SaveLocation', "POST", loc, function () { }, true, null);
        //MapData.json('GetLocations', "POST", null, function (locs) {
        //    var locations = [];
        //    $.each(locs, function (index, loc) {
        //        if (loc.latitude > 0)
        //            locations += loc;
        //    })
        //}, true, null);

        //myMap.create();
        //setTimeout(function () {
        //    window.location.reload(1);
        //}, 300000);
    });

    function onPause() {
        ///**
        //* Cordova foreground geolocation watch has no stop/start detection or scaled distance-filtering to conserve HTTP requests based upon speed.  
        //* You can't leave Cordova's GeoLocation running in background or it'll kill your battery.  This is the purpose of BackgroundGeoLocation:  to intelligently 
        //* determine start/stop of device.
        //*/
        //    console.log('- onPause');
        //    myMap.stopPositionWatch();
    };

    function onResume() {
        ///**
        //* Once in foreground, re-engage foreground geolocation watch with standard Cordova GeoLocation api
        //*/
        //    console.log('- onResume');
        //    myMap.watchPosition();
    };


} )();