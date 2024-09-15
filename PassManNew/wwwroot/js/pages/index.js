

//JS CHART
$(function () {
    if ($('#bar_chart1').length > 0) { new Chart(document.getElementById("bar_chart1").getContext("2d"), getChartJs('bar1')); }   
    if ($('#bar_chart2').length > 0) { new Chart(document.getElementById("bar_chart2").getContext("2d"), getChartJs('bar2')); }
    if ($('#bar_chart3').length > 0) { new Chart(document.getElementById("bar_chart3").getContext("2d"), getChartJs('bar3')); }
    if ($('#bar_chart4').length > 0) { new Chart(document.getElementById("bar_chart4").getContext("2d"), getChartJs('bar2')); }
    if ($('#bar_chart5').length > 0) { new Chart(document.getElementById("bar_chart5").getContext("2d"), getChartJs('bar5')); }
    if ($('#bar_chart6').length > 0) { new Chart(document.getElementById("bar_chart6").getContext("2d"), getChartJs('bar6')); }
    if ($('#bar_chart7').length > 0) { new Chart(document.getElementById("bar_chart7").getContext("2d"), getChartJs('bar7')); }

    if ($('#line_chart1').length > 0) { new Chart(document.getElementById("line_chart1").getContext("2d"), getChartJs('line1')); }

    //new Chart(document.getElementById("radar_chart").getContext("2d"), getChartJs('radar'));
    //new Chart(document.getElementById("pie_chart").getContext("2d"), getChartJs('pie'));
});

