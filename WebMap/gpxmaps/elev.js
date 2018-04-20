//function drawProfile1(elevid, elev_data) {


//    var ctx = document.getElementById(elevid);
//    var myChart = new Chart(ctx, {
//        type: 'line',
//        data: {
           
//            datasets: [{
//                data: elev_data,
//                lineTension: 0,
//                backgroundColor: 'transparent',
//                borderColor: '#007bff',
//                borderWidth: 4,
//                pointBackgroundColor: '#007bff'
//            }]
//        },
//        options: {
//            scales: {
//                yAxes: [{
//                    ticks: {
//                        beginAtZero: false
//                    }
//                }]
//            },
//            legend: {
//                display: false,
//            }
//        }
//    });
//}
var chart;

function clearChart() {
    //   chart.validateData();
    chart.clear();
}
function drawProfile(elevid, elev_data) {


    chart = AmCharts.makeChart(elevid, {
        "type": "serial",
        "theme": "light",
   
        "dataProvider": elev_data,
        "valueAxes": [{
            "gridColor": "#FFFFFF",
            "gridAlpha": 0.2,
            "title": "metres",
            "maximum": 300,
            "minimum": 0,
            "dashLength": 0
        }],
        "gridAboveGraphs": true,
        "startDuration": 1,
        "graphs": [{
            "balloonText": "[[category]]km<br><b>[[value]]m</b>",
            "fillAlphas": 0.8,
            "lineAlpha": 0.2,
            "type": "line",
            "valueField": "Height"
        }],
        "chartCursor": {
            "categoryBalloonEnabled": false,
            "cursorAlpha": 0,
            "zoomable": false
        },
        "categoryField": "Distance",
        "categoryAxis": {
            "gridPosition": "start",
            "gridAlpha": 0,
            "tickPosition": "start",
            "tickLength": 20,
            "precision": 0
        },
    });


chart.addListener("rendered", zoomChart);
if (chart.zoomChart) {
    chart.zoomChart();
}

function zoomChart() {
    chart.zoomToIndexes(Math.round(chart.dataProvider.length * 0.4), Math.round(chart.dataProvider.length * 0.55));
}}


