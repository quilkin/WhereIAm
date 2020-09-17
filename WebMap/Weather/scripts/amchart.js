
// http://www.amcharts.com/javascript-charts/
/*global $,AmCharts,weatherApp*/


var chart = AmCharts.makeChart("chartdiv", {
    "type": "serial",
    "theme": "light",
    "marginRight": 80,
    "dataProvider": dispValues,
    "valueAxes": [{
        "position": "left",
        "title": "Temperature"
    }],
    "graphs": [{
        "id": "g1",
        "fillAlphas": 0.4,
        "valueField": "shadeT",
        "balloonText": "<div style='margin:5px; font-size:19px;'>Temp:<b>[[value]]</b></div>"
    }],
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
        "minPeriod": "mm",
        "parseDates": true
    },
    "export": {
        "enabled": true,
        "dateFormat": "YYYY-MM-DD HH:NN:SS"
    }
});


chart.addListener("dataUpdated", zoomChart);
// when we apply theme, the dataUpdated event is fired even before we add listener, so
// we need to call zoomChart here
zoomChart();
// this method is called when chart is first inited as we listen for "dataUpdated" event
function zoomChart() {
    // different zoom methods can be used - zoomToIndexes, zoomToDates, zoomToCategoryValues
    chart.zoomToIndexes(dispValues.length - 75, dispValues.length - 25);
}

//function createAMChart(chartData, div, vals) {

//    "use strict";

//    var chart = new AmCharts.AmSerialChart(),
//        categoryAxis = chart.categoryAxis,
//        graphs = [],
//        datalines=0, 
//        dataline,
//        datasample,
//        lineName,
//        newHtml,prop ;

//    datalines = vals + 2; // add the alarm lines
//    for (dataline = 0; dataline < datalines; dataline++) {
//            graphs[dataline] = new AmCharts.AmGraph();
//    }
//    chart.dataProvider = chartData;
//    chart.pathToImages = "../www/images/";
//    chart.dataDateFormat = "YYYY-MM-DD JJ:NN";
//    chart.valueAxes = [
//        {
//            "id":"val",
//            "axisAlpha": 0,
//            "position": "left"
//            //"title": "value"
//        },
//        {
//            "id": "alarma",
//            "axisAlpha": 0,
//            "position": "right"
//            //"title": "alarms"
//        }
//    ];
//    chart.categoryField ="time";
//    chart.chartScrollbar= {
//        "graph": "g1",
//        "scrollbarHeight": 30
//    };
//    chart.legend =  {
//            useGraphSettings: true,
//    }
        
    
//    chart.chartCursor= {
//        "cursorPosition": "mouse",
//        "pan": true,
//        "valueLineEnabled": true,
//        "valueLineBalloonEnabled": true,
//        "categoryBalloonDateFormat": "MM-DD JJ:NN"

//    };

//    chart.addListener('rollOverGraph', function (e) {
//        //rollOverGraphItem
//        var graph = e.graph;
//        lineName = graph.title;

//    });

 
//    chart.exportConfig = {
//        menuRight: '20px',
//        menuBottom: '50px',
//        menuItems: [{
//            icon: '../www/images/export.png',
//            format: 'png'
//        }]  
//    };  
     
//    categoryAxis.parseDates = true;
//    categoryAxis.dashLength = 1;
//    categoryAxis.minorGridEnabled = true;
//    categoryAxis.position = "top";
//    categoryAxis.minPeriod = "mm";
  
//    // add max/min lines
//    //graphs[0].id = "g2";
//    graphs[0].lineColor = "#0000FF";
//    graphs[0].valueField = "minAlarm";
//    graphs[0].visibleInLegend = false;
//    graphs[0].bullet = "round";
//    graphs[0].bulletBorderAlpha = 1;
//    graphs[0].bulletBorderThickness = 1;
//    graphs[0].hideBulletsCount = 30;
//    graphs[0].fillAlphas = 0;
//    //chart.addGraph(graphs[0]);

//    //graphs[1].id = "g3";
//    graphs[1].lineColor = "#FF0000";
//    graphs[1].valueField = "maxAlarm";
//    graphs[1].visibleInLegend = false;
//    graphs[1].bullet = "round";
//    graphs[1].bulletBorderAlpha = 1;
//    graphs[1].bulletBorderThickness = 1;
//    graphs[1].hideBulletsCount = 30;
//    graphs[1].fillAlphas = 0.1;
//    graphs[1].fillToGraph = graphs[0];
//    graphs[1].fillColors = "#00FF00";
//    //chart.addGraph(graphs[1]);

//    // add temperature values
//    for (dataline=2; dataline < datalines; dataline++) {
//        prop = "val" + dataline;
//        //graphs[dataline].id = prop;
//        graphs[dataline].connect = false;
//        graphs[dataline].bullet = "round";
//        graphs[dataline].bulletBorderAlpha = 1;
//        graphs[dataline].bulletBorderThickness = 1;
//       // graphs[dataline].bulletColor = "#FFFFFF";
//       // graphs[dataline].bulletSize = 5;
//        graphs[dataline].hideBulletsCount = 30;
//        graphs[dataline].lineThickness = 2;
//       // graphs[dataline].title = bleSensors.DisplayedSensors()[dataline - 2].Name;
//        graphs[dataline].useLineColorForBulletBorder = true;
//        graphs[dataline].valueField = prop;
//        chart.addGraph(graphs[dataline]);

//    }
//    chart.addGraph(graphs[0]);
//    chart.addGraph(graphs[1]);
//    //graphs[dataline].includeInMinMax = false;

//    //// convert the div to provide the correct table height:
//    //newHtml = '<div data-role="content" style="width:95%; height: ' + bleApp.tableHeight() + 'px;" id="' + div + '"></div>';
//    ////var divToChange = 'div.' + div;
//    //$('div.' + div).html(newHtml);
//    chart.write(div);

//}