function getChartJs(type) {
    var config = null;

    if (type === 'line1') {
        config = {
            type: 'line',
            data: {
                labels: linechart1labels,
                datasets: [{
                    label: "SIM",
                    data: linechart1data1,
                    borderColor: 'rgba(0, 188, 212, 0.75)',
                    backgroundColor: 'rgba(0, 188, 212, 0.3)',
                    pointBorderColor: 'rgba(0, 188, 212, 0)',
                    pointBackgroundColor: 'rgba(0, 188, 212, 0.9)',
                    pointBorderWidth: 1
                },
                {
                    label: "Recharged",
                    data: linechart1data2,
                    borderColor: 'rgba(233, 30, 99, 0.75)',
                    backgroundColor: 'rgba(233, 30, 99, 0.3)',
                    pointBorderColor: 'rgba(233, 30, 99, 0)',
                    pointBackgroundColor: 'rgba(233, 30, 99, 0.9)',
                    pointBorderWidth: 1
                    },
                 {
                        label: "Packages",
                        data: linechart1data3,
                        borderColor: 'rgba(255, 193, 7, 0.75)',
                        backgroundColor: 'rgba(255, 193, 7, 0.3)',
                        pointBorderColor: 'rgba(255, 193, 7, 0)',
                        pointBackgroundColor: 'rgba(255, 30, 7, 0.9)',
                        pointBorderWidth: 1
                    },
                 {
                        label: "Activation",
                        data: linechart1data4,
                     borderColor: 'rgba(139, 195, 74, 0.75)',
                     backgroundColor: 'rgba(139, 195, 74, 0.3)',
                     pointBorderColor: 'rgba(139, 195, 74, 0)',
                     pointBackgroundColor: 'rgba(139, 195, 74, 0.9)',
                        pointBorderWidth: 1
                    }]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    else if (type === 'bar1') {
      
        config = {
            type: 'bar',
            data: {
                labels: barchart1users,
                datasets: [{
                    label: "Collection",
                    data: barchart1data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }, {
                        label: "Transfered",
                    data: barchart1data2,
                    backgroundColor: 'rgba(233, 30, 99, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    else if (type === 'bar2') {

        config = {
            type: 'bar',
            data: {
                labels: barchart2users,
                datasets: [{
                    label: "Collection",
                    data: barchart2data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }, {
                    label: "Transfered",
                    data: barchart2data2,
                    backgroundColor: 'rgba(233, 30, 99, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    else if (type === 'bar3') {

        config = {
            type: 'bar',
            data: {
                labels: barchart3users,
                datasets: [{
                    label: "Collection",
                    data: barchart3data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }, {
                    label: "Transfered",
                    data: barchart3data2,
                    backgroundColor: 'rgba(233, 30, 99, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    else if (type === 'bar5') {

        config = {
            type: 'bar',
            data: {
                labels: barchart5users,
                datasets: [{
                    label: "Sales",
                    data: barchart5data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }, {
                    label: "Target",
                    data: barchart5data2,
                    backgroundColor: 'rgba(233, 30, 99, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: true
            }
        }
    }
    else if (type === 'bar6') {

        config = {
            type: 'bar',
            data: {
                labels: barchart6users,
                datasets: [{
                    label: "Sales",
                    data: barchart6data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: true
            }
        }
    }
    else if (type === 'bar7') {

        config = {
            type: 'bar',
            data: {
                labels: barchart7users,
                datasets: [{
                    label: "Sales",
                    data: barchart7data1,
                    backgroundColor: 'rgba(0, 188, 212, 0.8)'
                }, {
                    label: "Target",
                    data: barchart7data2,
                    backgroundColor: 'rgba(233, 30, 99, 0.8)'
                }]
            },
            options: {
                responsive: true,
                legend: true
            }
        }
    }
    else if (type === 'radar') {
        config = {
            type: 'radar',
            data: {
                labels: ["January", "February", "March", "April", "May", "June", "July"],
                datasets: [{
                    label: "My First dataset",
                    data: [65, 25, 90, 81, 56, 55, 40],
                    borderColor: 'rgba(0, 188, 212, 0.8)',
                    backgroundColor: 'rgba(0, 188, 212, 0.5)',
                    pointBorderColor: 'rgba(0, 188, 212, 0)',
                    pointBackgroundColor: 'rgba(0, 188, 212, 0.8)',
                    pointBorderWidth: 1
                }, {
                    label: "My Second dataset",
                    data: [72, 48, 40, 19, 96, 27, 100],
                    borderColor: 'rgba(233, 30, 99, 0.8)',
                    backgroundColor: 'rgba(233, 30, 99, 0.5)',
                    pointBorderColor: 'rgba(233, 30, 99, 0)',
                    pointBackgroundColor: 'rgba(233, 30, 99, 0.8)',
                    pointBorderWidth: 1
                }]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    else if (type === 'pie') {
        config = {
            type: 'pie',
            data: {
                datasets: [{
                    data: [225, 50, 100, 40],
                    backgroundColor: [
                        "rgb(233, 30, 99)",
                        "rgb(255, 193, 7)",
                        "rgb(0, 188, 212)",
                        "rgb(139, 195, 74)"
                    ],
                }],
                labels: [
                    "Pink",
                    "Amber",
                    "Cyan",
                    "Light Green"
                ]
            },
            options: {
                responsive: true,
                legend: false
            }
        }
    }
    return config;
}





// Jquery KNOB
$(function () {
    $('.knob').knob({
        draw: function () {
            // "tron" case
            if (this.$.data('skin') === 'tron') {

                var a = this.angle(this.cv)  // Angle
                    , sa = this.startAngle          // Previous start angle
                    , sat = this.startAngle         // Start angle
                    , ea                            // Previous end angle
                    , eat = sat + a                 // End angle
                    , r = true;

                this.g.lineWidth = this.lineWidth;

                this.o.cursor
                    && (sat = eat - 0.3)
                    && (eat = eat + 0.3);

                if (this.o.displayPrevious) {
                    ea = this.startAngle + this.angle(this.value);
                    this.o.cursor
                        && (sa = ea - 0.3)
                        && (ea = ea + 0.3);
                    this.g.beginPath();
                    this.g.strokeStyle = this.previousColor;
                    this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, sa, ea, false);
                    this.g.stroke();
                }

                this.g.beginPath();
                this.g.strokeStyle = r ? this.o.fgColor : this.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, sat, eat, false);
                this.g.stroke();

                this.g.lineWidth = 2;
                this.g.beginPath();
                this.g.strokeStyle = this.o.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth + 1 + this.lineWidth * 2 / 3, 0, 2 * Math.PI, false);
                this.g.stroke();

                return false;
            }
        }
    });
});


//mORRIS CHART
$(function () {
 //   getMorris('line', 'line_chart');
 //   getMorris('bar', 'bar_chart');
    if ($('#area_chart1').length > 0) { getMorris('area', 'area_chart1'); }   
 //   getMorris('donut', 'donut_chart');
});

function getMorris(type, element) {
    if (type === 'line') {
        Morris.Line({
            element: element,
            data: [{
                'period': '2011 Q3',
                'licensed': 3407,
                'sorned': 660
            }, {
                'period': '2011 Q2',
                'licensed': 3351,
                'sorned': 629
            }, {
                'period': '2011 Q1',
                'licensed': 3269,
                'sorned': 618
            }, {
                'period': '2010 Q4',
                'licensed': 3246,
                'sorned': 661
            }, {
                'period': '2009 Q4',
                'licensed': 3171,
                'sorned': 676
            }, {
                'period': '2008 Q4',
                'licensed': 3155,
                'sorned': 681
            }, {
                'period': '2007 Q4',
                'licensed': 3226,
                'sorned': 620
            }, {
                'period': '2006 Q4',
                'licensed': 3245,
                'sorned': null
            }, {
                'period': '2005 Q4',
                'licensed': 3289,
                'sorned': null
            }],
            xkey: 'period',
            ykeys: ['licensed', 'sorned'],
            labels: ['Licensed', 'Off the road'],
            lineColors: ['rgb(233, 30, 99)', 'rgb(0, 188, 212)'],
            lineWidth: 3
        });
    } else if (type === 'bar') {
        Morris.Bar({
            element: element,
            data: [{
                x: '2011 Q1',
                y: 3,
                z: 2,
                a: 3
            }, {
                x: '2011 Q2',
                y: 2,
                z: null,
                a: 1
            }, {
                x: '2011 Q3',
                y: 0,
                z: 2,
                a: 4
            }, {
                x: '2011 Q4',
                y: 2,
                z: 4,
                a: 3
            }],
            xkey: 'x',
            ykeys: ['y', 'z', 'a'],
            labels: ['Y', 'Z', 'A'],
            barColors: ['rgb(233, 30, 99)', 'rgb(0, 188, 212)', 'rgb(0, 150, 136)'],
        });
    } else if (type === 'area') {
        Morris.Area({
            element: element,
            data: area_char1_data,
            xkey: 'BalanceDate',
            ykeys: ['Balance'],
            labels: ['Balance'],
            pointSize: 2,
            hideHover: 'auto',
            lineColors: ['rgb(233, 30, 99)']
        });
    }
    else if (type === 'donut') {
        Morris.Donut({
            element: element,
            data: [{
                label: 'Jam',
                value: 25
            }, {
                label: 'Frosted',
                value: 40
            }, {
                label: 'Custard',
                value: 25
            }, {
                label: 'Sugar',
                value: 10
            }],
            colors: ['rgb(233, 30, 99)', 'rgb(0, 188, 212)', 'rgb(255, 152, 0)', 'rgb(0, 150, 136)'],
            formatter: function (y) {
                return y + '%'
            }
        });
    }
}




$(function () {
    //Widgets count
    $('.count-to').countTo();

    //Sales count to
    $('.sales-count-to').countTo({
        formatter: function (value, options) {
            return '$' + value.toFixed(2).replace(/(\d)(?=(\d\d\d)+(?!\d))/g, ' ').replace('.', ',');
        }
    });

    // initRealTimeChart();
    initDonutChart();
    //  initSparkline();
});

var realtime = 'on';
function initRealTimeChart() {
    //Real time ==========================================================================================
    var plot = $.plot('#real_time_chart', [getRandomData()], {
        series: {
            shadowSize: 0,
            color: 'rgb(0, 188, 212)'
        },
        grid: {
            borderColor: '#f3f3f3',
            borderWidth: 1,
            tickColor: '#f3f3f3'
        },
        lines: {
            fill: true
        },
        yaxis: {
            min: 0,
            max: 100
        },
        xaxis: {
            min: 0,
            max: 100
        }
    });

    function updateRealTime() {
        plot.setData([getRandomData()]);
        plot.draw();

        var timeout;
        if (realtime === 'on') {
            timeout = setTimeout(updateRealTime, 320);
        } else {
            clearTimeout(timeout);
        }
    }

    updateRealTime();

    $('#realtime').on('change', function () {
        realtime = this.checked ? 'on' : 'off';
        updateRealTime();
    });
    //====================================================================================================
}

function initSparkline() {
    $(".sparkline").each(function () {
        var $this = $(this);
        $this.sparkline('html', $this.data());
    });
}

function initDonutChart() {



    if ($('#donut_chart2').length > 0) {
        Morris.Donut({
            element: 'donut_chart2',
            data: donut_chart2_data,
            //colors: ['rgb(233, 30, 99)', 'rgb(0, 188, 212)', 'rgb(255, 152, 0)', 'rgb(0, 150, 136)', 'rgb(96, 125, 139)'],
            formatter: function (y) {
                return y
            }
        });
    }

    if ($('#donut_chart1').length > 0) {
        Morris.Donut({
            element: 'donut_chart1',
            data: donut_chart1_data,
            colors: ['rgb(233, 30, 99)', 'rgb(0, 188, 212)', 'rgb(255, 152, 0)', 'rgb(0, 150, 136)', 'rgb(96, 125, 139)'],
            formatter: function (y) {
                return y + ' SAR'
            }
        });
    }
}

var data = [], totalPoints = 110;
function getRandomData() {
    if (data.length > 0) data = data.slice(1);

    while (data.length < totalPoints) {
        var prev = data.length > 0 ? data[data.length - 1] : 50, y = prev + Math.random() * 10 - 5;
        if (y < 0) { y = 0; } else if (y > 100) { y = 100; }

        data.push(y);
    }

    var res = [];
    for (var i = 0; i < data.length; ++i) {
        res.push([i, data[i]]);
    }

    return res;
}


