/*global bleTime*/

(function () {
    "use strict";
  
    $(document).ready(function () {
        
        //var today = new Date(),
        //    yesterday;
        //// round down to beginning of day
        //today = new Date(today.getFullYear(), today.getMonth(), today.getDate());
        //yesterday = bleTime.addDays(today, -1);
        //bleData.setDates(yesterday,today);
        //bleData.setDateChooser('Change');
        
        //$(".detectChange").change(function () {
        //    $("#saveChanges").prop("disabled", false);
        //});
        bleData.showData();
    });
})();



var weatherApp = (function () {
    "use strict";
    var weatherApp = {},
    ismobile,
    platform,
    interval = 60;  // seconds

    //function updateTime() {
    //    var d = new Date();
    //    if ((d.getSeconds()) < interval) {
    //        var timetext = d.toDateString() + ' ' + bleTime.timeString(d);
    //        $("#realtime").html('BLE Log <span style="color:black; font-size:small">' + timetext + ' ' + platform + '</span>');
    //    }
    //    if (ismobile) {
    //        // every few seconds, update connected status of devices
    //        tagConnect.updateConnections(interval);
    //    }
    //}

    return {
        init: function () {
            ismobile = false;
           // updateTime();
            interval = 5;
            window.setInterval(function () {
           //     updateTime();
            }, interval * 1000);
            $.ajaxSetup({ cache: false });
        },
        

    };

}());


