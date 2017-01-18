﻿/*global bleTime,jQuery*/

var bleData = (function ($) {

    "use strict";

    var bleData = {},

    // values downloaded from database for displaying
        dispValues = [],

    // start & end of currently displayed data
        startDate,
        endDate,

    // button to be reset when json interaction is complete
        $jsonBtn,
            
        chooseDates = function (sensor) {
            if ($('#fromDate').is(":visible")) {
                // dates have been chosen, get them and close options
                startDate = new Date($("#startDate").val());
                endDate = new Date($("#endDate").val());
                $('#fromDate').hide();
                $('#toDate').hide();
                bleData.setDateChooser('Change');
                // in case dates have been changed....
                bleData.showData();
                return;
            }

            $("#startDate").datepicker({ todayBtn: true, autoclose: true, format: "dd M yyyy" });
            $("#endDate").datepicker({ todayBtn: true, autoclose: true, format: "dd M yyyy" });
            $("#startDate").datepicker('setDate', startDate);
            $("#endDate").datepicker('setDate', endDate);
            $("#startDate").change(function () {
                startDate = new Date($("#startDate").val());
            });
            $("#endDate").change(function () {
                endDate = new Date($("#endDate").val());
            });
            //$("#chartName").html(sensor.Name);

            $('#fromDate').show();
            $('#toDate').show();
            bleData.setDateChooser('OK');
        },
       
    urlBase = function() {

            //return "http://www.quilkin.co.uk/WebMap.svc/";
            return "http://CE568/WebMap/WebMap.svc/";


    },
    webRequestFailed = function(handle, status, error) {
        popup.Alert("Error with web request: " + error);
        if ($jsonBtn !== null) {
            $jsonBtn.button('reset');
        }

    },
    webRequestSuccess = function(success, res) {
        success(res);
        if ($jsonBtn !== null) {
            $jsonBtn.button('reset');
        }

    },

    getWebData = function($btn) {

        //if (bleSensors.DisplayedSensors().length === 0) {
        //    popup.Alert("Please choose a sensor");
        //    //$btn.button('reset');
        //    return;
        //}
        var query,
            when1 = startDate,
            // get data until *end of* selected date
            when2 = bleTime.addDays(endDate, 1);

        query = new bleData.requestRecords( Math.round(when1.valueOf() / 60000), Math.round(when2.valueOf() / 60000));
        bleData.myJson("GetWeather", "POST", query, function (response) {
            if (response.length === 0) {
                popup.Alert("No data found!");
                return;
            }
            //var index = bleSensors.CurrentSensor().index;
            dispValues = response;
            //bleSensors.CurrentSensor().downloaded = true;
            bleData.CreateChart('dbChart');

        }, true, $btn);
    }

    $("#dateTitle").on('click', chooseDates);


    // global functions

    //var dataTime = new Date;
    bleData.Logdata = function (ID, time, value) {
        // short names to keep json.stringify small
        this.S = ID;
        this.T = time;
        // just a  single value in the array for uploading
        this.V = [value];
    };

    bleData.requestRecords = function ( from, to) {
        this.From = from;
        this.To = to;
    };

    bleData.ClearData = function (address) {
        //var values, sensor = bleSensors.findSensor(address);
        //if (sensor !== null && sensor !== undefined) {
        //    values = sensor.NewValues;
        //    if (values !== null && values !== undefined) {
        //        while (values.length > 0) { values.pop(); }
        //    }
        //}
        //while (tempValues1.length > 0) { tempValues1.pop(); }
        //while (ramValues.length > 0) { ramValues.pop(); }
        while (dispValues.length > 0) { dispValues.pop(); }
    };

    bleData.setDates = function (start, end) {
        startDate = start;
        endDate = end;
    };


    bleData.showData = function () {
        getWebData(null);
        //// if it's a narrow screen (i.e. mobile phone), collapse the sensor list and date chooser to make it easier to see the graph
        //if ($('#btnMenu').is(":visible")) {
        //    $('#findlist').empty();
        //    var htmlstr = '<a id="findTitle" class="list-group-item list-group-item-info">Choose sensor(s)</a>';
        //    $('#findlist').append(htmlstr);
        //    $('#findTitle').click(bleSensors.CreateSensorList);
        //    $('#fromDate').hide();
        //    $('#toDate').hide();
        //    bleData.setDateChooser('Change');
        //}
    };
    bleData.setDateChooser = function (btntext) {
        $('#dateTitle').html(bleTime.dateString(startDate) + " to " + bleTime.dateString(endDate) + '<span id="btnGo" role="button" class="btn btn-lifted  btn-info btn-sm pull-right">' + btntext + '</span>');
    };



    

    bleData.myJson = function (url, type, data, successfunc, async, $btn) {
        var dataJson = JSON.stringify(data),
            thisurl = urlBase() + url;

        $jsonBtn = $btn;

        $.ajax({
            type: type,
            data: dataJson,
            url: thisurl,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: async,
            success: function (response) { webRequestSuccess(successfunc, response); },
            error: webRequestFailed

        });
    };


    bleData.DisplayValues = function () {
        $("#tableName").html("Preparing table (" + dispValues.length + " records...)");
        // put a timeout here to enable the new html to be displayed at the correct time....
        window.setTimeout(function () {
            var d, v, val, vals,
                row,
                titles,
                valStr,
                dataArray = [];
            //var device = "";
            
            $.each(dispValues, function (index, logdata) {
                row = [];
                d = new Date(logdata.T * 60000);
                row.push(bleTime.dateString(d));
                row.push(bleTime.timeString(d));
                vals = logdata.V.length;
                for (v = 0; v < vals; v++) {
                    val = logdata.V[v];
                    if (val > -270) {
                        row.push(val);
                    }
                    else {
                        // missing data?
                        row.push('****');
                    }
                }
                dataArray.push(row);
            });
            //titles = [{ "sTitle": "Date" }, { "sTitle": "Time" }];
            //$.each(bleSensors.DisplayedSensors(), function (index, sensor) {
            //    valStr = '(' + (index + 1) + ')';
            //    titles.push({ "sTitle": valStr });
            //});
            //bleTable('#data', null, dataArray, bleApp.tableHeight(), titles, null);
            //$("#tableName").html(bleSensors.DisplayedSensorNames());
        }, 100);
    };


    bleData.CreateChart = function (div) {
        var chartData = [],
            d, v, val, vals, valname, dataPoint;

        $("#chartName").html("Preparing chart (" + dispValues.length + " records...)");
        // put a timeout here to enable the new html to be displayed at the correct time....
        window.setTimeout(function () {
            $.each(dispValues, function (index,logdata) {

                if (logdata.V < -270) {
                    // badData = true;
                    return true;
                }
                // logdata is in the form
                // {time:xx, v:[v1,v2,v3]}   (e.g. for three sets of values)
                // for the graph, need to change it to
                //  {time:xx, val1:v1, val2:v2, val3:v3}
                //
                
                d = new Date(logdata.T * 60000);
                vals = logdata.V.length;

                dataPoint = {
                    time: bleTime.dateTimeString(d),
                    minAlarm: 2,
                    maxAlarm: 8
                };
                for (v = 0; v < vals; v++) {
                    valname = "val" + (v + 2);
                    val = logdata.V[v];
                    if (val > -272) {
                        dataPoint[valname] = val;
                    }
                    //else {
                    //    dataPoint[valname] = '';
                    //}
                }
                // add object to chartData array
                chartData.push(dataPoint);

            });
            if (chartData.length < 4) {
                return null;
            }
            $("#chartName").html("Creating chart (" + dispValues.length + " records...)");
            window.setTimeout(function () {
                createAMChart(chartData, div,vals);
                //$("#chartName").html(bleSensors.DisplayedSensorNames());
            }, 100);

        }, 100);
        return chartData;
    };


    bleData.DisplayNewChart = function (sensor) {
        var name, index;
        for (index = 0; index < sensor.NewValues.length; index++) {
            dispValues[index] = sensor.NewValues[index];
        }

        //name = tagConnect.Connection().Name;
        name = sensor.Name;
        bleData.CreateChart('newChart');
        //bleData.DisplayValues();
        $("#newChartName").html(name);
        $("#newTableName").html(name);
    };

    return bleData;
}(jQuery));



