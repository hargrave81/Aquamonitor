// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//
// Water chart
//

var WaterChart = (function () {

	//
	// Variables
	//

	var $chart = $('#chart-water');	


	//
	// Methods
	//

	// Init chart
	function initChart($chart, dataset) {

		// Create chart
		var ordersChart = new Chart($chart, {
			type: 'line',
			bezierCurve: true,
			options: {
				scales: {
					yAxes: [{
						gridLines: {
							color: Charts.colors.gray[900],
							zeroLineColor: Charts.colors.gray[900]
						},
						ticks: {
							callback: function (value, index, values) {
								if (value == 1) return "High";
								if (value == 0) return "Low";
							}
						}
					}]
				},
				tooltips: {
					callbacks: {
						label: function (item, data) {
							var label = data.datasets[item.datasetIndex].label || '';
							var yLabel = item.yLabel;
							var content = '';

							if (data.datasets.length > 1) {
								content += '<span class="popover-body-label mr-auto">' + label + '</span>';
							}

							content += '<span class="popover-body-value">' + yLabel + '</span>';
							return content;
						}
					}
				}
			},
			data: dataset
		});

		// Save to jQuery object
		$chart.data('chart', ordersChart);
	}


	// Init chart
	if ($chart.length) {		
		// fetch from url options
		var jsonData = $.ajax({
			url: $($chart).parent().parent().parent().find('[data-default]').data('update'),
			dataType: 'json',
		}).done(function (results) {						
			initChart($chart, results);
		});
	}

})();

//
// Charts
//

'use strict';

//
// Temp chart
//

var TempChart = (function () {

	// Variables

	var $chart = $('#chart-temps');


	// Methods

	function init($chart, dataset) {

		var salesChart = new Chart($chart, {
			type: 'line',
			options: {
				scales: {
					yAxes: [{
						gridLines: {
							color: Charts.colors.gray[900],
							zeroLineColor: Charts.colors.gray[900]
						}
					}]
				},
				tooltips: {
					callbacks: {
						label: function (item, data) {
							var label = data.datasets[item.datasetIndex].label || '';
							var yLabel = item.yLabel;
							var content = '';

							if (data.datasets.length > 1) {
								content += '<span class="popover-body-label mr-auto">' + label + '</span>';
							}

							content += '<span class="popover-body-value">' + yLabel + '</span>';
							return content;
						}
					}
				}
			},
			data: dataset
		});

		// Save to jQuery object

		$chart.data('chart', salesChart);

	};


	// Events

	if ($chart.length) {		
		// Init chart
		// fetch from url options
		var jsonData = $.ajax({
			url: $($chart).parent().parent().parent().find('[data-default]').data('update'),
			dataType: 'json',
		}).done(function (results) {						
			init($chart, results);
		});		
	}

})();




var RelayChart = (function () {

    // Variables

    var $chart = $('#chart-relays');


    // Methods

    function init($chart, dataset) {

        var relayChart = new Chart($chart, {
            type: 'line',
            options: {
                scales: {
                    yAxes: [{
                        gridLines: {
                            color: Charts.colors.gray[900],
                            zeroLineColor: Charts.colors.gray[900]
						},
                        ticks: {
                            callback: function (value, index, values) {
                                if (value == 1) return "On";
                                if (value == 0) return "Off";
                            }
                        }
                    }]
                },
                tooltips: {
                    callbacks: {
                        label: function (item, data) {
                            var label = data.datasets[item.datasetIndex].label || '';
                            var yLabel = item.yLabel;
                            var content = '';

                            if (data.datasets.length > 1) {
                                content += '<span class="popover-body-label mr-auto">' + label + '</span>';
                            }

                            content += '<span class="popover-body-value">' + yLabel + '</span>';
                            return content;
                        }
                    }
                }
            },
            data: dataset
        });

        // Save to jQuery object

        $chart.data('chart', relayChart);

    };


    // Events

    if ($chart.length) {
        // Init chart
        // fetch from url options
        var jsonData = $.ajax({
            url: $($chart).parent().parent().parent().find('[data-default]').data('update'),
            dataType: 'json',
        }).done(function (results) {
            init($chart, results);
        });
    }

})();



//
// Outside Temperature chart
//

var TempOutChart = (function () {

    // Variables

    var $chart = $('#chart-outside');


    // Methods

    function init($chart, dataset) {

        var outTempChart = new Chart($chart, {
            type: 'line',
            options: {
                scales: {
                    yAxes: [{
                        gridLines: {
                            color: Charts.colors.gray[900],
                            zeroLineColor: Charts.colors.gray[900]
                        }
                    }]
                },
                tooltips: {
                    callbacks: {
                        label: function (item, data) {
                            var label = data.datasets[item.datasetIndex].label || '';
                            var yLabel = item.yLabel;
                            var content = '';

                            if (data.datasets.length > 1) {
                                content += '<span class="popover-body-label mr-auto">' + label + '</span>';
                            }

                            content += '<span class="popover-body-value">' + yLabel + '</span>';
                            return content;
                        }
                    }
                }
            },
            data: dataset
        });

        // Save to jQuery object

		$chart.data('chart', outTempChart);

    };


    // Events

    if ($chart.length) {
        // Init chart
        // fetch from url options
        var jsonData = $.ajax({
            url: $($chart).parent().parent().parent().find('[data-default]').data('update'),
            dataType: 'json',
        }).done(function (results) {
            init($chart, results);
        });
    }

})();


