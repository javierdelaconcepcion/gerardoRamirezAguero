/* All functions in this file are used only for charts.html */
var D3Charts = function () {



    // Init Flot Charts Plugin
    var runD3Charts = function () {

       // Add a series of colors to be used in the charts and pie graphs
        var Colors = [bgPrimary, bgInfo, bgWarning, bgAlert, bgDanger, bgSystem, bgSuccess,];

        // Area Chart
        var chart2 = c3.generate({
            bindto: '#area-chart',
            color: {
              pattern: Colors,
            },
            padding: {
            left: 30,
              right: 15,
              top: 0,
              bottom: 0,
           },
            data: {
                columns: [
                    ['data1', 300, 350, 300, 350, 0, 200],
                    ['data2', 130, 100, 140, 200, 150, 50]
                ],
                types: {
                    data1: 'area',
                    data2: 'area-spline'
                }
            }
        });



        setTimeout(function () {
            chart4.load({
                columns: [
                    ['data3', 130, -150, 200, 300, -200, 100]
                ]
            });
        }, 1000);
    };
    return {
        init: function () {
			
        	runD3Charts();
        }
    };
}();