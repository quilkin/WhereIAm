/*global hyper,console*/

var bleTime = (function () {

    "use strict";
    var bleTime = {},
        pad2 = function (num) {
            var s = "00" + num;
            return s.substr(s.length - 2);
        },
        dateString = function (time) {
            // toLocaleTimeString() is no good for different platforms
            return [time.getFullYear(), pad2(time.getMonth() + 1), pad2(time.getDate())].join('-');
        },
        timeString = function (time) {
            // toLocaleTimeString() is no good for different platforms
            return [pad2(time.getHours()), pad2(time.getMinutes())].join(':');
        },
        timeStringSecs = function (time) {
            return [pad2(time.getHours()), pad2(time.getMinutes()), pad2(time.getSeconds())].join(':');
        };

    return {
        dateString: function (time) {
            // toLocaleTimeString() is no good for different platforms
            //return [time.getFullYear(), pad2(time.getMonth() + 1), pad2(time.getDate())].join('-');
            return dateString(time);
        },
        timeString: function (time) {
            // toLocaleTimeString() is no good for different platforms
            //return [pad2(time.getHours()), pad2(time.getMinutes())].join(':');
            return timeString(time);
        },
        dateTimeString: function (time) {
            return dateString(time) + " " + timeString(time);
        },
        addDays: function (time, num) {
            var value = time.valueOf();
            value += 86400000 * num;
            return new Date(value);
        },
        log: function (string) {
            var d, timestr;
            d = new Date();
            timestr = timeStringSecs(d);
            timestr += ' ';
            timestr += d.getMilliseconds();
            if (window.hyper && window.hyper.log) {
                hyper.log(timestr + ': ' + string);
            }
            else {
                console.log(timestr + ': ' + string);
            }
        }
    };

}());