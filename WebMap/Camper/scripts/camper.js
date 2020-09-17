var dispValues = [];
var tempChart, solarChart;

setInterval(function () {
    // refesh every 5 minutes
    getWebData();
}, 300000);

getWebData();


function showTempChart() {
    return AmCharts.makeChart("tempchartdiv", {
        "type": "serial",
        "theme": "light",
        "marginRight": 80,
        "dataProvider": dispValues,
        "valueAxes": [{
            "position": "left",
            "title": "Temperature °C"
        }],
        "graphs": [
            {
                "id": "g1",
                "connect": false,
                "fillAlphas": 0.1,
                "valueField": "shadeT",
                "lineColor": "ffbcc6",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Shade:<b>[[value]]</b>°C</div>"
            },
            {
                "id": "g2",
                "connect": false,
                "fillAlphas": 0.1,
                "valueField": "vanT",
                "lineColor": "#FF0026",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Fridge:<b>[[value]]</b>°C</div>"
            },
            {
                "id": "g3",
                "connect": false,
                "fillAlphas": 0.1,
                "valueField": "fridgeT",
                "lineColor": "#3c66fc",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Van:<b>[[value]]</b>°C</div>"
            },
        ],
        "chartScrollbar": {
            "graph": "g1",
            "scrollbarHeight": 80,
            "backgroundAlpha": 0,
            "selectedBackgroundAlpha": 0.1,
            "selectedBackgroundColor": "#888888",
            "graphFillAlpha": 0,
            "graphLineAlpha": 0.5,
            "selectedGraphFillAlpha": 0,
            "selectedGraphLineAlpha": 1,
            "autoGridCount": true,
            "color": "#AAAAAA"
        },
        "chartCursor": {
            "categoryBalloonDateFormat": "JJ:NN, DD MMMM",
            "cursorPosition": "mouse"
        },
        "categoryField": "time",
        "categoryAxis": {
            "minPeriod": "10mm",
            "parseDates": true
        },
        "export": {
            "enabled": true,
            "dateFormat": "YYYY-MM-DD HH:NN:SS"
        }
    })
}

function showSolarChart() {
    return AmCharts.makeChart("solarchartdiv", {
        "type": "serial",
        "theme": "light",
        "marginRight": 80,
        "dataProvider": dispValues,
        "valueAxes": [
            {
                "id": "v1",
                "position": "left",
                "title": "Volts"
            },
            {
                "id": "v2",
                "position": "right",
                "title": "Watts"
            },

        ],
        "graphs": [
            {
                "id": "g4",
                "connect": false,
                "valueAxis": "v1",
                "fillAlphas": 0.4,
                "valueField": "battV",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Battery:<b>[[value]]V</b></div>"
            },
            {
                "id": "g5",
                "connect": false,
                "valueAxis": "v1",
                "fillAlphas": 0.4,
                "valueField": "panelV",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Panel:<b>[[value]]V</b></div>"
            },
            {
                "id": "g6",
                "connect": false,
                "valueAxis": "v2",
                "fillAlphas": 0.4,
                "valueField": "panelP",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Panel:<b>[[value]]W</b></div>"
            },
            {
                "id": "g7",
                "connect": false,
                "valueAxis": "v2",
                "fillAlphas": 0.4,
                "valueField": "loadC",
                "balloonText": "<div style='margin:5px; font-size:15px;'>Load:<b>[[value]]W</b></div>"
            },
        ],
        "chartScrollbar": {
            "graph": "g4",
            "scrollbarHeight": 80,
            "backgroundAlpha": 0,
            "selectedBackgroundAlpha": 0.1,
            "selectedBackgroundColor": "#888888",
            "graphFillAlpha": 0,
            "graphLineAlpha": 0.5,
            "selectedGraphFillAlpha": 0,
            "selectedGraphLineAlpha": 1,
            "autoGridCount": true,
            "color": "#AAAAAA"
        },
        "chartCursor": {
            "categoryBalloonDateFormat": "JJ:NN, DD MMMM",
            "cursorPosition": "mouse"
        },
        "categoryField": "time",
        "categoryAxis": {
            "minPeriod": "10mm",
            "parseDates": true
        },
        "export": {
            "enabled": true,
            "dateFormat": "YYYY-MM-DD HH:NN:SS"
        }
    })
}


function urlBase() {

    //return "http://www.timetrials.org.uk/Service1.svc/";
    //return "http://localhost/WebMap/WebMap.svc/";
    return "../Service1.svc/";

}

function webRequestFailed(handle, status, error) {
        alert("Error with web request: " + error);

}

function webRequestSuccess(success, res) {
        success(res);
}

function getWebData() {

    // get two days worth of 10-minutes readings (288 records)
        myJson("GetCamper", "POST", 288, function (response) {
            if (response.length === 0) {
                popup.Alert("No data found!");
                return;
            }

 
            // need to reverse records because they will be latest first.
            // Also need to convert values to decimal
            var len = response.length;
            for (index = len - 1; index >= 0; index--) {
                dispValues[index] = response[len - index - 1];

                dispValues[index]['shadeT'] = dispValues[index]['shadeT'] / 10;
                dispValues[index]['vanT'] = dispValues[index]['vanT'] / 10;
                dispValues[index]['fridgeT'] = dispValues[index]['fridgeT'] / 10;
                if (dispValues[index]['battV'] > 0) {
                    // some valid readings have been found
                    dispValues[index]['battV'] = dispValues[index]['battV'] / 1000;
                    dispValues[index]['panelV'] = dispValues[index]['panelV'] / 1000;
                    // load watts = load current * battery volts
                    dispValues[index]['loadC'] = Math.round(dispValues[index]['loadC'] / 1000 * dispValues[index]['battV']);
                }
                else if (index < len-1)
                {
                    // no valid readings, copy last reading
                    dispValues[index]['battV'] = dispValues[index+1]['battV'];
                    dispValues[index]['panelV'] = dispValues[index+1]['panelV'];
                    dispValues[index]['loadC'] = dispValues[index+1]['loadC'];

                }
                

            }
            
            tempChart = showTempChart();
            solarChart = showSolarChart();
        }, true);
}

function myJson(url, type, data, successfunc, async) {
    var dataJson = JSON.stringify(data),
        thisurl = urlBase() + url;

    $.ajax({
        type: type,
        data: dataJson,
        url: thisurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: async,
        success: function (response)
            { webRequestSuccess(successfunc, response); },
        error: webRequestFailed

        })
}


//// this method is called when chart is first inited as we listen for "dataUpdated" event
//function zoomChart() {
//    // different zoom methods can be used - zoomToIndexes, zoomToDates, zoomToCategoryValues
//    chart.zoomToIndexes(dispValues.length - 75, dispValues.length - 25);
//}